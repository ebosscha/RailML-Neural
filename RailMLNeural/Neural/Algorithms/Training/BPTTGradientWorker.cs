
using System;
using Encog.Engine.Network.Activation;
using Encog.MathUtil.Error;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Error;
using Encog.Neural.Flat;
using Encog.Util;
using Encog.Util.Concurrency;
using System.Collections.Generic;
using System.Linq;
using Encog.MathUtil;

namespace RailMLNeural.Neural.Algorithms.Training
{
    /// <summary>
    /// Worker class for the mulithreaded training of flat networks.
    /// </summary>
    ///
    public class BPTTGradientWorker
    {
        /// <summary>
        /// The actual values from the neural network.
        /// </summary>
        ///
        private readonly double[] _actual;

        /// <summary>
        /// The error calculation method.
        /// </summary>
        ///
        private readonly ErrorCalculation _errorCalculation;

        private double[] _dropoutMask;

        private double _dropoutPercentage;


        /// <summary>
        /// The gradients.
        /// </summary>
        ///
        private readonly double[] _gradients;

        private readonly double[] _tempGradients;

        private int[] _gradientChangeCount;

        private double[] _nextLayerDelta;

        private int[] _recurrentLayers;

        /// <summary>
        /// The low end of the training.
        /// </summary>
        ///
        private readonly int _high;

        /// <summary>
        /// The neuron counts, per layer.
        /// </summary>
        ///
        private readonly int[] _layerCounts;

        /// <summary>
        /// The deltas for each layer.
        /// </summary>
        ///
        private readonly double[] _layerDelta;

        /// <summary>
        /// The feed counts, per layer.
        /// </summary>
        ///
        private readonly int[] _layerFeedCounts;

        /// <summary>
        /// The layer indexes.
        /// </summary>
        ///
        private readonly int[] _layerIndex;

        /// <summary>
        /// The output from each layer.
        /// </summary>
        ///
        private readonly double[] _layerOutput;

        /// <summary>
        /// The sum from each layer.
        /// </summary>
        ///
        private readonly double[] _layerSums;

        /// <summary>
        /// The high end of the training data.
        /// </summary>
        ///
        private readonly int _low;

        /// <summary>
        /// The network to train.
        /// </summary>
        ///
        private readonly FlatNetwork _network;

        /// <summary>
        /// The owner.
        /// </summary>
        ///
        private readonly PropagationThroughTime _owner;

        /// <summary>
        /// The training data.
        /// </summary>
        ///
        private readonly IList<IMLDataSet> _training;

        /// <summary>
        /// The index to each layer's weights and thresholds.
        /// </summary>
        ///
        private readonly int[] _weightIndex;

        /// <summary>
        /// The weights and thresholds.
        /// </summary>
        ///
        private readonly double[] _weights;

        /// <summary>
        /// Derivative add constant.  Used to combat flat spot.
        /// </summary>
        private readonly double[] _flatSpot;

        /// <summary>
        /// The error function.
        /// </summary>
        private readonly IErrorFunction _ef;

        private List<double[]> layerOutputList;

        private List<double[]> layerSumList;

        private VectorAlgebra m_va;


