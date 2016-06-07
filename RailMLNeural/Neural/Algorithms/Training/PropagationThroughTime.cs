
using System;
using System.Threading.Tasks;
using Encog.Engine.Network.Activation;
using Encog.MathUtil;
using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Train;
using Encog.Neural.Flat;
using Encog.Util;
using Encog.Util.Logging;
using Encog.Neural.Error;
using Encog.Util.Concurrency;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Propagation;
using Encog;
using System.Collections.Generic;
using System.Linq;

namespace RailMLNeural.Neural.Algorithms.Training
{
    /// <summary>
    /// Implements basic functionality that is needed by each of the propagation
    /// methods. The specifics of each of the propagation methods is implemented
    /// inside of the PropagationMethod interface implementors.
    /// </summary>
    ///
    public abstract class PropagationThroughTime : BasicTraining, ITrain, IMultiThreadable, IBatchSize, IWeightNormalized
    {
        /// <summary>
        /// The network in indexable form.
        /// </summary>
        ///
        private readonly IList<IMLDataSet> _indexable;

        /// <summary>
        /// The last gradients, from the last training iteration.
        /// </summary>
        ///
        private readonly double[] _lastGradient;

        /// <summary>
        /// The network to train.
        /// </summary>
        private IContainsFlat _network;

        /// <summary>
        /// The network to train.
        /// </summary>
        ///
        private readonly FlatNetwork _flat;

        /// <summary>
        /// The training data.
        /// </summary>
        ///
        private readonly IList<IMLDataSet> _training;

        /// <summary>
        /// The gradients.
        /// </summary>
        ///
        protected internal double[] Gradients;

        /// <summary>
        /// The iteration.
        /// </summary>
        ///
        private int _iteration;

        /// <summary>
        /// The number of threads to use.
        /// </summary>
        ///
        private int _numThreads;

        /// <summary>
        /// Reported exception from the threads.
        /// </summary>
        ///
        private Exception _reportedException;

        /// <summary>
        /// The total error. Used to take the average of.
        /// </summary>
        ///
        private double _totalError;

        /// <summary>
        /// The workers.
        /// </summary>
        ///
        private BPTTGradientWorker[] _workers;

        /// <summary>
        /// True (default) if we should fix flatspots on supported activation functions.
        /// </summary>
        public bool FixFlatSpot { get; set; }

        /// <summary>
        /// The flat spot constants.
        /// </summary>
        private double[] _flatSpot;

        /// <summary>
        /// The error function.
        /// </summary>
        public IErrorFunction ErrorFunction { get; set; }

        public double MaxGradient { get; set; }

        /// <summary>
        /// Construct a propagation object.
        /// </summary>
        ///
        /// <param name="network">The network.</param>
        /// <param name="training">The training set.</param>
        protected PropagationThroughTime(IContainsFlat network, IList<IMLDataSet> training)
            : base(TrainingImplementationType.Iterative)
        {
            _network = network;
            _flat = network.Flat;
            _training = training.Where(x => x.Count > 0).ToList();

            Gradients = new double[_flat.Weights.Length];
            _lastGradient = new double[_flat.Weights.Length];

            _indexable = _training;
            _numThreads = 0;
            _reportedException = null;
            FixFlatSpot = true;
            ErrorFunction = new LinearErrorFunction();
        }

        /// <summary>
        /// Set the number of threads. Specify zero to tell Encog to automatically
        /// determine the best number of threads for the processor. If OpenCL is used
        /// as the target device, then this value is not used.
        /// </summary>
        public int ThreadCount
        {
            get { return _numThreads; }
            set { _numThreads = value; }
        }

        /// <summary>
        /// Increase the iteration count by one.
        /// </summary>
        public void RollIteration()
        {
            _iteration++;
        }

        #region Train Members



        /// <inheritdoc/>
        public override IMLMethod Method
        {
            get { return _network; }
        }

