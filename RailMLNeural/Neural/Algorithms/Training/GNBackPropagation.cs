using Encog.ML.Data;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation;
using Encog.Neural.Networks.Training.Strategy;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
    class GNBackPropagation : GraphNeuralPropagation , IMomentum, ILearningRate
    {
        /// <summary>
        /// The resume key for backpropagation.
        /// </summary>
        ///
        public const String PropertyLastDelta = "LAST_DELTA";

        /// <summary>
        /// The last delta values.
        /// </summary>
        ///
        private double[] _lastDelta;

        /// <summary>
        /// The learning rate.
        /// </summary>
        ///
        private double _learningRate;

        /// <summary>
        /// The momentum.
        /// </summary>
        ///
        private double _momentum;

        /// <summary>
        /// Create a class to train using backpropagation. Use auto learn rate and
        /// momentum. Use the CPU to train.
        /// </summary>
        ///
        /// <param name="network">The network that is to be trained.</param>
        /// <param name="training">The training data to be used for backpropagation.</param>
        public GNBackPropagation(GRNNConfiguration Owner, FlatGRNN network, DelayCombinationSet training) : this(Owner, network, training, 0, 0)
        {
            AddStrategy(new SmartLearningRate());
            AddStrategy(new SmartMomentum());
        }


        /// <param name="network">The network that is to be trained</param>
        /// <param name="training">The training set</param>
        /// <param name="learnRate"></param>
        /// <param name="momentum"></param>
        public GNBackPropagation(GRNNConfiguration Owner, FlatGRNN network,
                               DelayCombinationSet training, double learnRate,
                               double momentum) : base(Owner, network, training)
        {
            _momentum = momentum;
            _learningRate = learnRate;
            _lastDelta = new double[network.EncodedArrayLength()];
        }

        /// <param name="network">The network that is to be trained</param>
        /// <param name="training">The training set</param>
        /// <param name="learnRate"></param>
        /// <param name="momentum"></param>
        public GNBackPropagation(GRNNConfiguration Owner, FlatGRNN network,
                               IList<IMLDataSet> training, double learnRate,
                               double momentum)
            : base(Owner, network, training)
        {
            _momentum = momentum;
            _learningRate = learnRate;
            _lastDelta = new double[network.EncodedArrayLength()];
        }

        /// <inheritdoc />
        public override sealed bool CanContinue
        {
            get { return true; }
        }


        /// <value>Ther last delta values.</value>
        public double[] LastDelta
        {
            get { return _lastDelta; }
        }

        #region ILearningRate Members

        /// <summary>
        /// Set the learning rate, this is value is essentially a percent. It is the
        /// degree to which the gradients are applied to the weight matrix to allow
        /// learning.
        /// </summary>
        public virtual double LearningRate
        {
            get { return _learningRate; }
            set { _learningRate = value; }
        }

        #endregion

        #region IMomentum Members

        /// <summary>
        /// Set the momentum for training. This is the degree to which changes from
        /// which the previous training iteration will affect this training
        /// iteration. This can be useful to overcome local minima.
        /// </summary>
        public virtual double Momentum
        {
            get { return _momentum; }
            set { _momentum = value; }
        }

        #endregion

        /// <summary>
        /// Determine if the specified continuation object is valid to resume with.
        /// </summary>
        ///
        /// <param name="state">The continuation object to check.</param>
        /// <returns>True if the specified continuation object is valid for this
        /// training method and network.</returns>
        public bool IsValidResume(TrainingContinuation state)
        {
            if (!state.Contents.ContainsKey(PropertyLastDelta))
            {
                return false;
            }

            if (!state.TrainingType.Equals(GetType().Name))
            {
                return false;
            }

            var d = (double[])state.Get(PropertyLastDelta);
            return d.Length == Network.EncodedArrayLength();
        }

        /// <summary>
        /// Pause the training.
        /// </summary>
        ///
        /// <returns>A training continuation object to continue with.</returns>
        public override sealed TrainingContinuation Pause()
        {
            var result = new TrainingContinuation {TrainingType = GetType().Name};
            result.Set(PropertyLastDelta, _lastDelta);
            return result;
        }

        /// <summary>
        /// Resume training.
        /// </summary>
        ///
        /// <param name="state">The training state to return to.</param>
        public override sealed void Resume(TrainingContinuation state)
        {
            if (!IsValidResume(state))
            {
                throw new TrainingError("Invalid training resume data length");
            }

            _lastDelta = (double[])state.Get(PropertyLastDelta);
        }

        /// <summary>
        /// Update a weight.
        /// </summary>
        ///
        /// <param name="gradients">The gradients.</param>
        /// <param name="lastGradient">The last gradients.</param>
        /// <param name="index">The index.</param>
        /// <returns>The weight delta.</returns>
        public override double UpdateWeight(double[] gradients,
                                                   double[] lastGradient, int index)
        {
            double delta = (gradients[index] * _learningRate)
                           + (_lastDelta[index] * _momentum);
            _lastDelta[index] = delta;
            return delta;
        }

        /// <summary>
        /// Not needed for this training type.
        /// </summary>
        public override void InitOthers()
        {
        }
    }
}

