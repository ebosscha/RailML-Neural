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
using RailMLNeural.Neural.Configurations;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation;
using RailMLNeural.Neural.Algorithms;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.MathUtil.Randomize;
using Encog.Neural.Networks.Training.PSO;
using RailMLNeural.Neural.Algorithms.Training;

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
        private INeuralConfiguration _network;

        public INeuralConfiguration Network
        {
            get { return _network; }
            set { 
                if(_network == value) {return;}
                _network = value;
                RaisePropertyChanged("Network");
                RaisePropertyChanged("Visibility");
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

        private LearningAlgorithmEnum _learningAlgorithm;

        public LearningAlgorithmEnum LearningAlgorithm { 
            get
            {return _learningAlgorithm;}
            set{ _learningAlgorithm = value;
                RaisePropertyChanged("LearningAlgorithm");
                RaisePropertyChanged("LearningRateVisible");
                RaisePropertyChanged("MomentumVisible");
                RaisePropertyChanged("StartTempVisible");
                RaisePropertyChanged("EndTempVisible");
                RaisePropertyChanged("CyclesVisible");
                RaisePropertyChanged("RpropTypeVisible");
                RaisePropertyChanged("PSOVisible");

            }
        }

        public IEnumerable<LearningAlgorithmEnum> LearningAlgorithmEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(LearningAlgorithmEnum)).Cast<LearningAlgorithmEnum>();
            }
        }

        private ObservableCollection<INeuralConfiguration> _batchCollection;

        public ObservableCollection<INeuralConfiguration> BatchCollection
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
            BatchCollection = new ObservableCollection<INeuralConfiguration>(BatchManager.Queue);
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (msg) =>
                Network = msg.NeuralNetwork);
            BatchManager.QueueChanged += new EventHandler(OnBatchChanged);
            C1 = 2.0;
            C2 = 2.0;
            Inertia = 0.4;
            MaxVelocity = 2.0;
        }
        #endregion Public

        #region Private
        private IMLTrain AddLearningAlgorithm()
        {
            if (Network is FeedForwardConfiguration)
            {
                FeedForwardConfiguration tempconf = Network as FeedForwardConfiguration;
                switch (LearningAlgorithm)
                {
                    case LearningAlgorithmEnum.BackPropagation:
                        return new Backpropagation(tempconf.Network, tempconf.Data, LearningRate, Momentum);
                    case LearningAlgorithmEnum.ResilientPropagation:
                        ResilientPropagation prop = new ResilientPropagation(tempconf.Network, tempconf.Data);
                        prop.RType = ResilientPropType;
                        return prop;
                    case LearningAlgorithmEnum.ManhattanPropagation:
                        return new ManhattanPropagation(tempconf.Network, tempconf.Data, LearningRate);
                    case LearningAlgorithmEnum.QuickPropagation:
                        return new QuickPropagation(tempconf.Network, tempconf.Data);
                    case LearningAlgorithmEnum.ScaledConjugateGradient:
                        return new ScaledConjugateGradient(tempconf.Network, tempconf.Data);
                    case LearningAlgorithmEnum.SimulatedAnnealing:
                        return new NeuralSimulatedAnnealing(tempconf.Network, new DataSetCalculateScore(tempconf.Data), StartTemp, EndTemp, Cycles);
                    case LearningAlgorithmEnum.ParticleSwarmOptimization:
                        NeuralPSO temp = new NeuralPSO(tempconf.Network, new GaussianRandomizer(0, 1), new DataSetCalculateScore(tempconf.Data), Cycles);
                        temp.MaxVelocity = MaxVelocity;
                        temp.C1 = C1;
                        temp.C2 = C2;
                        temp.InertiaWeight = Inertia;
                        return temp;
                    default:
                        return null;
                }
            }
            else if (Network is RecursiveConfiguration)
            {
                RecursiveConfiguration tempconf = Network as RecursiveConfiguration;
                switch (LearningAlgorithm)
                {
                    case LearningAlgorithmEnum.BackPropagation:
                        return new Backpropagation(tempconf.Network, tempconf.Data, LearningRate, Momentum);
                    case LearningAlgorithmEnum.ResilientPropagation:
                        ResilientPropagation prop = new ResilientPropagation(tempconf.Network, tempconf.Data);
                        prop.RType = ResilientPropType;
                        return prop;
                    case LearningAlgorithmEnum.ManhattanPropagation:
                        return new ManhattanPropagation(tempconf.Network, tempconf.Data, LearningRate);
                    case LearningAlgorithmEnum.QuickPropagation:
                        return new QuickPropagation(tempconf.Network, tempconf.Data);
                    case LearningAlgorithmEnum.ScaledConjugateGradient:
                        return new ScaledConjugateGradient(tempconf.Network, tempconf.Data);
                    case LearningAlgorithmEnum.SimulatedAnnealing:
                        return new NeuralSimulatedAnnealing(tempconf.Network, new DataSetCalculateScore(tempconf.Data), StartTemp, EndTemp, Cycles);
                    case LearningAlgorithmEnum.ParticleSwarmOptimization:
                        NeuralPSO temp = new NeuralPSO(tempconf.Network, new GaussianRandomizer(0, 1), new DataSetCalculateScore(tempconf.Data), Cycles);
                        temp.MaxVelocity = MaxVelocity;
                        temp.C1 = C1;
                        temp.C2 = C2;
                        temp.InertiaWeight = Inertia;
                        return temp;
                    default:
                        return null;
                }
            }
            else if(Network is GRNNConfiguration)
            {
                GRNNConfiguration tempconf = Network as GRNNConfiguration;
                switch (LearningAlgorithm)
                {
                    case LearningAlgorithmEnum.SimulatedAnnealing:
                        return new BatchNeuralSimulatedAnnealing(tempconf.Network, new RecurrentCalculateScore(tempconf, tempconf.DataSet), StartTemp, EndTemp, Cycles);
                    case LearningAlgorithmEnum.ParticleSwarmOptimization:
                        GraphNeuralPSO temp = new GraphNeuralPSO(tempconf.Network, new GaussianRandomizer(0, 1), new RecurrentCalculateScore(tempconf, tempconf.DataSet), Cycles);
                        temp.MaxVelocity = MaxVelocity;
                        temp.C1 = C1;
                        temp.C2 = C2;
                        temp.InertiaWeight = Inertia;
                        return temp;
                    case LearningAlgorithmEnum.ResilientPropagation:
                        GNResilientPropagation prop = new GNResilientPropagation(tempconf, tempconf.Network, tempconf.DataSet);
                        prop.RType = ResilientPropType;
                        return prop;
                    default:
                        return null;
                }
            }
            return null;            
        }

        /// <summary>
        /// Updates the observablecollection to update the datagrid with the batch in the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBatchChanged(object sender, EventArgs e)
        {
            BatchCollection = new ObservableCollection<INeuralConfiguration>(BatchManager.Queue);
        }

        #endregion Private

        #region TrainingParameters

        public double LearningRate { get; set; }
        public double Momentum { get; set; }
        public double StartTemp { get; set; }
        public int BatchSize { get; set; }
        public double EndTemp { get; set; }
        public int Cycles { get; set; }
        public bool Greedy { get; set; }
        public RPROPType ResilientPropType { get; set; }
        public IEnumerable<RPROPType> RPROPTypeValues { get
            { return Enum.GetValues(typeof(RPROPType)).Cast<RPROPType>();}
        }
        public double C1 { get; set; }
        public double C2 { get; set; }
        public double MaxVelocity { get; set; }
        public double Inertia { get; set; }

        private static Dictionary<bool, Visibility> BoolToVis = new Dictionary<bool,Visibility>(){{true, Visibility.Visible}, {false, Visibility.Collapsed}};
        private static List<LearningAlgorithmEnum> HasLearningRate = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.BackPropagation, LearningAlgorithmEnum.ManhattanPropagation };
        private static List<LearningAlgorithmEnum> HasMomentum = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.BackPropagation};
        private static List<LearningAlgorithmEnum> HasStartTemp = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.SimulatedAnnealing };
        private static List<LearningAlgorithmEnum> HasEndTemp = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.SimulatedAnnealing };
        private static List<LearningAlgorithmEnum> HasCycles = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.SimulatedAnnealing, LearningAlgorithmEnum.ParticleSwarmOptimization};
        private static List<LearningAlgorithmEnum> HasRPROPType = new List<LearningAlgorithmEnum> { LearningAlgorithmEnum.ResilientPropagation };

        public Visibility LearningRateVisible { get { return BoolToVis[HasLearningRate.Contains(LearningAlgorithm)];} }
        public Visibility MomentumVisible { get { return BoolToVis[HasMomentum.Contains(LearningAlgorithm)];} }
        public Visibility StartTempVisible { get { return BoolToVis[HasStartTemp.Contains(LearningAlgorithm)];} }
        public Visibility EndTempVisible { get { return BoolToVis[HasEndTemp.Contains(LearningAlgorithm)];} }
        public Visibility CyclesVisible { get { return BoolToVis[HasCycles.Contains(LearningAlgorithm)];} }
        public Visibility RPropTypeVisible { get { return BoolToVis[HasRPROPType.Contains(LearningAlgorithm)]; } }
        public Visibility PSOVisible { get { return BoolToVis[LearningAlgorithm == LearningAlgorithmEnum.ParticleSwarmOptimization]; } }

        #endregion TrainingParameters
        #region Commands
        public ICommand TrainNetworkCommand { get; private set; }
        public ICommand AddToBatchCommand { get; private set; }
        public ICommand RunBatchCommand { get; private set; }
        public ICommand StopTrainingCommand { get; private set; }
        public ICommand ResetNetworkCommand { get; private set; }
        public ICommand AddTrainingCommand { get; private set; }
        public ICommand AddAlternateTrainingCommand { get; private set; }

        private void InitializeCommands()
        {
            TrainNetworkCommand = new RelayCommand(ExecuteTrainNetwork, CanExecuteRun);
            AddToBatchCommand = new RelayCommand(ExecuteAddToBatch, CanExecuteRun);
            RunBatchCommand = new RelayCommand(ExecuteRunBatch);
            AddTrainingCommand = new RelayCommand(ExecuteAddTraining);
            AddAlternateTrainingCommand = new RelayCommand(ExecuteAddAlternateTraining, CanExecuteRun);
            StopTrainingCommand = new RelayCommand(ExecuteStopTraining);
            ResetNetworkCommand = new RelayCommand(ExecuteResetNetwork);
        }

        private void ExecuteTrainNetwork()
        {
            AddLearningAlgorithm();
            //ThreadPool.QueueUserWorkItem(Network.TrainNetwork);
            Network.TrainNetwork();

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

        private void ExecuteStopTraining()
        {
            Network.StopTraining();
        }

        private void ExecuteResetNetwork()
        {
            Network.Reset();
        }

        private bool CanExecuteRun()
        {
            return Network != null && Network.Training != null;
        }

        private void ExecuteAddTraining()
        {
            Network.Training = AddLearningAlgorithm();
            if (Network.Training is IBatchSize)
            {
                ((IBatchSize)_network.Training).BatchSize = BatchSize;
            }
            if(Greedy)
            {
                Network.Training.AddStrategy(new Greedy());
            }
        }

        private void ExecuteAddAlternateTraining()
        {
            IStrategy strat = new HybridStrategy(AddLearningAlgorithm());
            Network.Training.AddStrategy(strat);
        }

        #endregion Commands
    }
}