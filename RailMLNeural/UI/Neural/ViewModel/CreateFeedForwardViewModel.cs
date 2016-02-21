using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using RailMLNeural.Neural;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using RailMLNeural.Neural.PreProcessing;
using System.ComponentModel;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Engine.Network.Activation;
using RailMLNeural.Neural.Normalization;
using RailMLNeural.Neural.PreProcessing.DataProviders;
using RailMLNeural.Neural.Configurations;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CreateFeedForwardViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// Defining all parameters in the ViewModel
        /// </summary>
        private FeedForwardConfiguration _network;
        public FeedForwardConfiguration Network
        {
            get { return _network; }
            set { _network = value; }
        }

        /// <summary>
        /// Creating list for algorithm enum values to feed to combobox
        /// </summary>
        public IEnumerable<AlgorithmEnum> AlgorithmEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(AlgorithmEnum)).Cast<AlgorithmEnum>();       
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

        public IEnumerable<InputDataProviderEnum> InputDataProviderEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(InputDataProviderEnum)).Cast<InputDataProviderEnum>();
            }
        }

        public IEnumerable<OutputDataProviderEnum> OutputDataProviderEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(OutputDataProviderEnum)).Cast<OutputDataProviderEnum>();
            }
        }

        public IEnumerable<DataProviderEnum> DataProviderEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(DataProviderEnum)).Cast<DataProviderEnum>();
            }
        }

        public DataProviderEnum DataProvider { get; set; }

        public ObservableCollection<LayerSize> HiddenLayerSize
        {
            get;
            set;
        }

        private string _statustext;

        public string StatusText 
        {
            get { return _statustext; }
            set { _statustext = value;
            RaisePropertyChanged("StatusText");
            }
        }

        private PreProcesser pproc;

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

        private ObservableCollection<IDataProvider> _inputDataProviders;

        public ObservableCollection<IDataProvider> InputDataProviders
        {
            get { return _inputDataProviders; }
            set { _inputDataProviders = value;
            RaisePropertyChanged("InputDataProviders");
            }
        }

        private ObservableCollection<IDataProvider> _outputDataProviders;

        public ObservableCollection<IDataProvider> OutputDataProviders
        {
            get { return _outputDataProviders; }
            set
            {
                _outputDataProviders = value;
                RaisePropertyChanged("OutputDataProviders");
            }
        }

        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the CreateFeedForwardViewModel class.
        /// </summary>
        public CreateFeedForwardViewModel()
        {
            Network = new FeedForwardConfiguration();
            HiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IDataProvider>();
            OutputDataProviders = new ObservableCollection<IDataProvider>();
            InitializeCommands();
            pproc = new PreProcesser(Network);
        }

        /// <summary>
        /// Handles the addition of a new layer to the observablecollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue == null) { return; }
            if((int)e.OldValue > (int)e.NewValue)
            {
                for(int i = (int)e.OldValue; i > (int)e.NewValue && i > 0; i--)
                {
                    HiddenLayerSize.RemoveAt(i-1);
                }
            }
            else
            {
                for(int i = (int)e.OldValue; i < (int)e.NewValue; i++)
                {
                    HiddenLayerSize.Add(new LayerSize());
                }
            }
            
        }

        /// <summary>
        /// Handles the change of selected neural network architecture
        /// </summary>
        public void Algorithm_SelectionChanged()
        {
            //TODO
        }
        #endregion Public

        #region Privates
        private void CreateNetwork()
        {

            BasicNetwork N = new BasicNetwork();
            N.AddLayer(new BasicLayer(null, true, Network.Data.InputSize));
            for (int i = 0; i < HiddenLayerSize.Count; i++ )
            {
                N.AddLayer(HiddenLayerSize[i].CreateLayer());
            }
            N.AddLayer(OutputLayerSize.CreateLayer(Network.Data.IdealSize));
            N.Structure.FinalizeStructure();
            Network.Network = N;
            Network.HiddenLayerSize = HiddenLayerSize.ToList();
        }

        private void Reset()
        {
            FeedForwardConfiguration NewNetwork = new FeedForwardConfiguration();
            Network = NewNetwork;
            HiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IDataProvider>();
            OutputDataProviders = new ObservableCollection<IDataProvider>();
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
            Network.InputDataProviders = InputDataProviders.ToList();
            Network.OutputDataProviders = OutputDataProviders.ToList();
            pproc = new PreProcesser(Network);
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PreProcessing_Finished);
            worker.ProgressChanged += new ProgressChangedEventHandler(PreProcessing_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(pproc.Process);
            worker.RunWorkerAsync();
            Messenger.Default.Send<IsBusyMessage>(new IsBusyMessage() { IsBusy = true });    
        }

        private void PreProcessing_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusText = e.UserState as string;
        }

        private void PreProcessing_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusText = string.Empty;
            Messenger.Default.Send<IsBusyMessage>(new IsBusyMessage() { IsBusy = false });
            CreateNetwork();
            Messenger.Default.Send<AddNeuralNetworkMessage>(new AddNeuralNetworkMessage() { NeuralNetwork = Network });
            Reset();
        }

        /// <summary>
        /// Check if all conditions are fulfilled before being able to create a new network configuration
        /// </summary>
        /// <returns></returns>
        private bool canExecuteCreateNeural()
        {
            return HiddenLayerSize.Count > 0 &&
                Network.Name != string.Empty && Network.Description != string.Empty &&
                Network.Name != null && Network.Description != null &&
                HiddenLayerSize.Where(x => x.Size > 0).Count() == HiddenLayerSize.Count;    
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
            IDataProvider provider = DataProviderFactory.CreateDataProvider(DataProvider);
            if(provider != null && provider.IsInput)
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


    /// <summary>
    /// Class to enable binding to the size of the layer.
    /// </summary>
    public class LayerSize
    {
        public int Size { get; set; }

        public ActivationFunctionEnum ActivationFunction { get; set; }

        public bool HasBias { get; set; }

        public LayerSize()
        {
            Size = 0;
            HasBias = true;
        }

        public BasicLayer CreateLayer(int layersize)
        {
            IActivationFunction act = ActivationFactory.Create(ActivationFunction);
            return new BasicLayer(act, HasBias, layersize);
        }

        public BasicLayer CreateLayer()
        {
            return CreateLayer(Size);
        }
        
    }


}