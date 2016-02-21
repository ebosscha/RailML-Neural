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
    class VertexDepartureCountInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "VertexDepartureCountInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 6; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Origin <5 Minutes", "Origin 5-15 Minutes", "Origin 15-30 Minutes", "Destination <5 Minutes", "Destination 5-15 Minutes", "Destination 15-30 Minutes" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public VertexDepartureCountInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            foreach(VertexTrainRepresentation other in new List<SimplifiedGraphVertex>(){rep.Edge.Origin, rep.Edge.Destination}
                .SelectMany(x => x.Trains).Where(x => x.TrainHeaderCode != rep.TrainHeaderCode))
            {
                DateTime thisTime;
                int i = 0;
                if((rep.Direction == DirectionEnum.Down && other.Vertex == rep.Edge.Destination)
                    ||(rep.Direction == DirectionEnum.Up && other.Vertex == rep.Edge.Origin))
                {
                    i = 3;
                    thisTime = rep.ScheduledArrivalTime;
                }
                else
                {
                    thisTime = rep.ScheduledDepartureTime;
                }

                foreach (DateTime othertime in new List<DateTime>() { other.PredictedArrivalTime, other.PredictedDepartureTime })
                {
                    TimeSpan Diff = thisTime - othertime;
                    if (Diff.TotalMinutes < 0)
                    {
                        continue;
                    }
                    else if (Diff.TotalMinutes < 5)
                    {
                        result[i]++;
                    }
                    else if (Diff.TotalMinutes < 15)
                    {
                        result[i + 1]++;
                    }
                    else if (Diff.TotalMinutes < 30)
                    {
                        result[i + 2]++;
                    }
                }
            }

            return result;
        }
    }
}
