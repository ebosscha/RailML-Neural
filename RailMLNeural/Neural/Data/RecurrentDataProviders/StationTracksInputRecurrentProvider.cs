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
    class StationTracksInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "StationTracksInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 2; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Origin Station Track Count", "Destination Station Track Count" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public StationTracksInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            if(rep.Direction == DirectionEnum.Down)
            {
                result[0] = rep.Edge.Origin.TrackCount;
                result[1] = rep.Edge.Destination.TrackCount;
            }
            else
            {
                result[1] = rep.Edge.Origin.TrackCount;
                result[0] = rep.Edge.Destination.TrackCount;
            }
            
            return result;


        }
    }
}
