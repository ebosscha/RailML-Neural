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

        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the NeuralRunManagerViewModel class.
        /// </summary>
        public NeuralRunManagerViewModel()
        {
            InitializeCommands();
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (msg) =>
                Network = msg.NeuralNetwork);
        }
        #endregion Public

        #region Private
        
        #endregion Private

        #region Commands
        public ICommand RunNetworkCommand { get; private set; }
        public ICommand AddToBatchCommand { get; private set; }

        private void InitializeCommands()
        {
            RunNetworkCommand = new RelayCommand(ExecuteRunNetwork);
            AddToBatchCommand = new RelayCommand(ExecuteAddToBatch);
        }

        private void ExecuteRunNetwork()
        {
            switch(LearningAlgorithm)
            {
                case LearningAlgorithmEnum.BackPropagation:
                    Network.Training = new Backpropagation(Network.Network, Network.Data, Network.Settings.LearningRate, Network.Settings.Momentum);
                    break;
                case LearningAlgorithmEnum.ResilientPropagation:
                    Network.Training = new ResilientPropagation(Network.Network, Network.Data);
                    break;
                default:
                    return;

            }

            ThreadPool.QueueUserWorkItem(Network.RunNetwork);

        }

        private void ExecuteAddToBatch()
        {

        }

        #endregion Commands
    }
}