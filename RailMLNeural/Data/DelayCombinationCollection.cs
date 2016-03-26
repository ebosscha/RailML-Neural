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
                return dict.Values.Where(x => x != null).Sum(x => x.Count);
            }
        }

        public int TotalDelayCount
        {
            get
            {
                return dict.Values.Where(x => x != null).Sum(x => x.Sum(y => y.primarydelays.Count + y.secondarydelays.Count));
            }
        }

        public double AverageCountPerCombination
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .SelectMany(x => x)
                        .Average(x => x.primarydelays.Count + x.secondarydelays.Count);
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

                    int[] array = new int[dict.Values.Where(x => x != null).Max(x => x.Sum(y => y.secondarydelays.Count))];
                    foreach (var x in dict.Values.Where(x => x != null))
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
                    return dict.Values.Where(x => x != null).Average(x => x.Count);
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
                    return dict.Values.Where(x => x != null)
                        .SelectMany(x => x
                            .SelectMany(y => y.primarydelays.Union(y.secondarydelays))).
                            Average(z => z.destinationdelay);
                }
                return 0;
            }
        }




    }

}
