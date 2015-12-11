using Encog.Neural.Data.Basic;
using Encog.Neural.NeuralData;
using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RailMLNeural.Neural.PreProcessing
{
    class PreProcesser
    {
        private NeuralNetwork NetworkSettings { get; set; }
        private double[] inputtemplate { get; set; }
        private double[] idealtemplate { get; set; }
        private BackgroundWorker worker;
        private List<string> inputmap { get; set; }

        public PreProcesser(NeuralNetwork Network)
        {
            NetworkSettings = Network;
        }

        #region PerLine
        /// <summary>
        /// Base for Preprocessing for PerLineClassification algorithm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PerLineClassification(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            //inputtemplate = new double[DataContainer.model.timetable.rosterings.Count];
            //idealtemplate = new double[DataContainer.model.timetable.rosterings.Count * 4];
            inputtemplate = new double[23];
            idealtemplate = new double[23 * 4];
            Dictionary<string, int> inputmap = new Dictionary<string, int>() 
            {
                {"Belfast - Connolly", 0},
                {"DART",1},
                {"IWT",2},
                {"DFDS",3},
                {"Timber",4},
                {"Tara Mines",5},
                {"Northern Commuter",6},
                {"Cork - Heuston",7},
                {"Heuston Commuter",8},
                {"Tralee",9 },
                {"Limerick - Heuston",10},
                {"Limerick Junction - Limerick",11},
                {"Ballybrophy - Limerick",12},
                {"Waterford - Heuston",13},
                {"Waterford - Limerick Junction",14},
                {"Rosslare - Waterford",15},
                {"Rosslare - Connolly",16},
                {"Galway - Heuston",17},
                {"Westport - Heuston",18},
                {"Sligo - Connolly",19},
                {"Maynooth Commuter",20}
            };
            inputtemplate = new double[inputmap.Count];
            idealtemplate = new double[inputmap.Count * 4];

            DateTime ThresholdDateLower = new DateTime(2010,1,1);
            DateTime ThresholdDateUpper = DateTime.Now;
            if(DataContainer.Settings.UseDateFilter)
            {
                ThresholdDateLower = DataContainer.Settings.DataStartDate;
                ThresholdDateUpper = DataContainer.Settings.DataEndDate;
            }

            List<double[]> inputlist = new List<double[]>();
            List<double[]> outputlist = new List<double[]>();
            foreach (KeyValuePair<DateTime, List<DelayCombination>> c in DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Key > ThresholdDateLower && x.Key < ThresholdDateUpper))
            {
                List<DelayCombination> day = c.Value;
                foreach (DelayCombination delaycombination in day)
                {
                    worker.ReportProgress(0, "Preprocessing Data... Date : " + delaycombination.primarydelays[0].date.ToString("dd/MM/yyyy"));

                    double[] inputline = new double[inputtemplate.Length];
                    double[] outputline = new double[idealtemplate.Length];

                    double[] secondarydelaysize = new double[inputtemplate.Length];
                    foreach (Delay d in delaycombination.primarydelays)
                    {
                        //string line = DataContainer.model.timetable.trains.Single(x => x.id == d.traincode).description;
                        if (GetLine(d) == "None") { goto skip; }
                        inputline[inputmap[GetLine(d)]] += d.destinationdelay;
                    }
                    foreach (Delay d in delaycombination.secondarydelays)
                    {
                        //string line = DataContainer.model.timetable.trains.Single(x => x.id == d.traincode).description;
                        if (GetLine(d) == "None") { goto skip; }
                        secondarydelaysize[inputmap[GetLine(d)]] += d.destinationdelay;
                    }

                    for (int i = 0; i < secondarydelaysize.Length; i++)
                    {
                        if (secondarydelaysize[i] == 0)
                        {
                            outputline[i * 4] = 1;
                        }
                        else if (secondarydelaysize[i] < 300)
                        {
                            outputline[i * 4 + 1] = 1;
                        }
                        else if (secondarydelaysize[i] < 600)
                        {
                            outputline[i * 4 + 2] = 1;
                        }
                        else
                        {
                            outputline[i * 4 + 3] = 1;
                        }
                    }

                    inputlist.Add(inputline);
                    outputlist.Add(outputline);
                skip:
                    continue;


                }
            }

            double[][] input = inputlist.ToArray();
            double[][] output = outputlist.ToArray();
            INeuralDataSet dataset = new BasicNeuralDataSet(input, output);
            NetworkSettings.Data = dataset;


        }

        public void PerLineExact(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            Dictionary<string, int> inputmap = new Dictionary<string, int>() 
            {
                {"Belfast - Connolly", 0},
                {"DART",1},
                {"IWT",2},
                {"DFDS",3},
                {"Timber",4},
                {"Tara Mines",5},
                {"Northern Commuter",6},
                {"Cork - Heuston",7},
                {"Heuston Commuter",8},
                {"Tralee",9 },
                {"Limerick - Heuston",10},
                {"Limerick Junction - Limerick",11},
                {"Ballybrophy - Limerick",12},
                {"Waterford - Heuston",13},
                {"Waterford - Limerick Junction",14},
                {"Rosslare - Waterford",15},
                {"Rosslare - Connolly",16},
                {"Galway - Heuston",17},
                {"Westport - Heuston",18},
                {"Sligo - Connolly",19},
                {"Maynooth Commuter",20}
            };
            inputtemplate = new double[inputmap.Count];
            idealtemplate = new double[inputmap.Count];

            DateTime ThresholdDateLower = new DateTime(2010, 1, 1);
            DateTime ThresholdDateUpper = DateTime.Now;
            if (DataContainer.Settings.UseDateFilter)
            {
                ThresholdDateLower = DataContainer.Settings.DataStartDate;
                ThresholdDateUpper = DataContainer.Settings.DataEndDate;
            }

            List<double[]> inputlist = new List<double[]>();
            List<double[]> outputlist = new List<double[]>();
            foreach (KeyValuePair<DateTime, List<DelayCombination>> c in DataContainer.DelayCombinations.dict.Where(x => x.Value != null 
                && x.Key > ThresholdDateLower && x.Key < ThresholdDateUpper))
            {
                List<DelayCombination> day = c.Value;
                foreach (DelayCombination delaycombination in day)
                {
                    worker.ReportProgress(0, "Preprocessing Data... Date : " + delaycombination.primarydelays[0].date.ToString("dd/MM/yyyy"));

                    double[] inputline = new double[inputtemplate.Length];
                    double[] outputline = new double[idealtemplate.Length];

                    double[] secondarydelaysize = new double[inputtemplate.Length];
                    foreach (Delay d in delaycombination.primarydelays)
                    {
                        //string line = DataContainer.model.timetable.trains.Single(x => x.id == d.traincode).description;
                        if (GetLine(d) == "None") { goto skip; }
                        inputline[inputmap[GetLine(d)]] += d.destinationdelay;
                    }
                    foreach (Delay d in delaycombination.secondarydelays)
                    {
                        //string line = DataContainer.model.timetable.trains.Single(x => x.id == d.traincode).description;
                        if (GetLine(d) == "None") { goto skip; }
                        secondarydelaysize[inputmap[GetLine(d)]] += d.destinationdelay;
                    }

                    outputline = secondarydelaysize;

                    inputlist.Add(inputline);
                    outputlist.Add(outputline);
                skip:
                    continue;


                }
            }

            double[][] input = inputlist.ToArray();
            double[][] output = outputlist.ToArray();
            INeuralDataSet dataset = new BasicNeuralDataSet(input, output);
            NetworkSettings.Data = dataset;
        }

        private string GetLine(Delay d)
        {
            string route;
            try
            {
                route = DataContainer.HeaderRoutes[d.traincode][d.date];
            }
            catch { return "None"; }
            switch (route)
            {
                case "1":
                    return "Belfast - Connolly";
                case "10":
                    return "DART";
                case "11":
                    return "IWT";
                case "12":
                    return "DFDS";
                case "13":
                    return "Timber";
                case "14":
                    return "Tara Mines";
                case "1a":
                    return "Northern Commuter";
                case "2":
                    return "Cork - Heuston";
                case "2a":
                    return "Heuston Commuter";
                case "3":
                    return "Tralee";
                case "4":
                    return "Limerick - Heuston";
                case "4a":
                    return "Limerick Junction - Limerick";
                case "4c":
                    return "Ballybrophy - Limerick";
                case "5":
                    return "Waterford - Heuston";
                case "5a":
                    return "Waterford - Limerick Junction";
                case "5b":
                    return "Rosslare - Waterford";
                case "6":
                    return "Rosslare - Connolly";
                case "7":
                    return "Galway - Heuston";
                case "8":
                    return "Westport - Heuston";
                case "9":
                    return "Sligo - Connolly";
                case "9a":
                    return "Maynooth Commuter";
                default:
                    return "None";

            }


        }
        #endregion PerLine

        #region PerRoutePart
        /// <summary>
        /// Base for algorithm learning total delay propagation per each route part.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PerRoutePartExact(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            inputmap = InputMap();
            List<double[]> inputlist = new List<double[]>();
            List<double[]> outputlist = new List<double[]>();

            DateTime ThresholdDateLower = new DateTime(2010, 1, 1);
            DateTime ThresholdDateUpper = DateTime.Now;
            if (DataContainer.Settings.UseDateFilter)
            {
                ThresholdDateLower = DataContainer.Settings.DataStartDate;
                ThresholdDateUpper = DataContainer.Settings.DataEndDate;
            }

            foreach (KeyValuePair<DateTime, List<DelayCombination>> c in DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Key > ThresholdDateLower && x.Key < ThresholdDateUpper))
            {
                worker.ReportProgress(0, "Preprocessing Data... Date : " + c.Key.ToString("dd/MM/yyyy"));
                List<DelayCombination> day = c.Value;
                foreach(DelayCombination dc in day)
                {
                    double[] inputpart = new double[inputmap.Count];
                    double[] outputpart = new double[inputmap.Count];
                    inputpart = HandleDelays(dc.primarydelays);
                    outputpart = HandleDelays(dc.secondarydelays);
                    inputlist.Add(inputpart);
                    outputlist.Add(outputpart);
                }
            }

            worker.ReportProgress(0, "Converting to Array.....");
            double[][] input = inputlist.ToArray();
            inputlist.Clear();
            double[][] output = outputlist.ToArray();
            outputlist.Clear();
            worker.ReportProgress(0, "Collectiong Garbage....");
            GC.Collect(2);
            worker.ReportProgress(0, "Converting to DataSet....");
            NetworkSettings.Data = new BasicNeuralDataSet(input, output);

            worker.ReportProgress(0, "Preprocessing Finished.");
        }

        /// <summary>
        /// Method to create a list of route parts between stations. Gathers all possible origin/destination combinations that have occurred within the dataset.
        /// </summary>
        /// <returns></returns>
        private List<string> InputMap()
        {
            worker.ReportProgress(0, "Creating Input Map.....");
            List<string> map = new List<string>();
            foreach (KeyValuePair<DateTime, List<DelayCombination>> c in DataContainer.DelayCombinations.dict.Where(x => x.Value != null))
            {
                worker.ReportProgress(0, "Creating Input Map ...... Date: " + c.Key.ToString("dd/MM/yyyy"));
                List<DelayCombination> dclist = c.Value;
                foreach (DelayCombination dc in dclist)
                {
                    foreach(Delay d in dc.primarydelays)
                    {
                        if(d.stopdelays.Count > 0)
                        {
                            if(!map.Contains(d.origin + "to" + d.stopdelays[0].location))
                            {
                                map.Add(d.origin + "to" + d.stopdelays[0].location);
                            }
                            for(int i = 0; i < d.stopdelays.Count-1; i++)
                            {
                                if(!map.Contains(d.stopdelays[i].location + "to" + d.stopdelays[i+1].location))
                                {
                                    map.Add(d.stopdelays[i].location + "to" + d.stopdelays[i + 1].location);
                                }
                            }
                            if (!map.Contains(d.stopdelays[d.stopdelays.Count-1].location + "to" + d.destination))
                            {
                                map.Add(d.stopdelays[d.stopdelays.Count - 1].location + "to" + d.destination);
                            }

                        }
                        else
                        {
                            if (!map.Contains(d.origin + "to" + d.destination))
                            {
                                map.Add(d.origin + "to" + d.destination);
                            }
                        }

                    }
                }
            }
            return map;

        }

        private double[] HandleDelays(List<Delay> delays)
        {
            double[] output = new double[inputmap.Count];
            foreach(Delay d in delays.Where(x => x.origin != null && x.destination != null))
            {
                if (d.stopdelays.Count > 0)
                {
                    output[inputmap.IndexOf(d.origin + "to" + d.stopdelays[0].location)] += d.stopdelays[0].arrivaldelay;
                    
                    for (int i = 0; i < d.stopdelays.Count - 1; i++)
                    {
                        output[inputmap.IndexOf(d.stopdelays[i].location + "to" + d.stopdelays[i + 1].location)] += d.stopdelays[i + 1].arrivaldelay;
                        
                    }
                    
                    output[inputmap.IndexOf(d.stopdelays[d.stopdelays.Count - 1].location + "to" + d.destination)] += d.destinationdelay;
                }
                else
                {
                    
                    output[inputmap.IndexOf(d.origin + "to" + d.destination)] += d.destinationdelay;
                }

            }
            return output;
        }

        #endregion PerRoutePart

    }
}
