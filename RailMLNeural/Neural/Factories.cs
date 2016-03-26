using Encog.Engine.Network.Activation;
using Encog.MathUtil.Randomize;
using Encog.ML;
using Encog.Neural.Networks.Structure;
using Encog.Neural.Networks.Training;
using RailMLNeural.Neural.Algorithms;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.Neural.PreProcessing.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural
{
    public static class ActivationFactory
    {
        public static IActivationFunction Create(ActivationFunctionEnum act)
        {
            switch(act)
            {
                case ActivationFunctionEnum.None:
                    return null;
                case ActivationFunctionEnum.BiPolar:
                    return new ActivationBiPolar();
                case ActivationFunctionEnum.BiPolarSteepenedSigmoid:
                    return new ActivationBipolarSteepenedSigmoid();
                case ActivationFunctionEnum.ClippedLinear:
                    return new ActivationClippedLinear();
                case ActivationFunctionEnum.Competitive:
                    return new ActivationCompetitive();
                case ActivationFunctionEnum.Elliott:
                    return new ActivationElliott();
                case ActivationFunctionEnum.ElliottSymmetric:
                    return new ActivationElliottSymmetric();
                case ActivationFunctionEnum.Linear:
                    return new ActivationLinear();
                case ActivationFunctionEnum.LOG:
                    return new ActivationLOG();
                case ActivationFunctionEnum.Ramp:
                    return new ActivationRamp();
                case ActivationFunctionEnum.Sigmoid:
                    return new ActivationSigmoid();
                case ActivationFunctionEnum.SIN:
                    return new ActivationSIN();
                case ActivationFunctionEnum.SoftMax:
                    return new ActivationSoftMax();
                case ActivationFunctionEnum.SteepenedSigmoid:
                    return new ActivationSteepenedSigmoid();
                case ActivationFunctionEnum.TANH:
                    return new ActivationTANH();
                case ActivationFunctionEnum.Gaussian:
                    return new ActivationGaussian();
                case ActivationFunctionEnum.ReLu:
                    return new ActivationReLu();
                case ActivationFunctionEnum.SoftPlus:
                    return new ActivationSoftPlus();
                case ActivationFunctionEnum.FuzzyReLu:
                    return new ActivationFuzzyReLu();
                case ActivationFunctionEnum.SteepenedFuzzyReLu:
                    return new ActivationSteepFuzzyReLu();
                case ActivationFunctionEnum.Biological:
                    return new ActivationBiological();
                default:
                    return null;
            }
        }
    }

    public static class DataProviderFactory
    {
        public static IDataProvider CreateDataProvider(DataProviderEnum p)
        {
            switch(p)
            {
                case DataProviderEnum.PerLineExactInput:
                    return new PerLineExactInputProvider();
                case DataProviderEnum.TimeOfDayInput:
                    return new TimeInputProvider(24);
                case DataProviderEnum.InitialDelayInput:
                    return new InitialDelaySizeInputProvider();
                case DataProviderEnum.LineClassificationInput:
                    return new LineClassificationInputProvider();
                case DataProviderEnum.PerLineClassificationOutput:
                    return new PerLineClassificationOutputProvider();
                case DataProviderEnum.PerLineExactOutput:
                    return new PerLineExactOutputProvider();
                default:
                    return null;
            }
        }

        public static IRecurrentDataProvider CreateRecurrentDataProvider(RecurrentDataProviderEnum p)
        {
            switch(p)
            {
                case RecurrentDataProviderEnum.PreviousDelaySizeInputRecurrentProvider:
                    return new PreviousDelaySizeInputRecurrentProvider();
                case RecurrentDataProviderEnum.DelaySizeInputRecurrentProvider:
                    return new DelaySizeInputRecurrentProvider();
                case RecurrentDataProviderEnum.EdgeHeadwayInputRecurrentProvider:
                    return new EdgeHeadwayInputRecurrentProvider();
                case RecurrentDataProviderEnum.EdgeDelaysInputRecurrentProvider:
                    return new EdgeDelaysInputRecurrentProvider();
                case RecurrentDataProviderEnum.VerticesHeadwayInputRecurrentProvider:
                    return new VerticesHeadwayInputRecurrentProvider();
                case RecurrentDataProviderEnum.RouteLengthInputRecurrentProvider:
                    return new RouteLengthInputRecurrentProvider();
                case RecurrentDataProviderEnum.EdgeDepartureCountInputRecurrentProvider:
                    return new EdgeDepartureCountInputRecurrentProvider();
                case RecurrentDataProviderEnum.VertexDepartureCountInputRecurrentProvider:
                    return new VertexDepartureCountInputRecurrentProvider();
                case RecurrentDataProviderEnum.StationTracksInputRecurrentProvider:
                    return new StationTracksInputRecurrentProvider();
                case RecurrentDataProviderEnum.DoubleTrackInputRecurrentProvider:
                    return new DoubleTrackInputRecurrentProvider();
                case RecurrentDataProviderEnum.DelaySizeOutputRecurrentProvider:
                    return new DelaySizeOutputRecurrentProvider();
                case RecurrentDataProviderEnum.ScheduledTimesInputRecurrentProvider:
                    return new ScheduledTimesInputRecurrentProvider();
                case RecurrentDataProviderEnum.SwitchCountInputRecurrentProvider:
                    return new SwitchCountInputRecurrentProvider();
                case RecurrentDataProviderEnum.ForecastedTimesInputRecurrentProvider:
                    return new ForecastedTimesInputRecurrentProvider();
                case RecurrentDataProviderEnum.IsDelayedOutputRecurrentProvider:
                    return new IsDelayedOutputRecurrentProvider();
                case RecurrentDataProviderEnum.TrainDirectionInputRecurrentProvider:
                    return new TrainDirectionInputRecurrentProvider();
                case RecurrentDataProviderEnum.TotalDelayOutputRecurrentProvider:
                    return new TotalDelayOutputRecurrentProvider();
                case RecurrentDataProviderEnum.RelativeTotalDelayOutputRecurrentProvider:
                    return new RelativeTotalDelayOutputRecurrentProvider();
                case RecurrentDataProviderEnum.TrainSpeedInputRecurrentProvider:
                    return new TrainSpeedInputRecurrentProvider();
                case RecurrentDataProviderEnum.EdgeMaxSpeedInputRecurrentProvider:
                    return new EdgeMaxSpeedInputRecurrentProvider();
                case RecurrentDataProviderEnum.EdgeSpeedHomogeneityInputRecurrentProvider:
                    return new EdgeSpeedHomogeneityInputRecurrentProvider();
                case RecurrentDataProviderEnum.InclusiveTotalDelayOutputRecurrentProvider:
                    return new InclusiveTotalDelayOutputRecurrentProvider();
                case RecurrentDataProviderEnum.InclusiveTotalDelayClassificationProvider:
                    return new InclusiveTotalDelayClassificationProvider();
                case RecurrentDataProviderEnum.TotalDelayClassificationProvider:
                    return new TotalDelayClassificationProvider();
                default:
                    return null;
            }
        }
    }

    public static class PropagatorFactory
    {
        public static IPropagator Create(IContainsGraph Owner, PropagatorEnum Type, bool UseSubGraph)
        {
            switch(Type)
            {
                case PropagatorEnum.Chronological:
                    return new ChronologicalPropagator(Owner, UseSubGraph);
                case PropagatorEnum.FollowTrain:
                    return new FollowTrainPropagator(Owner, UseSubGraph);
                default:
                    return null;
            }
        }
    }

    //public static class CalculateScoreFactory
    //{
    //    public static ICalculateScore CreateRecurrent(CalculateScoreEnum Type, IContainsGraph Owner, DelayCombinationSet Data)
    //    {
    //        switch(Type)
    //        {
    //            case CalculateScoreEnum.MSE:
    //                return new RecurrentCalculateScore(Owner, Data);
    //            case CalculateScoreEnum.Threshold:
    //                return new ThresholdCalculateScore(Owner, Data, 3);
    //            case CalculateScoreEnum.WeightedMSE:
    //                return new WeightedMSECalculateScore(Owner, Data);
    //            default:
    //                return null;                  
    //        }
    //    }
    //}

    public static class ErrorCalculationFactory
    {
        public static IErrorCalculation Create(CalculateScoreEnum Type, int OutputSize)
        {
            switch(Type)
            {
                case CalculateScoreEnum.MSE:
                    return new MSEErrorCalculation();
                case CalculateScoreEnum.Threshold:
                    return new ThresholdErrorCalculation();
                case CalculateScoreEnum.WeightedMSE:
                    return new WeightedMSEErrorCalculation(OutputSize);
                default:
                    return null; 
            }
        }
    }

    public static class NetworkFactory
    {
        public static IMLEncodable CreateEncodable(IMLEncodable N)
        {
            IMLEncodable r = (IMLEncodable)((ICloneable)N).Clone();
            GaussianRandomizer rand = new GaussianRandomizer(0, 2);
            double[] result = new double[r.EncodedArrayLength()];
            for(int i = 0; i < result.Length; i++)
            {
                result[i] = rand.NextDouble();
            }
            NetworkCODEC.ArrayToNetwork(result, r);
            return r;
        }
    }
}
