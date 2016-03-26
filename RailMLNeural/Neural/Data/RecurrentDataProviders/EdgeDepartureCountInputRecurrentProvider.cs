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
    class EdgeDepartureCountInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "EdgeDepartureCountInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 6; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "SameDir <5", "SameDir <15", "SameDir <30", "OtherDir <5", "OtherDir <15", "OtherDir <30" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public EdgeDepartureCountInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
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
                    i = 3;
                }
                TimeSpan Diff = other.ForecastedDepartureTime - rep.ForecastedDepartureTime;
                if(Diff.TotalMinutes < 0)
                {
                    append = false;
                }
                else if(Diff.TotalMinutes < 5)
                {
                    append = true;
                }
                else if (Diff.TotalMinutes < 15)
                {
                    i += 1;
                    append = true;
                }
                else if(Diff.TotalMinutes < 30)
                {
                    i += 2;
                    append = true;
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
