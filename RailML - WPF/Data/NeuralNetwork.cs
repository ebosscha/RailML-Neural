using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog;
using Encog.Neural;
using Encog.Neural.Networks;
using Encog.Neural.NeuralData;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using RailML___WPF.NeuralNetwork.PreProcessing;

namespace RailML___WPF.Data
{
    public static class NeuralNetwork
    {
        private static Settings _settings = new Settings { LearningRate = 0.1, Momentum = 0 };
        public static Settings Settings
        {
            get{ return _settings;}
            set{ _settings = value;}
        }
        public static BasicNetwork Network { get; set; }
        public static INeuralDataSet Data { get; set; }
        public static ResilientPropagation Training { get; set; }
        public static DelayCombinationCollection DelayCombinations { get; set; }
        public static string timetablefile { get; set; }
        public static string reportsfile { get; set; }

    



    }

    public class Settings
    {
        public double LearningRate { get; set; }
        public double Momentum { get; set; }


    }

    public class DelayCombinationCollection
    {
        public List<DelayCombination> list { get; set; }
        
        public DelayCombinationCollection()
        {
            list = new List<DelayCombination>();
        }
    }

}
