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
using ProtoBuf;

namespace RailML___WPF.Data
{
    [Serializable]
    [ProtoContract]
    public class NeuralNetwork
    {
        [ProtoMember(1)]
        private Settings _settings = new Settings { LearningRate = 0.1, Momentum = 0 };
        public Settings Settings
        {
            get{ return _settings;}
            set{ _settings = value;}
        }
        [ProtoIgnore]
        public BasicNetwork Network { get; set; }
        [ProtoIgnore]
        public INeuralDataSet Data { get; set; }
        [NonSerialized]
        [ProtoIgnore]    
        public ResilientPropagation Training;
        [ProtoMember(2)]
        public DelayCombinationCollection DelayCombinations { get; set; }
        [ProtoMember(3)]
        public string timetablefile { get; set; }
        [ProtoMember(4)]
        public string reportsfile { get; set; }

    



    }

    [Serializable]
    [ProtoContract]
    public class Settings
    {
        [ProtoMember(1)]
        public double LearningRate { get; set; }
        [ProtoMember(2)]
        public double Momentum { get; set; }


    }

    [Serializable]
    [ProtoContract]
    public class DelayCombinationCollection
    {
        [ProtoMember(1)]
        public Dictionary<DateTime, List<DelayCombination>> dict;
        
        public DelayCombinationCollection()
        {
            dict = new Dictionary<DateTime, List<DelayCombination>>();
        }

    }

}
