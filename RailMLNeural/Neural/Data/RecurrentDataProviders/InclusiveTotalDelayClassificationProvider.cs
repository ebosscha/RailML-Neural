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
    class InclusiveTotalDelayClassificationProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return false; } }
        const string _name = "InclusiveTotalDelayClassificationProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 8; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "<5 min", "5-10 min", "10-15 min", "15-20 min", "20-30 min", "30-45 min", "45-60 min", ">60 min" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public InclusiveTotalDelayClassificationProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            List<double> list = rep.Edge.Graph.Edges
                .SelectMany(x => x.Trains)
                .Where(x => x.IsRelevant && x.Next == null)
                .Select(x => (x.IdealArrivalTime - x.ScheduledArrivalTime).TotalMinutes).ToList();
            double delay = list.Sum();
            int i = 0;
            if(delay < 5)
            { i = 0; }
            else if(delay < 10)
            { i = 1; }
            else if (delay < 15)
            { i = 2; }
            else if (delay < 20)
            { i = 3; }
            else if (delay < 30)
            { i = 4; }
            else if (delay < 45)
            { i = 5; }
            else if(delay < 60)
            { i = 6; }
            else
            { i = 7; }
            result[i] = 1;
            return result;
        }

        public override void Update(double[] output, EdgeTrainRepresentation rep)
        {
            return;
        }
    }
}
