using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural
{
    public enum AlgorithmEnum
    {
        PerLineClassification,
        PerLineExact,
        PerRoutePartExact,
        RecurrentClassification,
        RecurrentExact
    }

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
        TANH
    }

    public enum LearningAlgorithmEnum
    {
        BackPropagation,
        ResilientPropagation,
        QuickPropagation,
        ManhattanPropagation,
        ScaledConjugateGradient
    }
}