        /// <summary>
        /// Construct a gradient worker.
        /// </summary>
        ///
        /// <param name="theNetwork">The network to train.</param>
        /// <param name="theOwner">The owner that is doing the training.</param>
        /// <param name="theTraining">The training data.</param>
        /// <param name="theLow">The low index to use in the training data.</param>
        /// <param name="theHigh">The high index to use in the training data.</param>
        /// <param name="theFlatSpots">Holds an array of flat spot constants.</param>
        public BPTTGradientWorker(FlatNetwork theNetwork,
                                    PropagationThroughTime theOwner, IList<IMLDataSet> theTraining,
                                    int theLow, int theHigh, double[] theFlatSpots, IErrorFunction ef, double dropoutPercentage)
        {
            _errorCalculation = new ErrorCalculation();
            _network = theNetwork;
            _training = theTraining;
            _low = theLow;
            _high = theHigh;
            _owner = theOwner;
            _flatSpot = theFlatSpots;

            _layerDelta = new double[_network.LayerOutput.Length];
            _nextLayerDelta = new double[_network.LayerOutput.Length];
            _gradients = new double[_network.Weights.Length];
            _tempGradients = new double[_network.Weights.Length];
            _gradientChangeCount = new int[_network.Weights.Length];
            _actual = new double[_network.OutputCount];

            _recurrentLayers = _network.ContextTargetSize.Select((x, i) => x > 0 ? i : -1).Where(i => i != -1).ToArray();
            _weights = _network.Weights;
            _layerIndex = _network.LayerIndex;
            _layerCounts = _network.LayerCounts;
            _weightIndex = _network.WeightIndex;
            _layerOutput = _network.LayerOutput;
            _layerSums = _network.LayerSums;
            _layerFeedCounts = _network.LayerFeedCounts;
            _dropoutPercentage = dropoutPercentage;
            _dropoutMask = new double[_layerOutput.Length];
            for (int i = 0; i < _dropoutMask.Length; i++ )
            {
                _dropoutMask[i] = 1;
            }
            _ef = ef;
            m_va = new VectorAlgebra();
        }

        #region FlatGradientWorker Members

        /// <inheritdoc/>
        public FlatNetwork Network
        {
            get { return _network; }
        }


        /// <value>The weights for this network.</value>
        public double[] Weights
        {
            get { return _weights; }
        }

        /// <summary>
        /// Perform the gradient calculation for the specified index range.
        /// </summary>
        ///
        public void Run()
        {
            try
            {
                _errorCalculation.Reset();
                for (int i = _low; i <= _high; i++)
                {
                    var set = _training[i];
                    Process(set);
                }
                double error = _errorCalculation.Calculate();
                _owner.Report(_gradients, error, null);
                EngineArray.Fill(_gradients, 0);
            }
            catch (Exception ex)
            {
                _owner.Report(null, 0, ex);
            }
        }

        #endregion

        private void Process(IMLDataSet seq)
        {
            EngineArray.Fill(_tempGradients, 0);
            _gradientChangeCount = new int[_network.Weights.Length];
            layerOutputList = new List<double[]>();
            layerSumList = new List<double[]>();
            //_network.ClearContext();
            ClearContexts();
            SetDropoutMask();
            for(int i = 0; i < seq.Count; i++)
            {
                IMLDataPair pair = seq[i];
                //_network.Compute(pair.Input, _actual);
                Compute(pair.Input, _actual);
                layerOutputList.Add(EngineArray.ArrayCopy(_network.LayerOutput));
                layerSumList.Add(EngineArray.ArrayCopy(_network.LayerSums));
                if (i == seq.Count - 1)
                {
                    EngineArray.Fill(_layerDelta, 0);
                    _errorCalculation.UpdateError(_actual, pair.Ideal, pair.Significance);
                    _ef.CalculateError(pair.Ideal, _actual, _layerDelta);

                    for (int j = 0; j < _actual.Length; j++)
                    {
                        _layerDelta[j] = (_network.ActivationFunctions[0]
                            .DerivativeFunction(_layerSums[j], _layerOutput[j]) + _flatSpot[0])
                            * _layerDelta[j] * pair.Significance;
                    }
                    for (int j = _network.BeginTraining; j < _network.EndTraining; j++)
                    {
                        ProcessLevel(j);
                    }
                    if (_network.ContextTargetSize.Any(x => x > 0))
                    {
                        int layerIndex = _network.ContextTargetSize.IndexOfMax();
                        double[] _RecurrentError = new double[_network.ContextTargetSize[layerIndex]];
                        EngineArray.ArrayCopy(_layerDelta, _network.ContextTargetOffset[layerIndex], _RecurrentError, 0, _RecurrentError.Length);
                        BPTT(pair.Significance);
                    }
                }
            }
            for(int i = 0; i < _tempGradients.Length; i++)
            {
                _gradients[i] += _tempGradients[i] / _gradientChangeCount[i];
            }
        }

