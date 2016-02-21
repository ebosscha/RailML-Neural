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
        Convolutional
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
        Gaussian
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

    }
}
