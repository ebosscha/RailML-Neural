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
        static TrainVariations trainvariations = new TrainVariations();
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
            int count2 = 0;
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
                    trainvariations.AddRep(rep, date);
                    if (entry.TrainCode != traincode) 
                    {
                        totalvariations.Add(trainvariations);
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
                if (entry.LocationType == "D") { rep.arrivaltime = default(DateTime).Add(entry.ScheduledArrival.TimeOfDay); }


               



                count++;
                
            }

            ProcessTimetableVariations();
        }


        public static void ProcessTimetableVariations()
        {
            foreach(TrainVariations variations in totalvariations)
            {
                eTrain train = new eTrain();
                train.id = variations.traincode;

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
}
