using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog;
using System.IO;
using CsvFiles;
using RailML___WPF.Data;
using RailML___WPF.NeuralNetwork.PreProcessing;
using Encog.Neural.NeuralData;

namespace RailML___WPF.NeuralNetwork.Algorithms
{
    public class PerLine
    {
        private CsvFileReader<Record> reportcsv;
        private CsvFileReader<TimetableEntry> timetablecsv;
        private CsvDefinition def = new CsvDefinition() { FieldSeparator = ',' };
        private bool endoffile;
        PreProcesser pproc = new PreProcesser();


        PerLine()
        {

        }

        public void Run()
        {
            while (true)
            {
                endoffile = false;
                reportcsv = new CsvFileReader<Record>(Data.NeuralNetwork.reportsfile, def);
                timetablecsv = new CsvFileReader<TimetableEntry>(Data.NeuralNetwork.timetablefile, def);
                while (!endoffile)
                {
                    INeuralDataSet data = pproc.CreateDayDataSet(GetDayData());

                }
            }
        }

        private DayData GetDayData()
        {
            DayData data = new DayData();
            DateTime date = reportcsv.Current.trainDate;
            while(true)
            {
                Record record = reportcsv.Current;
                if(record.trainDate == date)
                {
                    data.records.Add(record);
                }
                else { break; }
                if (!reportcsv.MoveNext()) { endoffile = true; break; }
            }
            while (true)
            {
                TimetableEntry entry = timetablecsv.Current;
                if (entry.TrainDate == date)
                {
                    data.timetable.Add(entry);
                }
                else { break; }
                if (!timetablecsv.MoveNext()) { endoffile = true; break; }
            }
            return data;
        }
        
    }

    class DayData
    {
        public List<Record> records { get; set; }
        public List<TimetableEntry> timetable { get; set; }

        public DayData()
        {
            records = new List<Record>();
            timetable = new List<TimetableEntry>();
        }
    }
}
