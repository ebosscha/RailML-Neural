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
    public class FeedForwardConfiguration : INeuralConfiguration
    {
        private NeuralSettings _settings = new NeuralSettings { LearningRate = 0.1, Momentum = 0 };
        [Category("Neural Network Settings")]
        [ExpandableObject]
        public NeuralSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        [Category("Neural Network")]
        [ExpandableObject]
        public IContainsFlat Network { get; set; }
        [ExpandableObject]
        public NormBuffMLDataSet Data { get; set; }
        public IMLTrain Training { get; set; }
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
        public List<double> VerificationHistory { get; set; }
        public List<IDataProvider> InputDataProviders { get; set; }
        public List<IDataProvider> OutputDataProviders { get; set; }
        public event EventHandler ProgressChanged;

        public FeedForwardConfiguration()
        {
            HiddenLayerSize = new List<LayerSize>();
            ErrorHistory = new List<Double>();
            VerificationHistory = new List<Double>();
            InputDataProviders = new List<IDataProvider>();
            OutputDataProviders = new List<IDataProvider>();
        }

        public void TrainNetwork(object state)
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
                VerificationHistory.Add(((BasicNetwork)Network).CalculateError(data));
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
            VerificationHistory = null;
            ProgressChanged = null;
        }

        public int InputSize
        {
            get
            {
                int result = 0;
                foreach(IDataProvider d in InputDataProviders)
                {
                    result += d.Size;
                }
                return result;
            }
        }

        public int OutputSize
        {
            get
            {
                int result = 0;
                foreach(IDataProvider d in OutputDataProviders)
                {
                    result += d.Size;
                }
                return result;
            }
        }

        #region Mapping
        public List<string> InputMap
        {
            get
            {
                var result = new List<string>();
                foreach(IDataProvider provider in InputDataProviders)
                {
                    result.AddRange(provider.Map);
                }
                return result;
            }
        }

        public List<string> OutputMap
        {
            get
            {
                var result = new List<string>();
                foreach (IDataProvider provider in OutputDataProviders)
                {
                    result.AddRange(provider.Map);
                }
                return result;
            }
        }
        #endregion Mapping

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

    

}
