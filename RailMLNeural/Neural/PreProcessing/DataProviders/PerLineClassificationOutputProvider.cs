using Encog.ML.Data;
using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.PreProcessing.DataProviders
{
    class PerLineClassificationOutputProvider : IDataProvider
    {
        /// <summary>
        /// Integer representing the node count of this module
        /// </summary>
        private int _size;
        public int Size { get { return _size; } }

        /// <summary>
        /// Representing the labels for the nodes
        /// </summary>
        private List<string> _map = new List<string>();
        public List<string> Map
        { get { return _map; } }

        /// <summary>
        /// Represeting the index of the input/output IMLData where this specific data starts
        /// </summary>
        public int LowerIndex { get; set; }

        private int _classes;

        public PerLineClassificationOutputProvider()
        {
            _map = inputmap.Keys.ToList();
            _classes = 5;
            _size = _map.Count * _classes;
        }

        public double[] Process(DelayCombination dc)
        {
            double[] result = new double[Size];
            double[] delaysize = new double[Map.Count];
            foreach (Delay d in dc.primarydelays)
            {
                if (GetLine(d) == "None") { return null; }
                delaysize[inputmap[GetLine(d)]] += d.destinationdelay;
            }

            foreach (Delay d in dc.secondarydelays)
            {
                if (GetLine(d) == "None") { return null; }
                delaysize[inputmap[GetLine(d)]] += d.destinationdelay;
            }

            for (int i = 0; i < delaysize.Length; i++)
            {
                if (delaysize[i] < 60)
                {
                    result[i * 5] = 1;
                }
                else if (delaysize[i] < 300)
                {
                    result[i * 4 + 1] = 1;
                }
                else if (delaysize[i] < 600)
                {
                    result[i * 4 + 2] = 1;
                }
                else if (delaysize[i] < 1800)
                {
                    result[i * 4 + 3] = 1;
                }
                else
                {
                    result[i * 4 + 4] = 1;
                }
            }
            return result;
        }

        public List<Tuple<string, dynamic>> PublishOutput(IMLData Data)
        {
            List<Tuple<string, dynamic>> result = new List<Tuple<string, dynamic>>();
            int n = 0;
            for (int i = LowerIndex; i < LowerIndex + Size; i += 5 )
            {
                double[] array = {Data[i], Data[i+1], Data[i+2], Data[i+3], Data[i+4] };
                int index = array.ToList().IndexOf(array.Max());
                string Class = Classes[index];

                result.Add(new Tuple<string, dynamic>(Map[n], Class));
                n++;
            }

                return result;
        }

        private string[] Classes = 
        {
            "< 1 Minute",
            "1 - 5 Minutes",
            "6 - 10 Minutes",
            "11 - 30 Minutes",
            "> 30 Minutes"
        };

        /// <summary>
        /// Dictionary containing all defined lines and corresponding index;
        /// </summary>
        private Dictionary<string, int> inputmap = new Dictionary<string, int>() 
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
    }
}
