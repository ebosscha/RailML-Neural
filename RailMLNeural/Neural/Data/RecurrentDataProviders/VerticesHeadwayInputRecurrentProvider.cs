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
            //result[0] = 3600; result[1] = 3600;
            //SimplifiedGraphVertex v1;
            //SimplifiedGraphVertex v2;
            //if(rep.Direction == DirectionEnum.Down)
            //{
            //    v1 = rep.Edge.Origin;
            //    v2 = rep.Edge.Destination;
            //}
            //else
            //{
            //    v1 = rep.Edge.Destination;
            //    v2 = rep.Edge.Origin;
            //}
            //int index = v1.Trains.FindIndex(x => x.TrainHeaderCode == rep.TrainHeaderCode);
            //if(index > 0)
            //{
            //    result[0] = (rep.ScheduledDepartureTime - v1.Trains[index - 1].PredictedDepartureTime).TotalSeconds;
            //}
            //index = v2.Trains.FindIndex(x => x.TrainHeaderCode == rep.TrainHeaderCode);
            //if(index > 0)
            //{
            //    result[1] = (rep.ScheduledArrivalTime - v2.Trains[index - 1].PredictedArrivalTime).TotalSeconds;
            //}
            return result;


        }
    }
}
