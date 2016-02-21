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
    class ScheduledTimesInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "ScheduledTimesInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 2; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Scheduled Departure", "Scheduled Arrival"  }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public ScheduledTimesInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[0] = rep.ScheduledDepartureTime.TimeOfDay.TotalHours;
            result[1] = rep.ScheduledArrivalTime.TimeOfDay.TotalHours;
            return result;
        }
    }
}
