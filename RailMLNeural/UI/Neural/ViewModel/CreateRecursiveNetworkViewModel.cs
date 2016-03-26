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
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using RailMLNeural.Neural.Algorithms.Propagators;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CreateRecursiveNetworkViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// Defining all parameters in the ViewModel
        /// </summary>
        private RecursiveConfiguration _configuration;
        public RecursiveConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
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

        public PropagatorEnum PropagatorType { get; set; }

        public RecurrentDataProviderEnum DataProvider { get; set; }

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

        public bool ElmanPattern { get; set; }

        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the CreateFeedForwardViewModel class.
        /// </summary>
        public CreateRecursiveNetworkViewModel()
        {
            Configuration = new RecursiveConfiguration();
            HiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            OutputDataProviders = new ObservableCollection<IRecurrentDataProvider>();
            InitializeCommands();
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
        #endregion Public

        #region Privates
        private void CreateNetwork()
        {

            BasicNetwork N = new BasicNetwork();
            List<BasicLayer> Layers = new List<BasicLayer>();
            Layers.Add(new BasicLayer(null, true, Configuration.InputDataProviders.Sum(x => x.Size)));
            for (int i = 0; i < HiddenLayerSize.Count; i++ )
            {
                Layers.Add(HiddenLayerSize[i].CreateLayer());
                if(HiddenLayerSize[i].IsRecurrent)
                {
                    Layers[i].ContextFedBy = Layers[i + 1];
                }
            }
            Layers.Add(OutputLayerSize.CreateLayer(Configuration.OutputDataProviders.Sum(x => x.Size)));
            if(ElmanPattern)
            {
                Layers[0].ContextFedBy = Layers[1];
            }
            for (int i = 0; i < Layers.Count; i++ )
            {
                N.AddLayer(Layers[i]);
            }
            N.Structure.FinalizeStructure();
            Configuration.Network = N;
        }

        private void Reset()
        {
            Configuration = new RecursiveConfiguration();
            HiddenLayerSize = new ObservableCollection<LayerSize>();
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
            Configuration.InputDataProviders = InputDataProviders.ToList();
            Configuration.OutputDataProviders = OutputDataProviders.ToList();
            Configuration.Propagator = PropagatorFactory.Create(Configuration, PropagatorType, true);
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PreProcessing_Finished);
            worker.ProgressChanged += new ProgressChangedEventHandler(PreProcessing_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(Configuration.Initialize);
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
            Messenger.Default.Send<AddNeuralNetworkMessage>(new AddNeuralNetworkMessage() { NeuralNetwork = Configuration });
            Reset();
        }

        /// <summary>
        /// Check if all conditions are fulfilled before being able to create a new network configuration
        /// </summary>
        /// <returns></returns>
        private bool canExecuteCreateNeural()
        {
            return HiddenLayerSize.Count > 0 &&
                Configuration.Name != string.Empty && Configuration.Description != string.Empty &&
                Configuration.Name != null && Configuration.Description != null &&
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