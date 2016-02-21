using RailMLNeural.Data;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data.RecurrentDataProviders
{
    [Serializable]
    class DoubleTrackInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "DoubleTrackInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Percentage Double Track" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public DoubleTrackInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[0] = rep.Edge.PercentageDoubleTrack;
            return result;


        }
    }
}
