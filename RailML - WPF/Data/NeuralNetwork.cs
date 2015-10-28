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
    [Serializable]
    public class NeuralNetwork
    {
        private Settings _settings = new Settings { LearningRate = 0.1, Momentum = 0 };
        public Settings Settings
        {
            get{ return _settings;}
            set{ _settings = value;}
        }
        public BasicNetwork Network { get; set; }
        public INeuralDataSet Data { get; set; }
        [NonSerialized]
        public ResilientPropagation Training;
        public DelayCombinationCollection DelayCombinations { get; set; }
        public string timetablefile { get; set; }
        public string reportsfile { get; set; }

    



    }

    [Serializable]
    public class Settings
    {
        public double LearningRate { get; set; }
        public double Momentum { get; set; }


    }

    [Serializable]
    public class DelayCombinationCollection
    {
        public List<DelayCombination> list { get; set; }
        
        public DelayCombinationCollection()
        {
            list = new List<DelayCombination>();
        }
    }

}
