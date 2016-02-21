using Encog.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.PreProcessing
{
    /// <summary>
    /// Interface that deals with supplying vector information and decoding this information for a neural network
    /// </summary>
    public interface IDataProvider
    {
        //Interface Members
        double[] Process(DelayCombination delaycombination);
        List<Tuple<string, dynamic>> PublishOutput(IMLData Data);
        List<string> Map { get; }
        int Size { get; }
        int LowerIndex { get; }
        Normalization.NormalizationTypeEnum NormalizationType { get; set; }
        string Name { get; }
        bool IsInput { get; }
    }

    [Serializable]
    public enum InputDataProviderEnum
    {
        PerLineExactInput,
        TimeOfDayInput,
        InitialDelayInput,
        LineClassificationInput
    }

    [Serializable]
    public enum OutputDataProviderEnum
    {
        PerLineExactOutput,
        PerLineClassificationOutput
    }

    [Serializable]
    public enum DataProviderEnum
    {
        InitialDelayInput,
        LineClassificationInput,
        PerLineExactInput,
        TimeOfDayInput,
        PerLineExactOutput,
        PerLineClassificationOutput
    }
}
