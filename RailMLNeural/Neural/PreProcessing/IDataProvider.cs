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
        int LowerIndex { get; }

    }
}
