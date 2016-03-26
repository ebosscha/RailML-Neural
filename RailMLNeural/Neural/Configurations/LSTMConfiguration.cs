using Encog.ML.Data;
using Encog.ML.Data.Basic;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Algorithms.RNNSharp;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using RailMLNeural.UI.Logger;
using RNNSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RailMLNeural.Neural.Configurations
{
    class LSTMConfiguration : BaseContainsGraph, INeuralConfiguration
    {
        public override AlgorithmEnum Type { get { return AlgorithmEnum.LSTM; } }
        public LSTM Network { get; set; }
        public DataSet Data2 { get; set; }

        public LSTMConfiguration() : base()
        {

        }

        public override double RunVerification()
        {
            var msecalc = new MSEErrorCalculation();
            var nmsecalc = new WeightedMSEErrorCalculation(OutputDataProviders.Sum(x => x.Size));
            SimplifiedGraph _graph = Graph.Clone();
            IPropagator propagator = Propagator.OpenAdditional();
            foreach (var dc in DataSet.VerificationCollection)
            {
                List<State> states = new List<State>();
                //graph.GenerateGraph(dc, true);
                propagator.NewCycle(_graph, dc, true);
                bool flag = true;
                while (propagator.HasNext)
                {
                    IMLDataPair pair = propagator.MoveNext();
                    if (!propagator.CurrentCorrupted)
                    {
                        if (pair.Ideal[0] != 0)
                        {
                            pair.Significance = 1;
                        }
                        State state = new State();
                        state.ideal = new double[pair.Ideal.Count];
                        pair.Ideal.CopyTo(state.ideal, 0, pair.Ideal.Count);
                        SingleVector DenseData = new SingleVector();
                        DenseData = new SingleVector(pair.Input.Count);
                        for (int i = 0; i < pair.Input.Count; i++)
                        {
                            DenseData[i] = (float)pair.Input[i];
                        }
                        state.DenseData = DenseData;
                        states.Add(state);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag && states.Count > 0)
                {
                    Sequence seq = new Sequence(states.Count);
                    for (int i = 0; i < states.Count; i++)
                    {
                        seq.States[i] = states[i];
                    }
                    double[] output = Network.Predict(seq, RunningMode.Validate);
                    msecalc.UpdateError(new BasicMLData(output), new BasicMLData(seq.States.Last().ideal), 1);
                    nmsecalc.UpdateError(new BasicMLData(output), new BasicMLData(seq.States.Last().ideal), 1);
                }
            }
               
            string msg = "Verification DelayCombination Count : " + DataSet.VerificationCount +
                "\n MSE : " + msecalc.CalculateError() + "\n NMSE : " + nmsecalc.CalculateError() +
                "\n Rsquared : " + (1 - nmsecalc.CalculateError());
            MessageBox.Show(msg);
            return msecalc.CalculateError();
        }
        

        public override void TrainNetwork(object state)
        {
            IsRunning = true;
            for(int i = 0; i < Settings.Epochs; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                ErrorHistory.Add(Network.TrainNet(Data2, i));
                sw.Stop();
                Logger.AddEntry("Finished Iteration. Elapsed Time: " + sw.ElapsedMilliseconds * 1000 + " seconds. Current Error :" + ErrorHistory.Last());
                OnProgressChanged();
            }
            IsRunning = false;
        }

        public override void Reset()
        {
            ErrorHistory = new List<double>();
            Network.CleanStatus();
            Network.InitMem();
            Network.initWeights(); 
        }

        public DataSet CreateDataSet()
        {
            DataSet result = new DataSet(OutputDataProviders.Sum(x => x.Size));
            SimplifiedGraph graph = Graph.Clone();
            IPropagator propagator = Propagator.OpenAdditional();
            foreach (var dc in DataSet.Collection)
            {
                List<State> states = new List<State>();
                //graph.GenerateGraph(dc, true);
                propagator.NewCycle(graph, dc, true);
                bool flag = true;
                while (propagator.HasNext)
                {
                    IMLDataPair pair = propagator.MoveNext();
                    if (!propagator.CurrentCorrupted)
                    {
                        if (pair.Ideal[0] != 0)
                        {
                            pair.Significance = 1;
                        }
                        State state = new State();
                        state.ideal = new double[pair.Ideal.Count];
                        pair.Ideal.CopyTo(state.ideal, 0, pair.Ideal.Count);
                        SingleVector DenseData = new SingleVector();
                        DenseData = new SingleVector(pair.Input.Count);
                        for (int i = 0; i < pair.Input.Count; i++)
                        {
                            DenseData[i] = (float)pair.Input[i];
                        }
                        state.DenseData = DenseData;
                        states.Add(state);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag && states.Count > 0)
                {
                    Sequence seq = new Sequence(states.Count);
                    for(int i = 0; i < states.Count; i++)
                    {
                        seq.States[i] = states[i];
                    }
                    result.SequenceList.Add(seq);
                }
            }
            return result;
        }

        public static LSTMConfiguration Test()
        {
            LSTMConfiguration conf = new LSTMConfiguration();
            conf.OutputDataProviders.Add(new InclusiveTotalDelayOutputRecurrentProvider());
            conf.InputDataProviders.Add(new DoubleTrackInputRecurrentProvider());
            conf.InputDataProviders.Add(new RouteLengthInputRecurrentProvider());
            conf.InputDataProviders.Add(new StationTracksInputRecurrentProvider());
            conf.InputDataProviders.Add(new EdgeDepartureCountInputRecurrentProvider());
            conf.InputDataProviders.Add(new VertexDepartureCountInputRecurrentProvider());
            conf.InputDataProviders.Add(new TrainSpeedInputRecurrentProvider());
            conf.InputDataProviders.Add(new EdgeMaxSpeedInputRecurrentProvider());
            conf.PerDay = false;
            conf.ExcludeEmpty = false;
            conf.JustFirstOrder = true;
            conf.Propagator = PropagatorFactory.Create(conf, PropagatorEnum.FollowTrain, true);

            Logger.AddEntry("Started Initializing Recursive Configuration");
            conf.Graph = new SimplifiedGraph();
            while (conf.Graph.RunningThreads > 0)
            {
                Thread.Sleep(1);
            }
            conf.DataSet = new Data.DelayCombinationSet(DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Value.Count > 0).Select(x => x.Key), conf.PerDay, conf.ExcludeEmpty, conf.JustFirstOrder);
            conf.DataSet.Split(0.4);
            conf.Data2 = conf.CreateDataSet();

            conf.Network = new LSTM();
            conf.Network.L0 = conf.InputDataProviders.Sum(x => x.Size);
            conf.Network.L1 = 50;
            conf.Network.L2 = conf.OutputDataProviders.Sum(x => x.Size);
            conf.Network.IsCRFTraining = false;
            conf.Network.Dropout = 0F;
            conf.Network.GradientCutoff = 5000000;
            conf.Network.ModelDirection = MODELDIRECTION.FORWARD;
            conf.Network.DenseFeatureSize = conf.InputDataProviders.Sum(x => x.Size);
            conf.Network.CleanStatus();
            conf.Network.InitMem();
            conf.Network.initWeights();
            conf.Network.parallelOption.MaxDegreeOfParallelism = 24;

            return conf;
        }

        #region Inherited
        public override Encog.ML.Data.IMLData Compute(Encog.ML.Data.IMLData Data)
        {
            throw new NotImplementedException();
        }

        public override IList<Encog.ML.Data.IMLDataSet> CreateData()
        {
            throw new NotImplementedException();
        }

        public override Data.NormBuffMLDataSet Data
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Process(RailMLNeural.Data.SimplifiedGraph Graph, PreProcessing.DelayCombination DC)
        {
            throw new NotImplementedException();
        }
        #endregion Inherited
    }
}
