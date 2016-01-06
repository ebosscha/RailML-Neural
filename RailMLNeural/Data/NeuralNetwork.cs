using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Factory;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using Encog.Util.Arrayutil;
using ProtoBuf;
using RailMLNeural.Neural;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Normalization;
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
        [ExpandableObject]
        public NormBuffMLDataSet Data { get; set; }
        [ProtoIgnore]
        public IMLTrain Training;
        [ProtoMember(2)]
        [Category("General")]
        public string Name { get; set; }
        [Category("General")]
        public string Description { get; set; }
        [Category("General"), ReadOnly(true)]
        public AlgorithmEnum Type { get; set; }
        [Category("Neural Network")]
        public List<LayerSize> HiddenLayerSize { get; set; }
        public bool IsRunning { get; set; }
        public string filefolder { get; set; }
        public List<double> ErrorHistory { get; set; }
        public List<double> VerificationSetHistory { get; set; }
        public List<string> InputMap { get; set; }
        public List<string> OutputMap { get; set; }
        //public NormalizationHelper Normalizer { get; set; }
        public event EventHandler ProgressChanged;

        public NeuralNetwork()
        {
            HiddenLayerSize = new List<LayerSize>();
            ErrorHistory = new List<Double>();
            VerificationSetHistory = new List<Double>();
            InputMap = new List<string>();
        }

        public void RunNetwork(object state)
        {
            Data.Open();
            IsRunning = true;
            if(ErrorHistory.Count == 0)
            {
                if (Network is IContainsFlat)
                {
                    ((IContainsFlat)Network).Flat.Randomize();
                }
            }
            for(int i = 0; i < Settings.Epochs; i++)
            {
                Training.Iteration();
                ErrorHistory.Add(Training.Error);
                RunVerificationSet();
                OnProgressChanged();
            }
            IsRunning = false;
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

        
        private void RunVerificationSet()
        {
            if(Network is BasicNetwork)
            {
                var data = Data.VerificationDataSet();
                VerificationSetHistory.Add(((BasicNetwork)Network).CalculateError(data));
            }
        }

        public IMLData Compute(IMLData data)
        {
            if(Network is BasicNetwork)
            {
                return ((BasicNetwork)Network).Compute(data);
            }
            return null;
        }

        public void Dispose()
        {
            Data.Close();
            Data = null;
            Training = null;
            Network = null;
            HiddenLayerSize = null;
            ErrorHistory = null;
            VerificationSetHistory = null;
            ProgressChanged = null;
        }

        #region Serialization
        public void Serialize()
        {

        }

        public void Deserialize()
        {

        }

        #endregion Serialization

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
        [ProtoMember(4)]
        public double VerificationSize { get; set; }


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
