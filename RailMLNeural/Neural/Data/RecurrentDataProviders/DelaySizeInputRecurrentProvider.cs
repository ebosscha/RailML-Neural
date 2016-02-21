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
    class DelaySizeInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "DelaySizeInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 2; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Departure Delay", "Arrival Delay" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public DelaySizeInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[1] = (rep.PredictedArrivalTime - rep.ScheduledArrivalTime).TotalMinutes;
            result[0] = (rep.PredictedDepartureTime - rep.ScheduledDepartureTime).TotalMinutes;
            return result;
        }
    }
}
