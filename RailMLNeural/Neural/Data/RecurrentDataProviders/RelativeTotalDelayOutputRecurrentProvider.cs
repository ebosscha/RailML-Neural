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
    class RelativeTotalDelayOutputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return false; } }
        const string _name = "RelativeTotalDelayOutputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Relative total Knock-on Delay" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public RelativeTotalDelayOutputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            List<double> list = rep.Edge.Graph.Edges
                .SelectMany(x => x.Trains)
                .Where(x => x.IsRelevant && !x.IsHandled && x.Next == null)
                .Select(x => (x.IdealArrivalTime - x.ScheduledArrivalTime).TotalHours).ToList();
            result[0] = list.Sum() / (rep.IdealArrivalTime - rep.ScheduledArrivalTime).TotalHours;
            return result;
        }

        public override void Update(double[] output, EdgeTrainRepresentation rep)
        {
            return;
        }
    }
}