        /// <summary>
        /// Process one training set element.
        /// </summary>
        ///
        /// <param name="input">The network input.</param>
        /// <param name="ideal">The ideal values.</param>
        /// <param name="s">The significance of this error.</param>
        private void Process(double[] error, double Significance, int startLayer)
        {
            EngineArray.ArrayCopy(error, 0, _layerDelta, Network.LayerIndex[startLayer], error.Length);

            int j = Network.LayerIndex[startLayer];
            for (int i = 0; i < error.Length; i++)
            {
                _layerDelta[j] = (_network.ActivationFunctions[startLayer]
                    .DerivativeFunction(_layerSums[j], _layerOutput[j]) + _flatSpot[0])
                    * _layerDelta[j] * Significance;
                j++;
            }

            for (int i = startLayer; i < _network.EndTraining; i++)
            {
                ProcessLevel(i);
            }
        }

        /// <summary>
        /// Process one training set element.
        /// </summary>
        ///
        /// <param name="input">The network input.</param>
        /// <param name="ideal">The ideal values.</param>
        /// <param name="s">The significance of this error.</param>
        private void Process(double Significance, int startLayer)
        {
            for (int i = startLayer; i < _network.EndTraining; i++)
            {
                ProcessLevel(i);
            }
        }

        /// <summary>
        /// The error calculation to use.
        /// </summary>
        public ErrorCalculation CalculateError { get { return _errorCalculation; } }

        public void Run(int index)
        {
            var set = _training[index];
            Process(set);
            _owner.Report(_gradients, 0, null);
            EngineArray.Fill(_gradients, 0);
        }

        public void Run(int low, int high)
        {
            for(int i = low; i <= high; i++)
            {
                Process(_training[i]);
                n++;
            }
            _owner.Report(_gradients, 0, null);
            EngineArray.Fill(_gradients, 0);
        }

        public int n = 0;
        public void Reset()
        {
            _errorCalculation.Reset();
            n = 0;
        }

        /// <summary>
        /// Process one level.
        /// </summary>
        ///
        /// <param name="currentLevel">The level.</param>
        private void ProcessLevel(int currentLevel)
        {
            int fromLayerIndex = _layerIndex[currentLevel + 1];
            int toLayerIndex = _layerIndex[currentLevel];
            int fromLayerSize = _layerCounts[currentLevel + 1];
            int toLayerSize = _layerFeedCounts[currentLevel];

            int index = _weightIndex[currentLevel];
            IActivationFunction activation = _network.ActivationFunctions[currentLevel + 1];
            double currentFlatSpot = _flatSpot[currentLevel + 1];

            // handle weights
            int yi = fromLayerIndex;
            unchecked
            {
                for (int y = 0; y < fromLayerSize; y++)
                {
                    double output = _layerOutput[yi];
                    double sum = 0;
                    int xi = toLayerIndex;
                    int wi = index + y;
                    for (int x = 0; x < toLayerSize; x++)
                    {
                        _tempGradients[wi] += unchecked(output * _layerDelta[xi]);
                        _gradientChangeCount[wi]++;
                        sum += unchecked(_weights[wi] * _layerDelta[xi]);
                        wi += fromLayerSize;
                        xi++;
                    }

                    _layerDelta[yi] += sum
                                        * (activation.DerivativeFunction(_layerSums[yi], _layerOutput[yi]) + currentFlatSpot);
                    yi++;
                }
            }
        }

