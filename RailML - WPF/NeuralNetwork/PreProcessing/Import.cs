using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using LumenWorks.Framework;
using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;
using CsvFiles;
using RailML___WPF.Data;

namespace RailML___WPF.NeuralNetwork.PreProcessing
{
    static class Import
    {

        public static void ImportDelayCombinations(object sender, DoWorkEventArgs e)
        {
            string filename = e.Argument as string;
            BackgroundWorker worker = sender as BackgroundWorker;
            DataContainer.NeuralNetwork.DelayCombinations = new Data.DelayCombinationCollection();

            CsvDefinition def = new CsvDefinition() { FieldSeparator = ',' };
            CsvFileReader<Record> reader = new CsvFileReader<Record>(filename, def);
            DateTime date = new DateTime();
            DelayDay day = new DelayDay();
            int count = 0;
            foreach (Record record in reader)
            {
                if (record.trainDate != date)
                {
                    DataContainer.NeuralNetwork.DelayCombinations.list.AddRange(day.FormCombinations());
                    date = record.trainDate;
                    day = new DelayDay();
                    worker.ReportProgress(0, new string[] { record.trainDate.ToString(), count.ToString() });
                }
                if (record.delayCode != string.Empty || record.difference > 120)
                {
                    Delay delay = new Delay();
                    delay.traincode = record.trainCode;
                    delay.WLCheader = record.WLCTrainCode;
                    delay.date = record.trainDate;
                    delay.delaycode = record.delayCode;
                    if (record.locationType == "D") { delay.destinationdelay = record.difference; }
                    else { delay.origindelay = record.difference; }

                    if (day.delays.ContainsKey(delay.traincode)) { ((Delay)day.delays[delay.traincode]).destinationdelay = delay.destinationdelay; }
                    else { day.delays.Add(delay.traincode, delay); }

                }
                count++;

            }
        }

    }

    public class Record
    {
        public int reportingId {get;set;}
        public DateTime trainDate { get; set; }
        public string trainCode { get; set; }
        public string locationSemaName { get; set; }
        public string locationType { get; set; }
        public DateTime scheduledtime { get; set; }
        public DateTime actualTime { get; set; }
        public int threshold { get; set; }
        public int difference { get; set; }
        public string delayCode { get; set; }
        public string WLCTrainCode { get; set; }
        public string comments { get; set; }
    }

    [Serializable]
    public class DelayCombination
    {
        public List<Delay> primarydelays { get; set; }
        public List<Delay> secondarydelays { get; set; }

        public DelayCombination()
        {
            primarydelays = new List<Delay>();
            secondarydelays = new List<Delay>();
        }

        public DateTime GetDate()
        {
            return primarydelays[0].date;
        }

        public bool HasTrain(string traincode)
        {
            foreach(Delay d in primarydelays.Where(d => d.traincode == traincode))
            {return true;}
            foreach (Delay d in secondarydelays.Where(d => d.traincode == traincode))
            {return true;}
            return false;
        }

        public void AddStopDelays(List<StopDelay> stopdelays, string traincode)
        {
            foreach(Delay d in primarydelays.Where(d => d.traincode == traincode))
            { d.stopdelays = stopdelays; return; }
            foreach (Delay d in secondarydelays.Where(d => d.traincode == traincode))
            { d.stopdelays = stopdelays; return; }
        }

    }

    [Serializable]
    public class Delay
    {
        public string traincode { get; set; }
        public DateTime date { get; set; }
        public double destinationdelay { get; set; }
        public double origindelay { get; set; }
        public string delaycode { get; set; }
        public string WLCheader { get; set; }
        public List<Delay> secondary { get; set; }
        public List<StopDelay> stopdelays { get; set; }

        public Delay()
        {
            secondary = new List<Delay>();
            stopdelays = new List<StopDelay>();
        }

        public List<Delay> GetSecondaries()
        {
            List<Delay> result = new List<Delay>();
            foreach (Delay d in secondary)
            {
                result.Add(d);
                result.AddRange(d.GetSecondaries());
            }
            return result;
        }       
    }

    [Serializable]
    public class StopDelay // Class defining the delay at an intemediary stop of a train.
    {
        public string location { get; set; } // OCP name of the location
        public double arrivaldelay { get; set; } // Arrivaldelay in seconds 
        public double departuredelay { get; set; } // Departuredelay in seconds
    }

    class DelayDay
    {
        public DateTime date { get; set; }
        public Hashtable delays { get; set; }

        public DelayDay()
        {
            delays = new Hashtable();
        }

        public List<DelayCombination> FormCombinations()
        {
            List<DelayCombination> list = new List<DelayCombination>();
            foreach(Delay delay in delays.Values)
            {
                if(delay.WLCheader != null && delay.WLCheader != String.Empty)
                {
                    try
                    {
                        ((Delay)delays[delay.WLCheader]).secondary.Add(delay); // Add Delay to it's primary delay.
                    }
                    catch { }
                }
            }

            foreach(Delay delay in delays.Values)
            {
                if(delay.WLCheader == String.Empty)
                {
                    DelayCombination combination = new DelayCombination();
                    combination.primarydelays.Add(delay);
                    combination.secondarydelays.AddRange(delay.GetSecondaries());
                    list.Add(combination);

                }
            }

            return list;

        }

    }

    public enum DelayType
    {
        Origin,
        Destination
    }

    public class TimetableEntry
    {
        public string TrainCode { get; set; }
        public DateTime TrainDate { get; set; }
        public string LocationCode { get; set; }
        public string LocationFullName { get; set; }
        public int LocationOrder { get; set; }
        public string LocationType { get; set; }
        public string TrainOrigin { get; set; }
        public string TrainDestination { get; set; }
        public DateTime ScheduledArrival { get; set; }
        public DateTime ScheduledDeparture { get; set; }
        public DateTime ExpectedArrival { get; set; }
        public DateTime ExpectedDeparture { get; set; }
        public DateTime Arrival { get; set; }
        public DateTime Departure { get; set; }
    }
}
