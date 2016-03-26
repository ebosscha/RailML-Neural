using Encog.Neural.Networks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Neural;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Algorithms;
using Encog.Neural.Networks.Layers;
using System.Threading;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CreateRecurrentNetworkViewModel : ViewModelBase
    {
         #region Parameters
        /// <summary>
        /// Defining all parameters in the ViewModel
        /// </summary>
        private GRNNConfiguration _configuration;
        public GRNNConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        private bool _isHitTest = true;
        public bool IsHitTest
        {
            get { return _isHitTest; }
            set { _isHitTest = value;
            RaisePropertyChanged("IsHitTest");
            }
        }
        /// <summary>
        /// Creating list for activation functions within encog
        /// </summary>
        public IEnumerable<ActivationFunctionEnum> ActivationFunctionEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(ActivationFunctionEnum)).Cast<ActivationFunctionEnum>();
            }
        }

        public IEnumerable<NormalizationTypeEnum> NormalizationTypeEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(NormalizationTypeEnum)).Cast<NormalizationTypeEnum>();
            }
        }

        public IEnumerable<RecurrentDataProviderEnum> DataProviderEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(RecurrentDataProviderEnum)).Cast<RecurrentDataProviderEnum>();
            }
        }

        public IEnumerable<PropagatorEnum> PropagatorEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(PropagatorEnum)).Cast<PropagatorEnum>();
            }
        }

        public RecurrentDataProviderEnum DataProvider { get; set; }

        public ObservableCollection<LayerSize> EdgeHiddenLayerSize {get; set;}

        public ObservableCollection<LayerSize> VertexHiddenLayerSize { get; set; }

        public PropagatorEnum PropagatorType { get; set; }

        private string _statustext;

        public string StatusText 
        {
            get { return _statustext; }
            set { _statustext = value;
            RaisePropertyChanged("StatusText");
            }
        }

        private LayerSize _outputLayerSize = new LayerSize();

        public LayerSize OutputLayerSize
        {
            get { return _outputLayerSize; }
            set
            {
                _outputLayerSize = value;
                RaisePropertyChanged("OutputLayerSize");
            }
        }

        private ObservableCollection<IRecurrentDataProvider> _inputDataProviders;

        public ObservableCollection<IRecurrentDataProvider> InputDataProviders
        {
            get { return _inputDataProviders; }
            set { _inputDataProviders = value;
            RaisePropertyChanged("InputDataProviders");
            }
        }

        private ObservableCollection<IRecurrentDataProvider> _outputDataProviders;

        public ObservableCollection<IRecurrentDataProvider> OutputDataProviders
        {
            get { return _outputDataProviders; }
            set
            {
                _outputDataProviders = value;
                RaisePropertyChanged("OutputDataProviders");
            }
        }

        public int VertexNetworkFeedIndex { get; set; }

        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the CreateFeedForwardViewModel class.
        /// </summary>
        public CreateRecurrentNetworkViewModel()
        {
            Configuration = new GRNNConfiguration();
            EdgeHiddenLayerSize = new ObservableCollection<LayerSize>();
            VertexHiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            OutputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            InitializeCommands();
        }

        /// <summary>
        /// Handles the addition of a new layer to the observablecollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EdgeHiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue == null) { return; }
            if((int)e.OldValue > (int)e.NewValue)
            {
                for(int i = (int)e.OldValue; i > (int)e.NewValue && i > 0; i--)
                {
                    EdgeHiddenLayerSize.RemoveAt(i-1);
                }
            }
            else
            {
                for(int i = (int)e.OldValue; i < (int)e.NewValue; i++)
                {
                    EdgeHiddenLayerSize.Add(new LayerSize());
                }
            }          
        }

        public void VertexHiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue == null) { return; }
            if ((int)e.OldValue > (int)e.NewValue)
            {
                for (int i = (int)e.OldValue; i > (int)e.NewValue && i > 0; i--)
                {
                    VertexHiddenLayerSize.RemoveAt(i - 1);
                }
            }
            else
            {
                for (int i = (int)e.OldValue; i < (int)e.NewValue; i++)
                {
                    VertexHiddenLayerSize.Add(new LayerSize());
                }
            }
        }
        #endregion Public

        #region Privates

        private void Init(object state)
        {
            IsHitTest = false;
            Configuration.InputDataProviders = InputDataProviders.ToList();
            Configuration.OutputDataProviders = OutputDataProviders.ToList();
            StatusText = "Creating Graph...";
            Configuration.Graph = new SimplifiedGraph();
            while (Configuration.Graph.RunningThreads > 0)
            {
                Thread.Sleep(1);
            }
            StatusText = "Creating Network...";
            Configuration.DataSet = new DelayCombinationSet(DataContainer.DelayCombinations.dict.Keys, Configuration.PerDay, Configuration.ExcludeEmpty);
            Configuration.DataSet.Split(Configuration.Settings.VerificationSize);
            Configuration.Propagator = PropagatorFactory.Create(Configuration, PropagatorType, true);
            CreateNetwork();
            Messenger.Default.Send<AddNeuralNetworkMessage>(new AddNeuralNetworkMessage() { NeuralNetwork = Configuration });
            IsHitTest = true;
            StatusText = null;
        }

        private void CreateNetwork()
        {


            FlatGRNN network = new FlatGRNN();
            BasicNetwork EdgeNetwork = new BasicNetwork();
            List<BasicLayer> edgelayers = new List<BasicLayer>();
            edgelayers.Add(new BasicLayer(null, true, InputDataProviders.Sum(x => x.Size) + (VertexHiddenLayerSize.Last().Size * 2)));
            for(int i = 0; i < EdgeHiddenLayerSize.Count; i++)
            {
                BasicLayer hiddenlayer = EdgeHiddenLayerSize[i].CreateLayer();
                if(EdgeHiddenLayerSize[i].IsRecurrent)
                {
                    edgelayers[i].ContextFedBy = hiddenlayer;
                }
                edgelayers.Add(hiddenlayer);
            }
            edgelayers.Add(OutputLayerSize.CreateLayer(OutputDataProviders.Sum(x => x.Size)));
            foreach (var layer in edgelayers)
            {
                EdgeNetwork.AddLayer(layer);
            }
            EdgeNetwork.Structure.FinalizeStructure();

            BasicNetwork VertexNetwork = new BasicNetwork();
            List<BasicLayer> vertexlayers = new List<BasicLayer>();
            vertexlayers.Add(new BasicLayer(null, true, EdgeHiddenLayerSize[VertexNetworkFeedIndex-1].Size));
            for (int i = 0; i < VertexHiddenLayerSize.Count; i++)
            {
                BasicLayer hiddenlayer = VertexHiddenLayerSize[i].CreateLayer();
                if (VertexHiddenLayerSize[i].IsRecurrent)
                {
                    vertexlayers[i].ContextFedBy = hiddenlayer;
                }
                vertexlayers.Add(hiddenlayer);
            }
            foreach(var layer in vertexlayers)
            {
                VertexNetwork.AddLayer(layer);
            }
            VertexNetwork.Structure.FinalizeStructure();

            network.Init(Configuration.Graph, EdgeNetwork, VertexNetwork, VertexNetworkFeedIndex);
            network.ResetWeights();
            Configuration.Network = network;
            
            
        }

        private void Reset()
        {
            Configuration = new GRNNConfiguration();
            EdgeHiddenLayerSize = new ObservableCollection<LayerSize>();
            VertexHiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            OutputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            OutputLayerSize = new LayerSize();
        }

        #endregion Privates


        #region Commands
        /// <summary>
        /// All Commands controlling the buttons in the Create Neural Configuration Window.
        /// </summary>
        public ICommand CreateNeuralCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddDataProviderCommand { get; set; }

        private void InitializeCommands()
        {
            CreateNeuralCommand = new RelayCommand(ExecuteCreateNeural, canExecuteCreateNeural);
            CancelCommand = new RelayCommand<object>((param) => ExecuteCancel(param));
            AddDataProviderCommand = new RelayCommand(ExecuteAddDataProvider);
        }

        /// <summary>
        /// Create network and send message to NeuralCollectionViewModel to add it to the list.
        /// </summary>
        private void ExecuteCreateNeural()
        {
            ThreadPool.QueueUserWorkItem(Init);         
        }

        /// <summary>
        /// Check if all conditions are fulfilled before being able to create a new network configuration
        /// </summary>
        /// <returns></returns>
        private bool canExecuteCreateNeural()
        {
            return EdgeHiddenLayerSize.Count > 0 &&
                Configuration.Name != string.Empty && Configuration.Description != string.Empty &&
                Configuration.Name != null && Configuration.Description != null &&
                EdgeHiddenLayerSize.Where(x => x.Size > 0).Count() == EdgeHiddenLayerSize.Count;    
        }


        /// <summary>
        /// Reset the current configuration and exit the window.
        /// </summary>
        /// <param name="parameter"></param>
        private void ExecuteCancel(object parameter)
        {
            if (parameter is Window)
            {
                Reset();
                (parameter as Window).Close();
            }
        }

        private void ExecuteAddDataProvider()
        {
            IRecurrentDataProvider provider = DataProviderFactory.CreateRecurrentDataProvider(DataProvider);
            if (provider != null && provider.IsInput)
            {
                InputDataProviders.Add(provider);
            }
            else if(provider != null)
            {
                OutputDataProviders.Add(provider);
            }
        }

        #endregion Commands
    }
    
}