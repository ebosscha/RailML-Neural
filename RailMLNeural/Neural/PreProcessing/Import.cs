using CsvFiles;
using ProtoBuf;
using RailMLNeural.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RailMLNeural.Neural.PreProcessing
{
    static class Import
    {

        public static void ImportDelayCombinations(object sender, DoWorkEventArgs e)
        {
            string filename = e.Argument as string;
            BackgroundWorker worker = sender as BackgroundWorker;
            DataContainer.DelayCombinations = new RailMLNeural.Data.DelayCombinationCollection();

            CsvDefinition def = new CsvDefinition() { FieldSeparator = ',' };
            CsvFileReader<Record> reader = new CsvFileReader<Record>(filename, def);
            DateTime date = new DateTime();
            DelayDay day = new DelayDay();
            int count = 0;
            DateTime ThresholdDateLower = new DateTime(2010, 1, 1);
            DateTime ThresholdDateUpper = DateTime.Now;
            if(DataContainer.Settings.UseDateFilter)
            {
                ThresholdDateLower = DataContainer.Settings.DataStartDate;
                ThresholdDateUpper = DataContainer.Settings.DataEndDate;
            }
            foreach (Record record in reader)
            {
                if (record.trainDate < ThresholdDateLower || record.trainDate > ThresholdDateUpper) { count++; continue; }
                if (record.trainDate != date)
                {
                    if (DataContainer.DelayCombinations.dict.ContainsKey(date))
                    {
                        DataContainer.DelayCombinations.dict[date].AddRange(day.FormCombinations());
                    }
                    else
                    {
                        DataContainer.DelayCombinations.dict.Add(date, day.FormCombinations());
                    }
                    date = record.trainDate;
                    day = new DelayDay();
                    worker.ReportProgress(0, new string[] { record.trainDate.ToString(), count.ToString() });
                }
                //if (record.delayCode != string.Empty)// || record.difference > 120)
                //{

                //Correct for midnight overruns
                if(record.difference < -50000)
                { 
                    record.difference += 86400; 
                }
                if(record.difference > 50000)
                {
                    record.difference -= 86400;
                }
                if (record.locationType == "D") 
                { 
                    if (day.delays.ContainsKey(record.trainCode)) 
                    {
                        if (record.actualTime < ((Delay)day.delays[record.trainCode]).ActualDeparture)
                        {
                            day.delays.Remove(record.trainCode);
                        }
                        else
                        {
                            ((Delay)day.delays[record.trainCode]).destinationdelay = record.difference;
                            ((Delay)day.delays[record.trainCode]).delaycode = record.delayCode;
                            ((Delay)day.delays[record.trainCode]).destination = record.locationSemaName;
                            ((Delay)day.delays[record.trainCode]).ActualArrival = record.actualTime;
                            ((Delay)day.delays[record.trainCode]).ScheduledArrival = record.scheduledtime;
                        }
                    }
                }
                else if(record.locationType == "O") 
                {
                    Delay delay = new Delay();
                    delay.traincode = record.trainCode;
                    delay.WLCheader = record.WLCTrainCode;
                    delay.date = record.trainDate;
                    delay.delaycode = record.delayCode;
                    delay.origindelay = record.difference;
                    delay.origin = record.locationSemaName;
                    delay.ActualDeparture = record.actualTime;
                    delay.ScheduledDeparture = record.scheduledtime;
                    day.delays.Add(delay.traincode, delay);
                }

                

                //}
                count++;

            }
            reader.Dispose();
        }
        public static void ImportHeaderHistory(object sender, DoWorkEventArgs e)
        {
            string filename = e.Argument as string;
            BackgroundWorker worker = sender as BackgroundWorker;
            Dictionary<string, Dictionary<DateTime, string>> HeaderRoutes = new Dictionary<string, Dictionary<DateTime, string>>();

            CsvDefinition def = new CsvDefinition() { FieldSeparator = ',' };
            CsvFileReader<HeaderHistory> reader = new CsvFileReader<HeaderHistory>(filename, def);
            string traincode = "";
            int count = 0;
            foreach (HeaderHistory hh in reader)
            {
                if (hh.trainDate < new DateTime(2010, 1, 1)) { count++; continue; }
                if (hh.trainCode != traincode)
                {
                    worker.ReportProgress(0, "Processing... HeaderCode : " + hh.trainCode + "     Lines Processed : " + count.ToString());
                    HeaderRoutes.Add(hh.trainCode, new Dictionary<DateTime, string>());
                    traincode = hh.trainCode;
                }
                HeaderRoutes[traincode].Add(hh.trainDate, hh.trainRoute);
            }

            DataContainer.HeaderRoutes = HeaderRoutes;
            reader.Dispose();
        }
    }

    public class Record
    {
        public int reportingId { get; set; }
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

    public class HeaderHistory
    {
        public string trainCode { get; set; }
        public DateTime trainDate { get; set; }
        public string trainOrigin { get; set; }
        public string trainRoute { get; set; }










    }


    [Serializable]
    [ProtoContract]
    public class DelayCombination
    {
        [ProtoMember(1)]
        public List<Delay> primarydelays { get; set; }
        [ProtoMember(2)]
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
            foreach (Delay d in primarydelays.Where(d => d.traincode == traincode))
            { return true; }
            foreach (Delay d in secondarydelays.Where(d => d.traincode == traincode))
            { return true; }
            return false;
        }

        public void AddStopDelays(List<StopDelay> stopdelays, string traincode)
        {
            foreach (Delay d in primarydelays.Where(d => d.traincode == traincode))
            { d.stopdelays = stopdelays; return; }
            foreach (Delay d in secondarydelays.Where(d => d.traincode == traincode))
            { d.stopdelays = stopdelays; return; }
        }

    }

    [Serializable]
    [ProtoContract]
    public class Delay
    {
        [ProtoMember(1)]
        public string traincode { get; set; }
        [ProtoMember(2)]
        public DateTime date { get; set; }
        [ProtoMember(3)]
        public double destinationdelay { get; set; }
        [ProtoMember(4)]
        public double origindelay { get; set; }
        [ProtoMember(5)]
        public string delaycode { get; set; }
        [ProtoMember(6)]
        public string WLCheader { get; set; }
        [ProtoMember(7)]
        public List<Delay> secondary { get; set; }
        [ProtoMember(8)]
        public List<StopDelay> stopdelays { get; set; }
        [ProtoMember(9)]
        public string origin { get; set; }
        [ProtoMember(10)]
        public string destination { get; set; }
        [ProtoMember(11)]
        public DateTime ActualDeparture { get; set; }
        [ProtoMember(12)]
        public DateTime ActualArrival { get; set; }
        [ProtoMember(13)]
        public DateTime ScheduledDeparture { get; set; }
        [ProtoMember(14)]
        public DateTime ScheduledArrival { get; set; }

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

        /// <summary>
        /// Get the maximum change in delay over the train run.
        /// </summary>
        /// <returns></returns>
        public double GetMaxDelta()
        {
            double d = Math.Max(0, origindelay);
            List<double> delays = new List<double>();
            delays.Add(origindelay);
            foreach(StopDelay s in stopdelays)
            {
                delays.Add(s.arrivaldelay);
            }
            delays.Add(destinationdelay);
            for(int i = 1; i < delays.Count; i++)
            {
                d = Math.Max(d, delays[i] - delays[i - 1]);
            }
            return d;
        }
    }

    [Serializable]
    [ProtoContract]
    public class StopDelay // Class defining the delay at an intemediary stop of a train.
    {
        [ProtoMember(1)]
        public string location { get; set; } // OCP name of the location
        [ProtoMember(2)]
        public double arrivaldelay { get; set; } // Arrivaldelay in seconds 
        [ProtoMember(3)]
        public double departuredelay { get; set; } // Departuredelay in seconds
        [ProtoMember(4)]
        public DateTime ActualArrival { get; set; }
        [ProtoMember(5)]
        public DateTime ActualDeparture { get; set; }
    }

    class DelayDay
    {
        public DateTime date { get; set; }
        public Dictionary<string, Delay> delays { get; set; }

        public DelayDay()
        {
            delays = new Dictionary<string,Delay>();
        }

        public List<DelayCombination> FormCombinations()
        {
            List<DelayCombination> list = new List<DelayCombination>();
            while(delays.Values.Any(x => x.ActualArrival == default(DateTime) || x.ActualDeparture == default(DateTime)))
            {
                var key = delays.Where(pair => pair.Value.ActualArrival == default(DateTime) || pair.Value.ActualDeparture == default(DateTime))
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
                if(key != null)
                {
                    delays.Remove(key);
                }
            }


            foreach (Delay delay in delays.Values)
            {
                if (delay.WLCheader != null && delay.WLCheader != String.Empty)
                {
                    try
                    {
                        ((Delay)delays[delay.WLCheader]).secondary.Add(delay); // Add Delay to it's primary delay.
                    }
                    catch { }
                }
            }

            foreach (Delay delay in delays.Values)
            {
                if (delay.WLCheader == String.Empty && (delay.delaycode != string.Empty || delay.secondary.Count != 0))
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
