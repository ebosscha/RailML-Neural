using CsvFiles;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RailMLNeural.Data
{
    class ImportTimetable
    {
        static int unhandled = 0;
        //static List<TrainVariations> totalvariations = new List<TrainVariations>();
        static TrainVariations trainvariations;
        static DateTime date = new DateTime();
        public static void TimetableFromCsv(object sender, DoWorkEventArgs e)
        {
            DateTime ThresholdDateLower;
            DateTime ThresholdDateUpper;

            if (DataContainer.Settings.UseDateFilter)
            {
                ThresholdDateLower = DataContainer.Settings.DataStartDate;
                ThresholdDateUpper = DataContainer.Settings.DataEndDate;
            }
            else
            {
                ThresholdDateLower = new DateTime(2010, 1, 1);
                ThresholdDateUpper = DateTime.Now;
            }
            if (DataContainer.model == null) { DataContainer.model = new railml(); }
            DataContainer.model.timetable = new timetable();
            Dictionary<string, int> categories = new Dictionary<string, int>();
            string filename = e.Argument as string;
            BackgroundWorker worker = sender as BackgroundWorker;

            CsvDefinition def = new CsvDefinition() { FieldSeparator = ',' };
            CsvFileReader<TimetableEntry> reader = new CsvFileReader<TimetableEntry>(filename, def);
            int count = 0;
            RuntimeRep rep = new RuntimeRep();
            List<StopDelay> stopdelays = new List<StopDelay>();
            string traincode = "";
            foreach (TimetableEntry entry in reader)
            {
                if (entry.TrainDate < ThresholdDateLower || entry.TrainDate > ThresholdDateUpper ) { count++; continue; }




                string category = entry.TrainOrigin + "to" + entry.TrainDestination;
                if (categories.ContainsKey(category))
                {
                    categories[category] = (int)categories[category] + 1;
                }
                else
                {
                    categories.Add(category, 1);
                }

                if (entry.TrainDate != date || entry.TrainCode != traincode)
                {
                    AddStopDelays(stopdelays, traincode, date);
                    stopdelays = new List<StopDelay>();
                    if (rep.traincode != null) { trainvariations.AddRep(rep, date); }
                    if (entry.TrainCode != traincode)
                    {
                        if (trainvariations != null)
                        {
                            string cat = categories.FirstOrDefault(x => x.Value == categories.Values.Max()).Key;
                            trainvariations.category = cat;
                            categories = new Dictionary<string, int>();
                            if (!DataContainer.model.timetable.rosterings.Any(x => x.name == cat))
                            {
                                eRostering roster = new eRostering()
                                {
                                    name = cat,
                                    scope = "scheduled"
                                };
                                DataContainer.IDGenerator(roster);
                                DataContainer.model.timetable.rosterings.Add(roster);

                            }
                            ProcessTimetableVariations(trainvariations);
                        }
                        trainvariations = new TrainVariations();
                        trainvariations.traincode = entry.TrainCode;
                        worker.ReportProgress(0, new string[] { traincode, count.ToString() });
                        traincode = entry.TrainCode;
                    }

                    rep = new RuntimeRep();
                    date = entry.TrainDate;
                    rep.traincode = entry.TrainCode;
                    rep.destination = entry.TrainDestination;
                    rep.origin = entry.TrainOrigin;

                }

                if (entry.LocationType == "O") { rep.departuretime = default(DateTime).Add(entry.ScheduledDeparture.TimeOfDay); }
                else if (entry.LocationType == "D") { rep.arrivaltime = default(DateTime).Add(entry.ScheduledArrival.TimeOfDay); }
                else if (entry.LocationType == "S")
                {
                    Stop stop = new Stop() { location = entry.LocationCode, arrival = entry.ScheduledArrival, departure = entry.ScheduledDeparture };
                    rep.stops.Add(stop);
                    if (entry.Arrival != default(DateTime) && entry.Departure != default(DateTime))
                    {
                        StopDelay delay = new StopDelay() { location = entry.LocationCode, departuredelay = (entry.Departure - entry.ScheduledDeparture).TotalSeconds, arrivaldelay = (entry.Arrival - entry.ExpectedArrival).TotalSeconds };
                        stopdelays.Add(delay);
                    }
                }

                //if (count > 300000) { break; }
                count++;

            }
            // Append the last trainvariation to the totalvariations list
            if (trainvariations != null)
            {
                string cat = categories.FirstOrDefault(x => x.Value == categories.Values.Max()).Key;
                trainvariations.category = cat;
                categories = new Dictionary<string, int>();
                ProcessTimetableVariations(trainvariations);
                if (!DataContainer.model.timetable.rosterings.Any(x => x.name == cat))
                {
                    eRostering roster = new eRostering()
                    {
                        name = cat,
                        scope = "scheduled"
                    };
                    DataContainer.IDGenerator(roster);
                    DataContainer.model.timetable.rosterings.Add(roster);

                }
            }

            e.Result = unhandled;




        }

        private static void AddStopDelays(List<StopDelay> stopdelays, string traincode, DateTime date)
        {
            if (stopdelays.Count > 0 && DataContainer.DelayCombinations.dict.ContainsKey(date) && DataContainer.DelayCombinations.dict[date].Count != 0)
            {
                foreach (DelayCombination comb in DataContainer.DelayCombinations.dict[date].Where(e => e.GetDate() == date && e.HasTrain(traincode)))
                {
                    comb.AddStopDelays(stopdelays, traincode);
                }
            }
        }


        private static void ProcessTimetableVariations(TrainVariations variations)
        {
            eTrain train = new eTrain();
            train.id = variations.traincode;
            train.description = variations.category;
            train.scope = tTrainScope.primary;
            List<eTrainPart> trainparts = new List<eTrainPart>();

            for (int i = 0; i < variations.dates.Count; i++)
            {
                try
                {
                    eTrainPart trainpart = new eTrainPart() { id = train.id + "-" + i.ToString() };

                    var operatingperiod = CreateOperatingPeriod(variations.dates[i]);
                    trainpart.operatingPeriodRef = new eOperatingPeriodRef() { @ref = operatingperiod.id };
                    DataContainer.model.timetable.operatingPeriods.Add(operatingperiod);
                    eOcpTT departure = new eOcpTT() { ocpRef = DataContainer.model.infrastructure.operationControlPoints.Single(x => x.code == variations.reps[i].origin).id };
                    departure.times.Add(new eArrivalDepartureTimes() { departure = variations.reps[i].departuretime, scope = "scheduled" });
                    trainpart.ocpsTT.Add(departure);
                    foreach (Stop stop in variations.reps[i].stops)
                    {
                        eOcpTT ocp = new eOcpTT() { ocpRef = DataContainer.model.infrastructure.operationControlPoints.Single(x => x.code == stop.location).id };
                        ocp.times.Add(new eArrivalDepartureTimes() { arrival = stop.arrival, departure = stop.departure, scope = "scheduled" });
                        trainpart.ocpsTT.Add(ocp);
                    }
                    eOcpTT arrival = new eOcpTT() { ocpRef = DataContainer.model.infrastructure.operationControlPoints.Single(x => x.code == variations.reps[i].destination).id };
                    arrival.times.Add(new eArrivalDepartureTimes() { arrival = variations.reps[i].arrivaltime, scope = "scheduled" });
                    trainpart.ocpsTT.Add(arrival);
                    trainparts.Add(trainpart);

                    tBlockPart blockpart = new tBlockPart()
                    {
                        trainPartRef = trainpart.id,
                        startOcpRef = variations.reps[i].origin,
                        endOcpRef = variations.reps[i].destination,
                        operatingPeriodRef = operatingperiod.id,
                        begin = variations.reps[i].departuretime,
                        end = variations.reps[i].arrivaltime
                    };
                    DataContainer.IDGenerator(blockpart);
                    eRostering roster = DataContainer.model.timetable.rosterings.Single(e => e.name == variations.category);
                    roster.blockParts.blockPart.Add(blockpart);
                    eBlock block = new eBlock() { code = train.id };
                    DataContainer.IDGenerator(block);
                    eBlockPartSequence seq = new eBlockPartSequence() { sequence = "1" };
                    seq.blockPartRef.Add(new tBlockPartRef() { @ref = blockpart.id });
                    block.blockPartSequence.Add(seq);
                    roster.blocks.Add(block);
                    tCirculation circ = new tCirculation()
                    {
                        startDate = operatingperiod.startDate,
                        endDate = operatingperiod.endDate,
                        operatingPeriodRef = operatingperiod.id,
                        blockRef = block.id
                    };
                    roster.circulations.Add(circ);
                }
                catch
                {
                    unhandled++;
                }




            }

            for (int i = 0; i < trainparts.Count; i++)
            {
                eTrainPartSequence seq = new eTrainPartSequence() { sequence = i.ToString() };
                seq.trainPartRef.Add(new tTrainPartRef() { @ref = trainparts[i].id });
                train.trainPartSequence.Add(seq);
            }
            DataContainer.model.timetable.trains.Add(train);
            DataContainer.model.timetable.trainParts.AddRange(trainparts);

        }





        private static eOperatingPeriod CreateOperatingPeriod(List<DateTime> dates)
        {
            eOperatingPeriod opperiod = new eOperatingPeriod();
            DataContainer.IDGenerator(opperiod);
            DateTime startdate = DateTime.MaxValue;
            DateTime enddate = DateTime.MinValue;
            List<DayOfWeek> dayindex = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
            int[] days = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
            foreach (DateTime date in dates)
            {
                if (date < startdate) { startdate = date; }
                if (date > enddate) { enddate = date; }
                
            }
            DateTime tempdate = startdate;
            int j = 0;
            string bitmask = "";
            for (int i = 0; i < ((enddate - startdate).TotalDays + 1); i++)
            {
                if (dates[j] == tempdate) { bitmask += "1"; j++; }
                else { bitmask += "0"; }
                tempdate = tempdate.AddDays(1);
            }
            opperiod.bitMask = bitmask;
            opperiod.startDate = startdate;
            opperiod.endDate = enddate;

            return opperiod;
        }


        //private static Dictionary<DateTime, TimetableDay> CreateTimetableDict()
        //{
        //    Dictionary<DateTime, TimetableDay> timetabledict = new Dictionary<DateTime,TimetableDay>();

        //    foreach(TrainVariations vars in totalvariations)
        //    {
        //        for(int i = 0; i < vars.dates.Count; i++)
        //        {
        //            RuntimeRep rep = vars.reps[i];
        //            for(int j = 0; j < vars.dates[i].Count; j++)
        //            {
        //                if(!timetabledict.ContainsKey(vars.dates[i][j]))
        //                {
        //                    TimetableDay ttday = new TimetableDay();
        //                    ttday.date = vars.dates[i][j];
        //                    ttday.runtimes.Add(rep);
        //                    timetabledict.Add(vars.dates[i][j], ttday);
        //                }
        //                else
        //                {
        //                    timetabledict[vars.dates[i][j]].runtimes.Add(rep);
        //                }
        //            }
        //        }
        //    }
        //    return timetabledict;
        //}

        private void TimetableGrouping(Dictionary<DateTime, TimetableDay> timetabledict)
        {
            IEnumerable<IGrouping<TimetableDay, DateTime>> grouping = timetabledict.Values.GroupBy(e => e, e => e.date);
            foreach (IGrouping<TimetableDay, DateTime> group in grouping.Where(e => e.Count() > 50))
            {
                var opperiod = CreateOperatingPeriod(group.ToList<DateTime>());
                DataContainer.model.timetable.operatingPeriods.Add(opperiod);
            }

        }

    }

    class RuntimeRep : IEquatable<RuntimeRep>
    {
        public DateTime departuretime { get; set; }
        public DateTime arrivaltime { get; set; }
        public string origin { get; set; }
        public string destination { get; set; }
        public string traincode { get; set; }
        public List<Stop> stops { get; set; }

        public RuntimeRep()
        {
            stops = new List<Stop>();
        }
        public bool Equals(RuntimeRep other)
        {
            return this.traincode == other.traincode &&
                this.departuretime == other.departuretime &&
                this.arrivaltime == other.arrivaltime &&
                this.origin == other.origin &&
                this.traincode == other.traincode;
        }

    }

    class Stop
    {
        public string location { get; set; }
        public DateTime arrival { get; set; }
        public DateTime departure { get; set; }
    }

    class TimetableDay : IEquatable<TimetableDay>
    {
        public DateTime date { get; set; }
        public List<RuntimeRep> runtimes { get; set; }

        public TimetableDay()
        {
            runtimes = new List<RuntimeRep>();
        }

        public bool Equals(TimetableDay other)
        {
            return runtimes.SequenceEqual(other.runtimes);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            for (int i = 0; i < 10; i++)
            {
                int j = i * runtimes.Count / 10;
                hash += (int)runtimes[j].departuretime.Ticks;
            }
            return hash;
        }
    }
    class TrainVariations
    {
        public List<RuntimeRep> reps { get; set; }
        public List<List<DateTime>> dates { get; set; }
        public string traincode { get; set; }
        public string category { get; set; }

        public TrainVariations()
        {
            reps = new List<RuntimeRep>();
            dates = new List<List<DateTime>>();
        }

        public void AddRep(RuntimeRep rep, DateTime date)
        {
            if (reps.Contains(rep))
            {
                dates[reps.IndexOf(rep)].Add(date);
            }
            else
            {
                reps.Add(rep);
                List<DateTime> tempdates = new List<DateTime>();
                tempdates.Add(date);
                dates.Add(tempdates);
            }
        }
    }

    public static class extensions
    {
        public static IEnumerable<DateTime> Range(this DateTime startdate, DateTime enddate)
        {
            return Enumerable.Range(0, (int)(enddate - startdate).TotalDays + 1).Select(i => startdate.AddDays(i));
        }
    }
}
