using Encog.ML.Data;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.PreProcessing.DataProviders
{
    class InitialDelaySizeInputProvider : IDataProvider
    {
        public NormalizationTypeEnum NormalizationType { get; set; }

        private const string name = "InitialDelaySize";
        public string Name {get {return name;}}
        /// <summary>
        /// Integer representing the node count of this module
        /// </summary>
        private int _size = 1;
        public int Size { get { return _size; } }

        /// <summary>
        /// Representing the labels for the nodes
        /// </summary>
        private List<string> _map = new List<string>();
        public List<string> Map
        { get { return _map; } }

        /// <summary>
        /// Represeting the index of the input/output IMLData where this specific data starts
        /// </summary>
        public int LowerIndex { get; set; }

        public InitialDelaySizeInputProvider()
        {
            Map.Add("Initial Delay");
        }

        public double[] Process(DelayCombination dc)
        {
            double[] result = new double[Size];
            foreach(Delay d in dc.primarydelays)
            {
                result[0] += d.destinationdelay;
            }
            return result;

        }

        public List<Tuple<string, dynamic>> PublishOutput(IMLData d)
        {
            throw new NotImplementedException();
        }

    }
}
