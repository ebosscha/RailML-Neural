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
    class PreviousDelaySizeInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "PreviousDelaySizeInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Previous Arrival Delay" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public PreviousDelaySizeInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            if(rep.Previous == null)
            {
                result[0] = 0;
            }
            else
            {
                result[0] = (rep.Previous.PredictedArrivalTime - rep.Previous.ScheduledArrivalTime).TotalSeconds;
            }
            return result;
        }
    }
}
