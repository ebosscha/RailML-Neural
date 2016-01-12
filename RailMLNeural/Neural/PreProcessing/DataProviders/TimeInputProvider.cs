using Encog.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.PreProcessing.DataProviders
{
    class TimeInputProvider : IDataProvider
    {
        private const string name = "TimeInputProvider";
        public string Name { get { return name; } }
        /// <summary>
        /// Integer representing the node count of this module
        /// </summary>
        private int _size;
        public int Size { get { return _size; } }

        /// <summary>
        /// Representing the labels for the nodes
        /// </summary>
        private List<string> _map = new List<string>();
        public List<string> Map
        { get { return _map; } }

        /// <summary>
        /// Represeting the index of the input/output IMLData where this specific data starts
        /// </summary>
        public int LowerIndex { get; set; }

        private long[] lowerticks;
        private long step;

        /// <summary>
        /// Enum representing the type of normalization applied to this input/output section
        /// </summary>
        public Normalization.NormalizationTypeEnum NormalizationType { get; set; }

        public TimeInputProvider(int ClassCount)
        {
            _size = ClassCount;
            step = new TimeSpan(24, 0, 0).Ticks / _size;
            lowerticks = new long[_size];
            for(int i = 0; i < _size; i++)
            {
                lowerticks[i] = i * step;
                string label = (new TimeSpan(lowerticks[i]).ToString() + " - " + new TimeSpan(lowerticks[i] + step).ToString());
                _map.Add(label);
            }
           
        }

        public double[] Process(DelayCombination dc)
        {
            double[] result = new double[Size];
            foreach(Delay d in dc.primarydelays)
            {
                long x = (d.ActualArrival - d.ActualDeparture ).Ticks / 2;
                DateTime average = new DateTime(x).Add(d.ActualDeparture.TimeOfDay);
                int index = lowerticks.ToList().IndexOf(lowerticks.First((e) => x < e));
                result[index] = 1;
            }
            return result;
        }

        public List<Tuple<string, dynamic>> PublishOutput(IMLData d)
        {
            throw new NotImplementedException();
        }

    }
}
