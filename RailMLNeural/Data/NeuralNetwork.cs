﻿using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using ProtoBuf;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;

namespace RailMLNeural.Data
{
    [Serializable]
    [ProtoContract]
    public class NeuralNetwork
    {
        [ProtoMember(1)]
        private NeuralSettings _settings = new NeuralSettings { LearningRate = 0.1, Momentum = 0 };
        public NeuralSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        [ProtoIgnore]
        public BasicNetwork Network { get; set; }
        [ProtoIgnore]
        public INeuralDataSet Data { get; set; }
        [NonSerialized]
        [ProtoIgnore]
        public ResilientPropagation Training;
        [ProtoMember(2)]
        
        public string timetablefile { get; set; }
        [ProtoMember(5)]
        public string reportsfile { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }




    }

    [Serializable]
    [ProtoContract]
    public class NeuralSettings
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
