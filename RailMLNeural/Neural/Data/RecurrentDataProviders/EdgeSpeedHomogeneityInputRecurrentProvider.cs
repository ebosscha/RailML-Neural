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
    class EdgeSpeedHomogeneityInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "EdgeMaxSpeedInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "stddev of speed on edge" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public EdgeSpeedHomogeneityInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            double maxspeed = (rep.Direction == DirectionEnum.Up ? rep.Edge.AverageSpeedUp : rep.Edge.AverageSpeedDown);
            result[0] = maxspeed == 0 ? 0 : (rep.Edge.SpeedHomogenityDown / maxspeed);
            return result;
        }
    }
}
