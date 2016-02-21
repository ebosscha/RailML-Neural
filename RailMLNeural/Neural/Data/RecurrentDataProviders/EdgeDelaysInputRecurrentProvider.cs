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
    class EdgeDelaysInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "EdgeDelaysInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Total Delays on Edge" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public EdgeDelaysInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            for(int i = rep.Edge.Trains.IndexOf(rep)-1; i > -1; i-- )
            {
                result[0] += (rep.Edge.Trains[i].PredictedDepartureTime - rep.Edge.Trains[i].ScheduledDepartureTime).TotalSeconds;
            }
            return result;


        }
    }
}
