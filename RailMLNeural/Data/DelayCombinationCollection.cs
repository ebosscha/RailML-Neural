using ProtoBuf;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Data
{
    [Serializable]
    [ProtoContract]
    public class DelayCombinationCollection
    {
        [ProtoMember(1)]
        public Dictionary<DateTime, List<DelayCombination>> dict;

        public DelayCombinationCollection()
        {
            dict = new Dictionary<DateTime, List<DelayCombination>>();
        }

        public int CombinationCount
        {
            get
            {
                return dict.Values.Sum(x => x.Count);
            }
        }

        public int TotalDelayCount
        {
            get
            {
                return dict.Values.Sum(x => x.Sum(y => y.primarydelays.Count + y.secondarydelays.Count));
            }
        }

        public double AverageCountPerCombination
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Average(x => x.Sum(y => y.primarydelays.Count + y.secondarydelays.Count));
                }
                return 0;
            }
        }

        public List<int> DelayCountHistogram
        {
            get
            {
                if (dict.Values.Count > 0)
                {

                    int[] array = new int[dict.Values.Max(x => x.Sum(y => y.secondarydelays.Count))];
                    foreach (var x in dict.Values)
                    {
                        foreach (var y in x)
                        {
                            array[y.secondarydelays.Count]++;
                        }
                    }
                    return array.ToList();
                }
                return null;
            }
        }

        public double AverageCombinationsPerDay
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Average(x => x.Count);
                }
                return 0;
            }
        }

        public double AverageDestinationDelay
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Average(x => x.Average(y => (y.primarydelays.Average(z => z.destinationdelay) + y.secondarydelays.Average(z => z.destinationdelay)) / 2));
                }
                return 0;
            }
        }




    }

}
