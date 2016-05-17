using RailMLNeural.Data;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data.RecurrentDataProviders
{
    /// <summary>
    /// Computes the amount of trains leaving from both ends of the link, after the current train.
    /// </summary>
    [Serializable]
    class MoreEdgeDepartureCountInputProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "MoreEdgeDepartureCountInputProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 20; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get; set; }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public MoreEdgeDepartureCountInputProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
            Map = new string[20].ToList();
            for (int i = 0; i < 9; i++ )
            {
                Map[i] = "SameDir <" + (i + 1) * 3;
                Map[i + 10] = "OtherDir <" + (i + 1) * 3;
            }
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            foreach(EdgeTrainRepresentation other in rep.Edge.Trains.Where(x => x.IsRelevant && x.TrainHeaderCode != rep.TrainHeaderCode))
            {
                bool append = false;
                int i = 0;
                if(other.Direction != rep.Direction)
                {
                    i = 10;
                }
                TimeSpan Diff = other.ForecastedDepartureTime - rep.ForecastedDepartureTime;
                if(Diff.TotalMinutes < 0)
                {
                    append = false;
                }
                else if(Diff.TotalMinutes < 30)
                {
                    append = true;
                    i += (int)Math.Floor(Diff.TotalMinutes / 3);
                }
               
                if(append)
                {
                    result[i]++;
                }
            }

            return result;
        }
    }
}