        private void BPTT(double Significance)
        {
            double[] Gradients = new double[_weights.Length];
            int j = 0;
            for (int i = layerOutputList.Count - 2; i > -1; i--)
            {
                EngineArray.Fill(_nextLayerDelta, 0);
                foreach (int l in _recurrentLayers)
                {
                    EngineArray.ArrayCopy(_layerDelta, _network.ContextTargetOffset[l], _nextLayerDelta, _layerIndex[l], _network.ContextTargetSize[l]);
                }
                EngineArray.ArrayCopy(layerOutputList[i], _network.LayerOutput);
                EngineArray.ArrayCopy(layerSumList[i], _network.LayerSums);
                EngineArray.ArrayCopy(_nextLayerDelta, _layerDelta);
                Process(Significance, _recurrentLayers.Min());
                j++;
            }
        }

        #region Dropout Computation
        private void Compute(IMLData input, double[] output)
        {
            int sourceIndex = _layerOutput.Length
                              - _layerCounts[_layerCounts.Length - 1];

            input.CopyTo(_layerOutput, sourceIndex, input.Count);

            InnerCompute(output);
        }

        private void InnerCompute(double[] output)
        {
            for (int i = _layerIndex.Length - 1; i > 0; i--)
            {
                ComputeLayer(i);
            }

            // update context values
            int offset = _network.ContextTargetOffset[0];

            for (int x = 0; x < _network.ContextTargetSize[0]; x++)
            {
                _layerOutput[offset + x] = _layerOutput[x];
            }

            EngineArray.ArrayCopy(_layerOutput, 0, output, 0, _network.OutputCount);
        }


        private void ComputeLayer(int currentLayer)
        {
            unchecked
            {
                int inputIndex = _layerIndex[currentLayer];
                int outputIndex = _layerIndex[currentLayer - 1];
                int inputSize = _layerCounts[currentLayer];
                int outputSize = _layerFeedCounts[currentLayer - 1];

                int index = _weightIndex[currentLayer - 1];

                int limitX = outputIndex + outputSize;
                int limitY = inputIndex + inputSize;

                // weight values
                for (int x = outputIndex; x < limitX; x++)
                {
                    double sum = 0;
                    for (int y = inputIndex; y < limitY; y++)
                    {
                        sum += unchecked(_weights[index++] * _network.LayerOutput[y]);
                    }
                    _network.LayerOutput[x] = sum * _dropoutMask[x];
                    _network.LayerSums[x] = sum * _dropoutMask[x];
                }

                _network.ActivationFunctions[currentLayer - 1].ActivationFunction(
                    _network.LayerOutput, outputIndex, outputSize);

                // update context values
                int offset = _network.ContextTargetOffset[currentLayer];

                for (int x = 0; x < _network.ContextTargetSize[currentLayer]; x++)
                {
                    _network.LayerOutput[offset + x] = _network.LayerOutput[outputIndex + x];
                }
            }
        }

        private void ClearContexts()
        {
            int index = 0;

            for (int i = 0; i < _layerIndex.Length; i++)
            {
                bool hasBias = (_network.LayerContextCount[i] + _layerFeedCounts[i]) != _layerCounts[i];

                // fill in regular neurons
                for (int j = 0; j < _layerFeedCounts[i]; j++)
                {
                    _network.LayerOutput[index++] = 0;
                }

                // fill in the bias
                if (hasBias)
                {
                    _network.LayerOutput[index++] = _network.BiasActivation[i];
                }

                // fill in context
                for (int j = 0; j < _network.LayerContextCount[i]; j++)
                {
                    _network.LayerOutput[index++] = 0;
                }
            }
        }

        private Random rand = new Random();
        private void SetDropoutMask()
        {
            for(int i = 1; i < _layerIndex.Length - 1; i++)
            {
                for(int j = _layerIndex[i]; j < _layerIndex[i] + _layerFeedCounts[i]; j++)
                {
                    _dropoutMask[j] = rand.NextDouble() < _dropoutPercentage ? 0 : 1;
                }
            }
        }
        #endregion Dropout Computation
    }
}

