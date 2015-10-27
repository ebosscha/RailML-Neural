using CsvFiles;
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





                //if (count > 3000000) { break; }
                count++;
                
            }

            CreateGeneralTimetable();
        }


        public static void ProcessTimetableVariations()
        {
            foreach(TrainVariations variations in totalvariations)
            {
                eTrain train = new eTrain();
                train.id = variations.traincode;

            }
        }

        public static void CreateGeneralTimetable()
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

            IEnumerable<IGrouping<TimetableDay, DateTime>> grouping = timetabledict.Values.GroupBy(e => e, e => e.date);
            foreach(IGrouping<TimetableDay, DateTime> group in grouping.Where(e => e.Count() > 50))
            {
                DateTime startdate = DateTime.MaxValue;
                DateTime enddate = DateTime.MinValue;
                List<DayOfWeek> dayindex = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
                int[] days = new int[7] {0,0,0,0,0,0,0};
                foreach(DateTime date in group )
                {
                    if (date < startdate) { startdate = date; }
                    if (date > enddate) { enddate = date; }
                    days[dayindex.IndexOf(date.DayOfWeek)]++;
                }
                var missing = startdate.Range(enddate).Except(group);
                var exclude = new List<DateTime>();
                foreach(DateTime missingdate in missing)
                {
                    if(days[dayindex.IndexOf(missingdate.DayOfWeek)] == 0)
                    {
                        exclude.Add(missingdate);
                    }
                }
                missing = missing.Except(exclude);
                eTimetablePeriod ttperiod = new eTimetablePeriod();
                DataContainer.IDGenerator(ttperiod);
                eOperatingPeriod opperiod = new eOperatingPeriod();
                DataContainer.IDGenerator(opperiod);
                opperiod.timetablePeriodRef = ttperiod.id;
                string operatingcode = "";
                foreach(int c in days)
                {
                    if (c > 0) { operatingcode += "1"; }
                    else { operatingcode += "0"; }
                }
                eOperatingDay opday = new eOperatingDay() { startDate = startdate, endDate = enddate, operatingCode = operatingcode };
                opperiod.operatingDay.Add(opday);
                foreach(DateTime missingdate in missing)
                {
                    tSpecialService specialservice = new tSpecialService(){type = tSpecialServiceType.exclude, singleDate = missingdate};
                    opperiod.specialService.Add(specialservice);
                }
                DataContainer.model.timetable.timetablePeriods.Add(ttperiod);
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

        public bool Equals(RuntimeRep other)
        {
            return this.traincode == other.traincode &&
                this.departuretime == other.departuretime &&
                this.arrivaltime == other.arrivaltime &&
                this.origin == other.origin &&
                this.traincode == other.traincode;
        }

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
