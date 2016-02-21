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
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace RailMLNeural.Neural.Configurations
{
    [Serializable]
    public class FeedForwardConfiguration : BaseNeuralConfiguration, INeuralConfiguration
    {
        private NeuralSettings _settings = new NeuralSettings();
        [Category("Neural Network Settings")]
        [ExpandableObject]
        public NeuralSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        [Category("Neural Network")]
        [ExpandableObject]
        public BasicNetwork Network { get; set; }
        [ExpandableObject]
        public NormBuffMLDataSet Data { get; set; }
        public IMLTrain Training { get; set; }
        [Category("General")]
        public string Name { get; set; }
        [Category("General")]
        public string Description { get; set; }
        [Category("General"), ReadOnly(true)]
        public AlgorithmEnum Type { get { return AlgorithmEnum.FeedForward; } }
        [Category("Neural Network")]
        public List<LayerSize> HiddenLayerSize { get; set; }
        public List<IDataProvider> InputDataProviders { get; set; }
        public List<IDataProvider> OutputDataProviders { get; set; }

        public FeedForwardConfiguration() : base()
        {
            HiddenLayerSize = new List<LayerSize>();
            InputDataProviders = new List<IDataProvider>();
            OutputDataProviders = new List<IDataProvider>();
        }

        public void Reset()
        {
            if (!IsRunning)
            {
                Network.Reset();
            }
            else
            {
                MessageBox.Show("Error: Network still running.");
            }
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

        public override void Dispose()
        {
            base.Dispose();
            Data.Close();
            Data = null;
            Network = null;
            HiddenLayerSize = null;
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
        }

        protected FeedForwardConfiguration(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
        }
        #endregion Serialization

    }

    [Serializable]
    [ProtoContract]
    public class NeuralSettings
    {
        [ProtoMember(3)]
        public int Epochs { get; set; }
        [ProtoMember(4)]
        public double VerificationSize { get; set; }
    }

    

}
