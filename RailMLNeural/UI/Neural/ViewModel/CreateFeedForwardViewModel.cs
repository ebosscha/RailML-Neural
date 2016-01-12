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
        private NeuralNetwork _network;
        public NeuralNetwork Network
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

        private LayerSize _inputLayerSize = new LayerSize();

        public LayerSize InputLayerSize
        { get { return _inputLayerSize; }
            set { _inputLayerSize = value;
            RaisePropertyChanged("InputLayerSize");
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
            Network = new NeuralNetwork();
            Network.Type = AlgorithmEnum.FeedForward;
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
            N.AddLayer(InputLayerSize.CreateLayer(Network.Data.InputSize));
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
            NeuralNetwork NewNetwork = new NeuralNetwork();
            Network = NewNetwork;
            Network.Type = AlgorithmEnum.FeedForward;
            HiddenLayerSize = new ObservableCollection<LayerSize>();
            InputDataProviders = new ObservableCollection<IDataProvider>();
            OutputDataProviders = new ObservableCollection<IDataProvider>();
            InputLayerSize = new LayerSize();
            OutputLayerSize = new LayerSize();
        }

        #endregion Privates


        #region Commands
        /// <summary>
        /// All Commands controlling the buttons in the Create Neural Configuration Window.
        /// </summary>
        public ICommand CreateNeuralCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddInputDataProviderCommand { get; set; }
        public ICommand AddOutputDataProviderCommand { get; set; }

        private void InitializeCommands()
        {
            CreateNeuralCommand = new RelayCommand(ExecuteCreateNeural, canExecuteCreateNeural);
            CancelCommand = new RelayCommand<object>((param) => ExecuteCancel(param));
            AddInputDataProviderCommand = new RelayCommand(ExecuteAddInputDataProvider);
            AddOutputDataProviderCommand = new RelayCommand(ExecuteAddOutputDataProvider);
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

        private void ExecuteAddInputDataProvider()
        {
            switch(DataProvider)
            {
                case DataProviderEnum.PerLineExactInput:
                    InputDataProviders.Add(new PerLineExactInputProvider());
                    break;
                case DataProviderEnum.TimeOfDayInput:
                    InputDataProviders.Add(new TimeInputProvider(24));
                    break;
                case DataProviderEnum.InitialDelayInput:
                    InputDataProviders.Add(new InitialDelaySizeInputProvider());
                    break;
                case DataProviderEnum.LineClassificationInput:
                    InputDataProviders.Add(new LineClassificationInputProvider());
                    break;
                default:
                    break;
            }
        }

        private void ExecuteAddOutputDataProvider()
        {
            switch (DataProvider)
            {
                case DataProviderEnum.PerLineClassificationOutput:
                    OutputDataProviders.Add(new PerLineClassificationOutputProvider());
                    break;
                case DataProviderEnum.PerLineExactOutput:
                    OutputDataProviders.Add(new PerLineExactOutputProvider());
                    break;
                default:
                    break;
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
            switch (ActivationFunction)
            {
                case ActivationFunctionEnum.None:
                    return new BasicLayer(null, HasBias, layersize);
                case ActivationFunctionEnum.BiPolar:
                    return new BasicLayer(new ActivationBiPolar(), HasBias, layersize);
                case ActivationFunctionEnum.BiPolarSteepenedSigmoid:
                    return new BasicLayer(new ActivationBipolarSteepenedSigmoid(), HasBias, layersize);
                case ActivationFunctionEnum.ClippedLinear:
                    return new BasicLayer(new ActivationClippedLinear(), HasBias, layersize);
                case ActivationFunctionEnum.Competitive:
                    return new BasicLayer(new ActivationCompetitive(), HasBias, layersize);
                case ActivationFunctionEnum.Elliott:
                    return new BasicLayer(new ActivationElliott(), HasBias, layersize);
                case ActivationFunctionEnum.ElliottSymmetric:
                    return new BasicLayer(new ActivationElliottSymmetric(), HasBias, layersize);
                case ActivationFunctionEnum.Linear:
                    return new BasicLayer(new ActivationLinear(), HasBias, layersize);
                case ActivationFunctionEnum.LOG:
                    return new BasicLayer(new ActivationLOG(), HasBias, layersize);
                case ActivationFunctionEnum.Ramp:
                    return new BasicLayer(new ActivationRamp(), HasBias, layersize);
                case ActivationFunctionEnum.Sigmoid:
                    return new BasicLayer(new ActivationSigmoid(), HasBias, layersize);
                case ActivationFunctionEnum.SIN:
                    return new BasicLayer(new ActivationSIN(), HasBias, layersize);
                case ActivationFunctionEnum.SoftMax:
                    return new BasicLayer(new ActivationSoftMax(), HasBias, layersize);
                case ActivationFunctionEnum.SteepenedSigmoid:
                    return new BasicLayer(new ActivationSteepenedSigmoid(), HasBias, layersize);
                case ActivationFunctionEnum.TANH:
                    return new BasicLayer(new ActivationTANH(), HasBias, layersize);
                default:
                    return null;
            }
        }

        public BasicLayer CreateLayer()
        {
            return CreateLayer(Size);
        }
        
    }


}