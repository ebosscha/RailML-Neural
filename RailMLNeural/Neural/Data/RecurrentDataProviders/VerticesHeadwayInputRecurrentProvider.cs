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
    class VerticesHeadwayInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "VerticesHeadwayInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 2; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Origin OCP Headway", "Destination OCP Headway"  }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public VerticesHeadwayInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            result[0] = 1;
            result[1] = 1;
            //var other = rep.Origin.Trains.Where(x => x.TrainHeaderCode != rep.TrainHeaderCode)
            //    .Where(x => (x.Arrival != null && x.ForecastedArrivalTime < rep.ForecastedDepartureTime) ||
            //        (x.Departure != null && x.ForecastedDepartureTime < rep.ForecastedDepartureTime))
            //        .OrderBy(x => x.Arrival != null ? x.ForecastedArrivalTime : x.ForecastedDepartureTime)
            //        .LastOrDefault();
            var other = rep.Origin.Trains.Where(x => x.IsRelevant && x.TrainHeaderCode != rep.TrainHeaderCode)
                .SelectMany(x => new DateTime[]{x.ForecastedArrivalTime, x.ForecastedDepartureTime})
                .Where(x => x != default(DateTime) && x < rep.ForecastedDepartureTime)
                .DefaultIfEmpty().Max();
            result[0] = other == default(DateTime) ? 1 : (rep.ForecastedDepartureTime - other).TotalHours;
            other = rep.Destination.Trains.Where(x => x.IsRelevant && x.TrainHeaderCode != rep.TrainHeaderCode)
                .SelectMany(x => new DateTime[] { x.ForecastedArrivalTime, x.ForecastedDepartureTime })
                .Where(x => x != default(DateTime) && x < rep.ForecastedArrivalTime)
                .DefaultIfEmpty().Max();
            result[1] = other == default(DateTime) ? 1 : (rep.ForecastedArrivalTime - other).TotalHours;
            return result;


        }
    }
}
