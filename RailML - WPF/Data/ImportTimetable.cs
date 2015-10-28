﻿using CsvFiles;
using RailML___WPF.NeuralNetwork.PreProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.Data
{
    class ImportTimetable
    {
        static List<TrainVariations> totalvariations = new List<TrainVariations>();
        static TrainVariations trainvariations;
        static DateTime date = new DateTime();
        public static void TimetableFromCsv(object sender, DoWorkEventArgs e)
        {
            if (DataContainer.model == null) { DataContainer.model = new railml(); }
            DataContainer.model.timetable = new timetable();
            Hashtable categories = new Hashtable();
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
                string category = entry.TrainOrigin + "to" + entry.TrainDestination;
                if (categories.ContainsKey(category))
                {
                    categories[category] = (int)categories[category] + 1;
                }
                else
                {
                    categories.Add(category, 1);
                    eCategory cat = new eCategory();
                    cat.id = category;
                    cat.name = category;
                    DataContainer.model.timetable.categories.Add(cat);
                }

                if(entry.TrainDate != date || entry.TrainCode != traincode)
                {
                    AddStopDelays(stopdelays, traincode, date);
                    stopdelays = new List<StopDelay>();
                    if(rep.traincode != null){trainvariations.AddRep(rep, date);}
                    if (entry.TrainCode != traincode) 
                    {
                        if(trainvariations != null){totalvariations.Add(trainvariations);}
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
                    Stop stop = new Stop() { location = entry.LocationCode, arrival = entry.ScheduledArrival, departure = entry.ScheduledDeparture};
                    rep.stops.Add(stop);
                    if(entry.Arrival != default(DateTime) && entry.Departure != default(DateTime))
                    {
                        StopDelay delay = new StopDelay() { location = entry.LocationCode, departuredelay = (entry.Departure - entry.ScheduledDeparture).TotalSeconds, arrivaldelay = (entry.Arrival - entry.ExpectedArrival).TotalSeconds };
                        stopdelays.Add(delay);
                    }
                }
                if (count == 100000)
                {
                    int q = 1;
                }
                if (count > 300000) { break; }
                count++;
                
            }

            ProcessTimetableVariations();

        }

        private static void AddStopDelays(List<StopDelay> stopdelays, string traincode, DateTime date)
        {
            foreach(DelayCombination comb in DataContainer.NeuralNetwork.DelayCombinations.list.Where(e => e.GetDate() == date && e.HasTrain(traincode)))
            {
                comb.AddStopDelays(stopdelays, traincode);
            }
        }


        private static void ProcessTimetableVariations()
        {
            foreach(TrainVariations variations in totalvariations)
            {
                eTrain train = new eTrain();
                train.id = variations.traincode;
                train.scope = tTrainScope.primary;
                List<eTrainPart> trainparts = new List<eTrainPart>();
                for(int i = 0; i < variations.dates.Count; i++)
                {
                    eTrainPart trainpart = new eTrainPart() { id = train.id + "-" + i.ToString() };
                    
                    var operatingperiod = CreateOperatingPeriod(variations.dates[i]);
                    trainpart.operatingPeriodRef = new eOperatingPeriodRef() {@ref = operatingperiod.id };
                    DataContainer.model.timetable.operatingPeriods.Add(operatingperiod);
                    eOcpTT departure = new eOcpTT() { ocpRef = variations.reps[i].origin };
                    departure.times.Add(new eArrivalDepartureTimes(){ departure = variations.reps[i].departuretime, scope = "scheduled"});
                    trainpart.ocpsTT.Add(departure); 
                    foreach(Stop stop in variations.reps[i].stops)
                    {
                        eOcpTT ocp = new eOcpTT() { ocpRef = stop.location };
                        ocp.times.Add(new eArrivalDepartureTimes() { arrival = stop.arrival, departure = stop.departure, scope = "scheduled" });
                        trainpart.ocpsTT.Add(ocp);
                    }
                    eOcpTT arrival = new eOcpTT(){ocpRef = variations.reps[i].destination};
                    arrival.times.Add(new eArrivalDepartureTimes(){arrival = variations.reps[i].arrivaltime, scope = "scheduled"});
                    trainpart.ocpsTT.Add(arrival);
                    trainparts.Add(trainpart);
                }

                for(int i = 0; i < trainparts.Count; i++)
                {
                    eTrainPartSequence seq = new eTrainPartSequence(){sequence = i.ToString()};
                    seq.trainPartRef.Add(new tTrainPartRef(){@ref = trainparts[i].id });
                    train.trainPartSequence.Add(seq);
                }
                DataContainer.model.timetable.trains.Add(train);
                DataContainer.model.timetable.trainParts.AddRange(trainparts);

            }


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
                //days[dayindex.IndexOf(date.DayOfWeek)]++;
            }
            //var missing = startdate.Range(enddate).Except(dates);
            //var exclude = new List<DateTime>();
            //foreach (DateTime missingdate in missing)
            //{
            //    if (days[dayindex.IndexOf(missingdate.DayOfWeek)] == 0)
            //    {
            //        exclude.Add(missingdate);
            //    }
            //}
            //missing = missing.Except(exclude);
            //string operatingcode = "";
            //foreach (int c in days)
            //{
            //    if (c > 0) { operatingcode += "1"; }
            //    else { operatingcode += "0"; }
            //}
            //eOperatingDay opday = new eOperatingDay() { startDate = startdate, endDate = enddate, operatingCode = operatingcode };
            //opperiod.operatingDay.Add(opday);
            //foreach (DateTime missingdate in missing)
            //{
            //    tSpecialService specialservice = new tSpecialService() { type = tSpecialServiceType.exclude, singleDate = missingdate };
            //    opperiod.specialService.Add(specialservice);
            //}
            DateTime tempdate = startdate;
            int j = 0;
            string bitmask = "";
            for (int i = 0; i < ((enddate - startdate).TotalDays + 1); i++ )
            {
                if (dates[j] == tempdate) { bitmask += "1"; j++;  }
                else {bitmask += "0";}
                tempdate = tempdate.AddDays(1);
            }
            opperiod.bitMask = bitmask;
            opperiod.startDate = startdate;
            opperiod.endDate = enddate;

            return opperiod;
        }


        private static Dictionary<DateTime, TimetableDay> CreateTimetableDict()
        {
            Dictionary<DateTime, TimetableDay> timetabledict = new Dictionary<DateTime,TimetableDay>();

            foreach(TrainVariations vars in totalvariations)
            {
                for(int i = 0; i < vars.dates.Count; i++)
                {
                    RuntimeRep rep = vars.reps[i];
                    for(int j = 0; j < vars.dates[i].Count; j++)
                    {
                        if(!timetabledict.ContainsKey(vars.dates[i][j]))
                        {
                            TimetableDay ttday = new TimetableDay();
                            ttday.date = vars.dates[i][j];
                            ttday.runtimes.Add(rep);
                            timetabledict.Add(vars.dates[i][j], ttday);
                        }
                        else
                        {
                            timetabledict[vars.dates[i][j]].runtimes.Add(rep);
                        }
                    }
                }
            }
            return timetabledict;
        }

        private void TimetableGrouping(Dictionary<DateTime, TimetableDay> timetabledict)
        {
            IEnumerable<IGrouping<TimetableDay, DateTime>> grouping = timetabledict.Values.GroupBy(e => e, e => e.date);
            foreach(IGrouping<TimetableDay, DateTime> group in grouping.Where(e => e.Count() > 50))
            {
                var opperiod = CreateOperatingPeriod(group.ToList<DateTime>());
                DataContainer.model.timetable.operatingPeriods.Add(opperiod);
            }

        }

        private void CreateTimeTable(Dictionary<DateTime, TimetableDay> timetabledict)
        {

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
            for (int i = 0; i < 10; i++ )
            {
                int j = i * runtimes.Count / 10;
                hash += (int)runtimes[j].departuretime.Ticks;
            }
            return hash;
        }
    }
    class TrainVariations
    {
        public List<RuntimeRep> reps {get; set;}
        public List<List<DateTime>> dates { get; set; }
        public string traincode { get; set; }

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
