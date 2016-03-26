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
    class ForecastedTimesInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "ForecastedTimesInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 2; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Forecasted Departure", "Forecasted Arrival"  }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public ForecastedTimesInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[0] = rep.ForecastedDepartureTime.TimeOfDay.TotalHours;
            result[1] = rep.ForecastedArrivalTime.TimeOfDay.TotalHours;
            return result;
        }
    }
}
