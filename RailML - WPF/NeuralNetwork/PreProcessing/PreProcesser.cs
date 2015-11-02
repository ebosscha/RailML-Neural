using Encog.Neural.Data.Basic;
using Encog.Neural.NeuralData;
using RailML___WPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.NeuralNetwork.PreProcessing
{
    class PreProcesser
    {
        private double[] inputtemplate {get; set;}
        private double[] idealtemplate {get; set;}
        private BackgroundWorker worker;

        public PreProcesser()
        {

        }
        
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
            
            List<double[]> inputlist = new List<double[]>();
            List<double[]> outputlist = new List<double[]>();
            foreach(List<DelayCombination> day in DataContainer.NeuralNetwork.DelayCombinations.dict.Values.Where(x => x != null))
            {
                
                foreach(DelayCombination delaycombination in day)
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
            DataContainer.NeuralNetwork.Data = dataset;


        }

        private string GetLine(Delay d)
        {
            string route;
            try
            {
                route = DataContainer.NeuralNetwork.HeaderRoutes[d.traincode][d.date];
            }
            catch { return "None";}
            switch(route)
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
    }

    
}