        /// <summary>
        /// Perform the specified number of training iterations. This can be more
        /// efficient than single training iterations. This is particularly true if
        /// you are training with a GPU.
        /// </summary>        
        public override void Iteration()
        {
            try
            {
                PreIteration();

                RollIteration();

                if (BatchSize == 0)
                {
                    ProcessPureBatch();
                }
                else
                {
                    ProcessBatches();
                }


                foreach (BPTTGradientWorker worker in _workers)
                {
                    EngineArray.ArrayCopy(_flat.Weights, 0,
                                            worker.Weights, 0, _flat.Weights.Length);
                }

                if (_flat.HasContext)
                {
                    CopyContexts();
                }

                if (_reportedException != null)
                {
                    throw (new EncogError(_reportedException));
                }

                PostIteration();

                EncogLogging.Log(EncogLogging.LevelInfo,
                                    "Training iterations done, error: " + Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                EncogValidate.ValidateNetworkForTraining(_network,
                                                            Training);
                throw new EncogError(ex);
            }
        }

        /// <value>The gradients from the last iteration;</value>
        public double[] LastGradient
        {
            get { return _lastGradient; }
        }

        #region TrainFlatNetwork Members

        /// <inheritdoc/>
        public virtual void FinishTraining()
        {
            // nothing to do
        }

        /// <inheritdoc/>
        public override int IterationNumber
        {
            get { return _iteration; }
            set { _iteration = value; }
        }


        /// <inheritdoc/>
        public IContainsFlat Network
        {
            get { return _network; }
        }


        /// <inheritdoc/>
        public int NumThreads
        {
            get { return _numThreads; }
            set { _numThreads = value; }
        }


        /// <inheritdoc/>
        public new IMLDataSet Training
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        /// <summary>
        /// Calculate the gradients.
        /// </summary>
        ///
        public virtual void CalculateGradients()
        {
            if (_workers == null)
            {
                Init();
            }

            if (_flat.HasContext)
            {
                _workers[0].Network.ClearContext();
            }

            _totalError = 0;

            Parallel.ForEach(_workers, worker => worker.Run());


            Error = _totalError / _workers.Length;
        }

        /// <summary>
        /// Copy the contexts to keep them consistent with multithreaded training.
        /// </summary>
        ///
        private void CopyContexts()
        {
            // copy the contexts(layer outputO from each group to the next group
            for (int i = 0; i < (_workers.Length - 1); i++)
            {
                double[] src = _workers[i].Network.LayerOutput;
                double[] dst = _workers[i + 1].Network.LayerOutput;
                EngineArray.ArrayCopy(src, dst);
            }

            // copy the contexts from the final group to the real network
            EngineArray.ArrayCopy(_workers[_workers.Length - 1].Network.LayerOutput, _flat.LayerOutput);
        }

        /// <summary>
        /// Init the process.
        /// </summary>
        ///
        private void Init()
        {
            // fix flat spot, if needed
            _flatSpot = new double[_flat.ActivationFunctions.Length];

            if (FixFlatSpot)
            {
                for (int i = 0; i < _flat.ActivationFunctions.Length; i++)
                {
                    IActivationFunction af = _flat.ActivationFunctions[i];
                    if (af is ActivationSigmoid)
                    {
                        _flatSpot[i] = 0.1;
                    }
                    else
                    {
                        _flatSpot[i] = 0.0;
                    }
                }
            }
            else
            {
                EngineArray.Fill(_flatSpot, 0);
            }


            var determine = new RailMLNeural.Neural.Algorithms.DetermineWorkload(
                _numThreads, (int)_indexable.Count);

            _workers = new BPTTGradientWorker[determine.ThreadCount];

            int index = 0;


            // handle CPU
            foreach (IntRange r in determine.CalculateWorkers())
            {
                _workers[index++] = new BPTTGradientWorker(((FlatNetwork)_network.Flat.Clone()),
                                                            this, _indexable, r.Low,
                                                            r.High, _flatSpot, ErrorFunction, DropoutPercentage);
            }

            InitOthers();
        }

        /// <summary>
        /// Apply and learn.
        /// </summary>
        ///
        protected internal void Learn()
        {
            double[] weights = _flat.Weights;
            for (int i = 0; i < Gradients.Length; i++)
            {
                if (MaxGradient > 0 && Math.Abs(Gradients[i]) > MaxGradient)
                {
                    Gradients[i] = MaxGradient * (Math.Abs(Gradients[i]) / Gradients[i]);
                }
                weights[i] += UpdateWeight(Gradients, _lastGradient, i);
                Gradients[i] = 0;
            }
            //HandleReLu();
            NormalizeWeights();
        }



        /// <summary>
        /// Apply and learn. This is the same as learn, but it checks to see if any
        /// of the weights are below the limit threshold. In this case, these weights
        /// are zeroed out. Having two methods allows the regular learn method, which
        /// is what is usually use, to be as fast as possible.
        /// </summary>
        ///
        protected internal void LearnLimited()
        {
            double limit = _flat.ConnectionLimit;
            double[] weights = _flat.Weights;
            for (int i = 0; i < Gradients.Length; i++)
            {
                if (Math.Abs(weights[i]) < limit)
                {
                    weights[i] = 0;
                }
                if (MaxGradient > 0 && Math.Abs(Gradients[i]) > MaxGradient)
                {
                    Gradients[i] = MaxGradient * (Math.Abs(Gradients[i]) / Gradients[i]);
                }
                weights[i] += UpdateWeight(Gradients, _lastGradient, i);
                Gradients[i] = 0;
            }
            //HandleReLu();
            NormalizeWeights();
        }

        private void NormalizeWeights()
        {
            if(MaxWeightNorm == 0)
            {
                return;
            }
            for(int i = 1; i < _flat.LayerCounts.Length - 1; i++)
            {
                int inputIndex = _flat.LayerIndex[i+1];
                int outputIndex = _flat.LayerIndex[i];
                int inputSize = _flat.LayerCounts[i+1];
                int outputSize = _flat.LayerFeedCounts[i];

                int index = _flat.WeightIndex[i];

                int limitX = outputIndex + outputSize;
                int limitY = inputIndex + inputSize;

                // weight values
                for (int x = outputIndex; x < limitX; x++)
                {
                    double sum = 0;
                    int j = index;
                    for (int y = inputIndex; y < limitY; y++)
                    {
                        sum += Math.Abs(_flat.Weights[j++]);
                    }
                    if (sum > MaxWeightNorm)
                    {
                        j = index;
                        for (int y = inputIndex; y < limitY; y++)
                        {
                            _flat.Weights[j++] *= MaxWeightNorm / sum;
                        }
                        index += inputSize;
                    }
                }
            }
        }

        private void HandleReLu()
        {
            if(_flat.ActivationFunctions.Any(x => x is ActivationReLu || x is ActivationSoftPlus))
            {
                for(int i = 0; i < _flat.LayerCounts.Length - 1; i++)
                {
                    if(_flat.ActivationFunctions[i] is ActivationReLu || _flat.ActivationFunctions[i] is ActivationSoftPlus)
                    {
                        //int index = _flat.WeightIndex[i];
                        //int c = _flat.LayerCounts[i + 1];
                        //for (int j = 0; j < _flat.LayerFeedCounts[i]; j++)
                        //{
                        //    double sum = 0;
                        //    for (int k = index; k < index + c; k++)
                        //    {
                        //        sum += _flat.Weights[k];
                        //    }
                        //    for (int k = index; k < index + c; k++)
                        //    {
                        //        _flat.Weights[k] /= sum;
                        //    }
                        //    index += c;
                        //}
                        int inputIndex = _flat.LayerIndex[i+1];
                        int outputIndex = _flat.LayerIndex[i];
                        int inputSize = _flat.LayerCounts[i+1];
                        int outputSize = _flat.LayerFeedCounts[i];

                        int index = _flat.WeightIndex[i];

                        int limitX = outputIndex + outputSize;
                        int limitY = inputIndex + inputSize;

                        // weight values
                        for (int x = outputIndex; x < limitX; x++)
                        {
                            double sum = 0;
                            int j = index;
                            for (int y = inputIndex; y < limitY; y++)
                            {
                                sum += _flat.Weights[j++];
                            }
                            j = index;
                            for (int y = inputIndex; y < limitY; y++)
                            {
                                _flat.Weights[j++] /= sum;
                            }
                            index += inputSize;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called by the worker threads to report the progress at each step.
        /// </summary>
        ///
        /// <param name="gradients">The gradients from that worker.</param>
        /// <param name="error">The error for that worker.</param>
        /// <param name="ex">The exception.</param>
        public void Report(double[] gradients, double error,
                            Exception ex)
        {
            lock (this)
            {
                if (ex == null)
                {
                    for (int i = 0; i < gradients.Length; i++)
                    {
                        Gradients[i] += gradients[i];
                    }
                    _totalError += error;
                }
                else
                {
                    _reportedException = ex;
                }
            }
        }

        /// <summary>
        /// Update a weight, the means by which weights are updated vary depending on
        /// the training.
        /// </summary>
        ///
        /// <param name="gradients">The gradients.</param>
        /// <param name="lastGradient">The last gradients.</param>
        /// <param name="index">The index.</param>
        /// <returns>The update value.</returns>
        public abstract double UpdateWeight(double[] gradients,
                                            double[] lastGradient, int index);

        /// <summary>
        /// Allow other training methods to init.
        /// </summary>
        public abstract void InitOthers();


        #endregion

        /// <summary>
        /// Process as pure batch (size 0). Batch size equal to training set size.
        /// </summary>
        private void ProcessPureBatch()
        {
            CalculateGradients();

            if (_flat.Limited)
            {
                LearnLimited();
            }
            else
            {
                Learn();
            }
        }

        private void ProcessBatches()
        {
            var sorted = _indexable.OrderBy(a => Guid.NewGuid()).ToList();
            _indexable.Clear();
            ((List<IMLDataSet>)_indexable).AddRange(sorted);
            DetermineWorkload determineLoad = new DetermineWorkload(ThreadCount, BatchSize);
            var ranges = determineLoad.CalculateWorkers();
            if (_workers == null)
            {
                Init();
            }

            if (_flat.HasContext)
            {
                _workers[0].Network.ClearContext();
            }

            Parallel.ForEach(_workers, x =>  x.Reset() );

            int lastLearn = 0;

            for (int i = 0; i < _training.Count - 1; i += BatchSize)
            {
                Parallel.For(0, ranges.Count, (j) =>
                {
                    _workers[j].Run(ranges[j].Low + i< _training.Count - 1 ? ranges[j].Low + i : _training.Count - 1,
                        ranges[j].High + i < _training.Count - 1 ? ranges[j].High + i : _training.Count - 1);
                });

                if (_flat.Limited)
                {
                    LearnLimited();
                    lastLearn = 0;
                }
                else
                {
                    Learn();
                    lastLearn = 0;
                }
                
            }

            // handle any remaining learning
            if (lastLearn > 0)
            {
                Learn();
            }

            this.Error = _workers.Sum(x => x.CalculateError.CalculateMSE() * x.n) / _workers.Sum(x => x.n);

        }


        /// <summary>
        /// The batch size. Specify 1 for pure online training. Specify 0 for pure
        /// batch training (complete training set in one batch). Otherwise specify
        /// the batch size for batch training.
        /// </summary>
        public int BatchSize { get; set; }

        public double DropoutPercentage { get; set; }

        public double MaxWeightNorm { get; set; }
    }
}

