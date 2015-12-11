using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Factory;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using ProtoBuf;
using RailMLNeural.Neural;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.UI.Neural.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace RailMLNeural.Data
{
    [Serializable]
    [ProtoContract]
    public class NeuralNetwork
    {
        [ProtoMember(1)]
        
        private NeuralSettings _settings = new NeuralSettings { LearningRate = 0.1, Momentum = 0 };
        [Category("Neural Network Settings")]
        [ExpandableObject]
        public NeuralSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        [ProtoIgnore]
        [Category("Neural Network")]
        [ExpandableObject]
        public IContainsFlat Network { get; set; }
        [ProtoIgnore]
        public IMLDataSet Data { get; set; }
        [NonSerialized]
        [ProtoIgnore]
        public IMLTrain Training;
        [ProtoMember(2)]
        [Category("General")]
        public string Name { get; set; }
        [Category("General")]
        public string Description { get; set; }
        [Category("General")]
        public AlgorithmEnum Type { get; set; }
        [Category("Neural Network")]
        public List<LayerSize> HiddenLayerSize { get; set; }
        public bool IsRunning { get; set; }
        public List<double> ErrorHistory { get; set; }
        public event EventHandler ProgressChanged;

        public NeuralNetwork()
        {
            HiddenLayerSize = new List<LayerSize>();
            ErrorHistory = new List<Double>();
        }

        public void RunNetwork(object state)
        {
            IsRunning = true;
            for(int i = 0; i < Settings.Epochs; i++)
            {
                Training.Iteration();
                ErrorHistory.Add(Training.Error);
                OnProgressChanged();
            }
        }

        private void OnProgressChanged()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                EventHandler handler = ProgressChanged;
                if (handler != null)
                {
                    handler(ErrorHistory, new PropertyChangedEventArgs("ErrorHistory"));
                }
            }));
        }

    }

    [Serializable]
    [ProtoContract]
    public class NeuralSettings
    {
        [ProtoMember(1)]
        public double LearningRate { get; set; }
        [ProtoMember(2)]
        public double Momentum { get; set; }
        [ProtoMember(3)]
        public int Epochs { get; set; }


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
