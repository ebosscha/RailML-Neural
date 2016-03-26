﻿using Encog.Neural.Networks.Structure;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation;
using Encog.Util;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
    class GNQuickPropagation : GraphNeuralPropagation, ILearningRate
    {
         /// <summary>
        /// This factor times the current weight is added to the slope 
        /// at the start of each output epoch. Keeps weights from growing 
        /// too big.
        /// </summary>
        public double Decay { get; set; }

        /// <summary>
        /// Used to scale for the size of the training set.
        /// </summary>
        public double EPS { get; set; }

        /// <summary>
        /// The last deltas.
        /// </summary>
        public double[] LastDelta { get; set; }

        /// <summary>
        /// The learning rate.
        /// </summary>
        public double LearningRate { get; set; }

        /// <summary>
        /// Controls the amount of linear gradient descent 
        /// to use in updating output weights.
        /// </summary>
        public double OutputEpsilon { get; set; }

        /// <summary>
        /// Used in computing whether the proposed step is 
        /// too large.  Related to learningRate.
        /// </summary>
        public double Shrink { get; set; }


        /// <summary>
        /// Continuation tag for the last gradients.
        /// </summary>
        public const String LastGradients = "LAST_GRADIENTS";
        
        /// <summary>
        /// Construct a QPROP trainer for flat networks.  Uses a learning rate of 2.
        /// </summary>
        /// <param name="network">The network to train.</param>
        /// <param name="training">The training data.</param>
        public GNQuickPropagation(GRNNConfiguration Owner, FlatGRNN network, DelayCombinationSet training) : this(Owner, network, training, 2.0)
        {
        }


        /// <summary>
        /// Construct a QPROP trainer for flat networks.
        /// </summary>
        /// <param name="network">The network to train.</param>
        /// <param name="training">The training data.</param>
        /// <param name="learnRate">The learning rate.  2 is a good suggestion as 
        ///            a learning rate to start with.  If it fails to converge, 
        ///            then drop it.  Just like backprop, except QPROP can 
        ///            take higher learning rates.</param>
        public GNQuickPropagation(GRNNConfiguration Owner, FlatGRNN network,
                                DelayCombinationSet training, double learnRate) : base(Owner, network, training)
        {
            LearningRate = learnRate;
            LastDelta = new double[Network.EncodedArrayLength()];
            OutputEpsilon = 1.0;
        }

        /// <inheritdoc />
        public override bool CanContinue
        {
            get { return true; }
        }

       
        /// <summary>
        /// Determine if the specified continuation object is valid to resume with.
        /// </summary>
        /// <param name="state">The continuation object to check.</param>
        /// <returns>True if the specified continuation object is valid for this
	    /// training method and network.</returns>
        public bool IsValidResume(TrainingContinuation state)
        {
            if (!state.Contents.ContainsKey(LastGradients))
            {
                return false;
            }

            if (!state.TrainingType.Equals(GetType().Name))
            {
                return false;
            }

            var d = (double[]) state.Contents[LastGradients];
            return d.Length == Network.EncodedArrayLength();
        }

        /// <summary>
        /// Pause the training.
        /// </summary>
        /// <returns>A training continuation object to continue with.</returns>
        public override TrainingContinuation Pause()
        {
            var result = new TrainingContinuation {TrainingType = (GetType().Name)};
            result.Contents[LastGradients] = LastGradient;
            return result;
        }
        
        /// <summary>
        /// Resume training.
        /// </summary>
        /// <param name="state">The training state to return to.</param>
        public override void Resume(TrainingContinuation state)
        {
            if (!IsValidResume(state))
            {
                throw new TrainingError("Invalid training resume data length");
            }

            var lastGradient = (double[]) state.Contents[
                LastGradients];

            EngineArray.ArrayCopy(lastGradient,LastGradient);
        }

        /// <summary>
        /// Called to init the QPROP.
        /// </summary>
        public override void InitOthers()
        {
            EPS = OutputEpsilon / _training.Count;
            Shrink = LearningRate / (1.0 + LearningRate);
        }

        /// <summary>
        /// Update a weight.
        /// </summary>
        /// <param name="gradients">The gradients.</param>
        /// <param name="lastGradient">The last gradients.</param>
        /// <param name="index">The index.</param>
        /// <returns>The weight delta.</returns>
        public override double UpdateWeight(double[] gradients,
                                            double[] lastGradient, int index)
        {
            double w = NetworkCODEC.NetworkToArray(Network)[index];
            double d = LastDelta[index];
            double s = -Gradients[index] + Decay * w;
            double p = -lastGradient[index];
            double nextStep = 0.0;

            // The step must always be in direction opposite to the slope.
            if (d < 0.0)
            {
                // If last step was negative...
                if (s > 0.0)
                {
                    // Add in linear term if current slope is still positive.
                    nextStep -= EPS * s;
                }
                // If current slope is close to or larger than prev slope...
                if (s >= (Shrink * p))
                {
                    // Take maximum size negative step.
                    nextStep += LearningRate * d;
                }
                else
                {
                    // Else, use quadratic estimate.
                    nextStep += d * s / (p - s);
                }
            }
            else if (d > 0.0)
            {
                // If last step was positive...
                if (s < 0.0)
                {
                    // Add in linear term if current slope is still negative.
                    nextStep -= EPS * s;
                }
                // If current slope is close to or more neg than prev slope...
                if (s <= (Shrink * p))
                {
                    // Take maximum size negative step.
                    nextStep += LearningRate * d;
                }
                else
                {
                    // Else, use quadratic estimate.
                    nextStep += d * s / (p - s);
                }
            }
            else
            {
                // Last step was zero, so use only linear term. 
                nextStep -= EPS * s;
            }

            // update global data arrays
            LastDelta[index] = nextStep;
            LastGradient[index] = gradients[index];

            return nextStep;
        }
    }
}
