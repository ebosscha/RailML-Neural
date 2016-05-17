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
    class MoreVertexDepartureCountInputProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "MoreDepartureCountInputProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 20; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get; set; }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public MoreVertexDepartureCountInputProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
            Map = new string[20].ToList();
            for (int i = 0; i < 9; i++)
            {
                Map[i] = "SameDir <" + (i + 1) * 3;
                Map[i + 10] = "OtherDir <" + (i + 1) * 3;
            }
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            foreach(VertexTrainRepresentation other in new List<SimplifiedGraphVertex>(){rep.Edge.Origin, rep.Edge.Destination}
                .SelectMany(x => x.Trains).Where(x => x.IsRelevant && x.TrainHeaderCode != rep.TrainHeaderCode))
            {
                DateTime thisTime;
                int i = 0;
                if((rep.Direction == DirectionEnum.Down && other.Vertex == rep.Edge.Destination)
                    ||(rep.Direction == DirectionEnum.Up && other.Vertex == rep.Edge.Origin))
                {
                    i = 10;
                    thisTime = rep.ForecastedArrivalTime;
                }
                else
                {
                    thisTime = rep.ForecastedDepartureTime;
                }

                foreach (DateTime othertime in new List<DateTime>() { other.ForecastedArrivalTime, other.ForecastedDepartureTime })
                {
                    TimeSpan Diff = othertime - thisTime;
                    if (Diff.TotalMinutes < 0)
                    {
                        continue;
                    }
                    else if (Diff.TotalMinutes < 30)
                    {
                        result[i + (int)Math.Floor(Diff.TotalMinutes / 3)]++;
                    }
                }
            }

            return result;
        }
    }
}
