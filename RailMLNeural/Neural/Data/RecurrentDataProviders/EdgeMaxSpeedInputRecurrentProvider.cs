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
    class EdgeMaxSpeedInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "EdgeMaxSpeedInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Maximum speed on edge(km/h)" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public EdgeMaxSpeedInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[0] = rep.Direction == DirectionEnum.Down ? rep.Edge.AverageSpeedDown : rep.Edge.AverageSpeedUp;
            result[0] /= 100;
            return result;
        }
    }
}
