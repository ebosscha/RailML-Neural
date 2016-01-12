using GalaSoft.MvvmLight;
using RailMLNeural.Data;
using RailMLNeural.Neural;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using System.Threading;
using System.Collections.ObjectModel;
using Encog.Neural.Networks.Training.Propagation.Manhattan;
using Encog.Neural.Networks.Training.Propagation.Quick;
using Encog.Neural.Networks.Training.Propagation.SCG;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NeuralRunManagerViewModel : ViewModelBase
    {
        #region Parameters
        private NeuralNetwork _network;

        public NeuralNetwork Network
        {
            get { return _network; }
            set { 
                if(_network == value) {return;}
                _network = value;
                RaisePropertyChanged("Network");
                RaisePropertyChanged("Visibility");
                RaisePropertyChanged("IsEnabled");
            }
        }

        public Visibility Visibility
        {
            get
            {
                if (Network == null) { return System.Windows.Visibility.Hidden; }
                return System.Windows.Visibility.Visible;
            }
        }

        public bool IsEnabled
        {
            get
            {
                if (Network == null) { return false; }
                return !Network.IsRunning;
            }
        }

        public System.Windows.Visibility StatusTextVisibility
        {
            get
            {
                if (Network != null && Network.IsRunning) { return System.Windows.Visibility.Visible; }
                return System.Windows.Visibility.Hidden;
            }
        }

        public LearningAlgorithmEnum LearningAlgorithm { get; set; }

        public IEnumerable<LearningAlgorithmEnum> LearningAlgorithmEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(LearningAlgorithmEnum)).Cast<LearningAlgorithmEnum>();
            }
        }

        private ObservableCollection<NeuralNetwork> _batchCollection;

        public ObservableCollection<NeuralNetwork> BatchCollection
        {
            get { return _batchCollection; }
            set { _batchCollection = value;
            RaisePropertyChanged("BatchCollection");
            }
        }


        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the NeuralRunManagerViewModel class.
        /// </summary>
        public NeuralRunManagerViewModel()
        {
            InitializeCommands();
            BatchCollection = new ObservableCollection<NeuralNetwork>(BatchManager.Queue);
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (msg) =>
                Network = msg.NeuralNetwork);
            BatchManager.QueueChanged += new EventHandler(OnBatchChanged);
        }
        #endregion Public

        #region Private
        private void AddLearningAlgorithm()
        {
            switch (LearningAlgorithm)
            {
                case LearningAlgorithmEnum.BackPropagation:
                    Network.Training = new Backpropagation(Network.Network, Network.Data, Network.Settings.LearningRate, Network.Settings.Momentum);
                    break;
                case LearningAlgorithmEnum.ResilientPropagation:
                    Network.Training = new ResilientPropagation(Network.Network, Network.Data);
                    break;
                case LearningAlgorithmEnum.ManhattanPropagation:
                    Network.Training = new ManhattanPropagation(Network.Network, Network.Data, Network.Settings.LearningRate);
                    break;
                case LearningAlgorithmEnum.QuickPropagation:
                    Network.Training = new QuickPropagation(Network.Network, Network.Data);
                    break;
                case LearningAlgorithmEnum.ScaledConjugateGradient:
                    Network.Training = new ScaledConjugateGradient(Network.Network, Network.Data);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Updates the observablecollection to update the datagrid with the batch in the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBatchChanged(object sender, EventArgs e)
        {
            BatchCollection = new ObservableCollection<NeuralNetwork>(BatchManager.Queue);
        }

        #endregion Private

        #region Commands
        public ICommand TrainNetworkCommand { get; private set; }
        public ICommand AddToBatchCommand { get; private set; }
        public ICommand RunBatchCommand { get; private set; }

        private void InitializeCommands()
        {
            TrainNetworkCommand = new RelayCommand(ExecuteTrainNetwork);
            AddToBatchCommand = new RelayCommand(ExecuteAddToBatch);
            RunBatchCommand = new RelayCommand(ExecuteRunBatch);
        }

        private void ExecuteTrainNetwork()
        {
            AddLearningAlgorithm();
            ThreadPool.QueueUserWorkItem(Network.TrainNetwork);
            //Network.TrainNetwork(0);

        }

        private void ExecuteAddToBatch()
        {
            AddLearningAlgorithm(); 
            BatchManager.Add(Network);
        }

        private void ExecuteRunBatch()
        {
            BatchManager.RunBatch();
        }

        #endregion Commands
    }
}