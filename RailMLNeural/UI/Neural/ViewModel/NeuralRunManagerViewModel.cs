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

        }

        private void ExecuteAddToBatch()
        {

        }

        #endregion Commands
    }
}