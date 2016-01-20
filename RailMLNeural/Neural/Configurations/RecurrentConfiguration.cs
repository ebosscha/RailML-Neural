using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.EA.Train;
using Encog.ML.Genetic;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Pattern;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RailMLNeural.Neural.Configurations
{
    [Serializable]
    class RecurrentConfiguration : INeuralConfiguration
    {
        #region Parameters
        public RecurrentNetwork Network { get; set; }
        public AlgorithmEnum Type { get { return AlgorithmEnum.Recurrent; } }
        public IMLTrain Training { get; set; }
        public NeuralSettings Settings { get; set; }
        public IPropagator Propagator { get; set; }
        public List<IRecurrentDataProvider> InputDataProviders { get; set; }
        public List<IRecurrentDataProvider> OutputDataProviders { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; private set; }
        public List<double> ErrorHistory { get; set; }
        public List<double> VerificationHistory { get; set; }
        public List<string> InputMap { get; set; }
        public List<string> OutputMap { get; set; }
        public event EventHandler ProgressChanged;

        private SimplifiedGraph _graph;
        public SimplifiedGraph Graph { get { return _graph; } }
        #endregion Parameters

        #region Public
        /// <summary>
        /// Constructs a new Recurrent Configuration
        /// </summary>
        public RecurrentConfiguration()
        {
            ErrorHistory = new List<double>();
            VerificationHistory = new List<double>();
            InputDataProviders = new List<IRecurrentDataProvider>();
            OutputDataProviders = new List<IRecurrentDataProvider>();
            InputMap = new List<string>();
            OutputMap = new List<string>();
            _graph = new SimplifiedGraph();
            Settings = new NeuralSettings();
        }

        public void TrainNetwork(object state)
        {
            if(Training == null)
            {
                throw new Exception("Training is null");
            }
            for (int i = 0; i < Settings.Epochs; i++)
            {
                Training.Iteration();
                ErrorHistory.Add(Training.Error);
                OnProgressChanged();
            }
        }

        public IMLData Compute(IMLData Data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        public void AddTraining(RecurrentTrainingEnum Type)
        {
            switch(Type)
            {
                case RecurrentTrainingEnum.Genetic:
                    AddGeneticTraining();
                    break;
                case RecurrentTrainingEnum.SimulatedAnnealing:
                    AddAnnealingTraining();
                    break;
                default:
                    break;
            }
        }


        #endregion Public

        #region Private
        private void AddGeneticTraining()
        {

            //Training = new MLMethodGeneticAlgorithm()
        }

        private void AddAnnealingTraining()
        {
            Training = new NeuralSimulatedAnnealing(Network, new RecurrentCalculateScore(this),1,0.01,10);
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
        #endregion Private

        #region Test
        public static RecurrentConfiguration Test()
        {
            RecurrentConfiguration result = new RecurrentConfiguration()
            {
                Name = "Test",
                Description = "RecurrentNetworkTest"
            };
            result.InputDataProviders.Add(new DelaySizeInputRecurrentProvider());
            result.OutputDataProviders.Add(new DelaySizeInputRecurrentProvider());
            FeedForwardPattern pattern = new FeedForwardPattern();
            pattern.InputNeurons = 3;
            pattern.OutputNeurons = 3;
            pattern.AddHiddenLayer(30);
            pattern.ActivationFunction = new ActivationSigmoid();
            result.Network = new RecurrentNetwork(result);
            result.Network.Network = (BasicNetwork)pattern.Generate();
            result.Network.Network.Reset();
            result.Propagator = new ChronologicalPropagator(result);
            result.AddAnnealingTraining();
            return result;
            
                
        }
        #endregion Test


    }
    
    public enum RecurrentTrainingEnum
    {
        Genetic,
        SimulatedAnnealing
    }
}
