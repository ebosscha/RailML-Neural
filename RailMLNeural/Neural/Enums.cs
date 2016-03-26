using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural
{
    [Serializable]
    public enum AlgorithmEnum
    {
        GraphRecurrent,
        Recursive,
        FeedForward,
        Convolutional,
        NEAT,
        LSTM
    }

    [Serializable]
    public enum ActivationFunctionEnum
    {
        None,
        BiPolar,
        BiPolarSteepenedSigmoid,
        ClippedLinear,
        Competitive,
        Elliott,
        ElliottSymmetric,
        Linear,
        LOG,
        Ramp,
        Sigmoid,
        SIN,
        SoftMax,
        SteepenedSigmoid,
        TANH,
        Gaussian,
        ReLu,
        FuzzyReLu,
        SteepenedFuzzyReLu,
        SoftPlus,
        Biological
    }

    [Serializable]
    public enum LearningAlgorithmEnum
    {
        BackPropagation,
        ResilientPropagation,
        QuickPropagation,
        ManhattanPropagation,
        ScaledConjugateGradient,
        SimulatedAnnealing,
        ParticleSwarmOptimization,
        MultipleSwarmOptimization,
        Genetic,

    }

    [Serializable]
    public enum CalculateScoreEnum
    {
        MSE,
        Threshold,
        WeightedMSE,
        DataMSE,
        WeightedDataMSE,
    }
}
