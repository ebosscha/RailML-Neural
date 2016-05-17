using ProtoBuf;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public double AverageTotalPrimaryDelays
        {
            get
            {
                if(dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .Average(x => x.Sum(y => y.primarydelays
                            .Sum(z => z.destinationdelay )));
                }
                return 0;
            }
        }

        public double AverageTotalFirstOrderDelays
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .Average(x => x.Sum(y => y.secondarydelays
                            .Where(z => y.primarydelays.Any(q => q.traincode == z.WLCheader))
                            .Sum(z => z.destinationdelay)));
                }
                return 0;
            }
        }

        public double AverageTotalSecondOrderDelays
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .Average(x => x.Sum(y => y.secondarydelays
                            .Where(z => !y.primarydelays.Any(q => q.traincode == z.WLCheader))
                            .Sum(z => z.destinationdelay)));
                }
                return 0;
            }
        }

        public double AverageIsDelayedPercentage
        {
            get
            {
                if(dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .SelectMany(x => x)
                        .SelectMany(x => x.primarydelays.Concat(x.secondarydelays))
                        .Count()
                        / (double)TrainCount;

                }
                return 0;
            }
        }

        public double AverageIsPrimaryDelayedPercentage
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .SelectMany(x => x)
                        .SelectMany(x => x.primarydelays)
                        .Count()
                        / (double)TrainCount;

                }
                return 0;
            }
        }

        public double HasKnockOnDelayedPercentage
        {
            get
            {
                if (dict.Values.Count > 0)
                {
                    return dict.Values.Where(x => x != null)
                        .SelectMany(x => x)
                        .Where(x => x.secondarydelays.Count > 0)
                        .Count()
                        /
                        (double)dict.Values.Where(x => x != null)
                        .SelectMany(x => x)
                        .Count();
                }
                return 0;
            }
        }

        public int TrainCount
        {
            get
            {
                if(DataContainer.model.timetable != null)
                {
                    int traincount = 0;
                    var dates = DataContainer.model.timetable.GetDates();
                    Parallel.ForEach(dates, (date) =>
                    {
                        var trains = DataContainer.model.timetable.GetTrainsByDay(date);
                        Interlocked.Add(ref traincount, trains.Count);
                    });
                    return traincount;
                }
                return 0;
            }
        }





    }

}
