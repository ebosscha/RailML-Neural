using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
    class LearningRateDecayStrategy : IStrategy
    {
        double _rate;
        ILearningRate training;

        public LearningRateDecayStrategy(double DecayRate)
        {
            _rate = DecayRate;
        }

        public void Init(Encog.ML.Train.IMLTrain train)
        {
            if(train is ILearningRate)
            {
                training = (ILearningRate)train;
            }
        }

        public void PostIteration()
        {
            training.LearningRate *= (1 - _rate);
        }

        public void PreIteration()
        {
            return;
        }
    }

    public class PureRecurrence : IStrategy
    {
        IMLTrain _training;
        public void Init(Encog.ML.Train.IMLTrain train)
        {
            _training = train;
            if(_training.Method is BasicNetwork)
            {
                var net = _training.Method as BasicNetwork;
                var flat = net.Flat;
                for(int i = 1; i < flat.LayerCounts.Length; i++)
                {
                    if(flat.LayerContextCount[i] > 0)
                    {
                        int inputIndex = flat.LayerIndex[i];
                        int outputIndex = flat.LayerIndex[i - 1];
                        int inputSize = flat.LayerCounts[i];
                        int outputSize = flat.LayerFeedCounts[i - 1];
                        int ContextIndex = flat.ContextTargetOffset[i - 1];


                        int index = flat.WeightIndex[i - 1];

                        int limitX = outputIndex + outputSize;
                        int limitY = inputIndex + inputSize;

                        // weight values
                        for (int x = outputIndex; x < limitX; x++)
                        {
                            for (int y = inputIndex; y < limitY; y++)
                            {
                                if (y >= ContextIndex && y - ContextIndex != x - outputIndex)
                                {
                                    flat.Weights[index++] = 0;
                                }
                            }
                        }                    
                    }
                }
            }
            PostIteration();
        }

        public void PostIteration()
        {
            return;
        }

        public void PreIteration()
        {
            return;
        }
    }

    public class EnforcedElmanStrategy : IStrategy
    {
        IMLTrain _training;
        public void Init(Encog.ML.Train.IMLTrain train)
        {
            _training = train;
            PostIteration();
        }

        public void PostIteration()
        {
            if (_training.Method is BasicNetwork)
            {
                var net = _training.Method as BasicNetwork;
                var flat = net.Flat;
                for (int i = 1; i < flat.LayerCounts.Length; i++)
                {
                    if (flat.LayerContextCount[i] > 0)
                    {
                        int inputIndex = flat.LayerIndex[i];
                        int outputIndex = flat.LayerIndex[i - 1];
                        int inputSize = flat.LayerCounts[i];
                        int outputSize = flat.LayerFeedCounts[i - 1];
                        int ContextIndex = flat.ContextTargetOffset[i - 1];


                        int index = flat.WeightIndex[i - 1];

                        int limitX = outputIndex + outputSize;
                        int limitY = inputIndex + inputSize;

                        // weight values
                        for (int x = outputIndex; x < limitX; x++)
                        {
                            for (int y = inputIndex; y < limitY; y++)
                            {
                                if (y >= ContextIndex && y - ContextIndex != x - outputIndex)
                                {
                                    flat.Weights[index++] = 0;
                                }
                                else if (y - ContextIndex == x - outputIndex)
                                {
                                    flat.Weights[index++] = 1;
                                }

                            }
                        }
                    }
                }
            }
        }

        public void PreIteration()
        {
            return;
        }
    }
}
