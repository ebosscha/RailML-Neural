using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Genetic;
using Encog.ML.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Pattern;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.UI.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RailMLNeural.Neural.Configurations
{
    [Serializable]
    public class NEATConfiguration : BaseContainsGraph, INeuralConfiguration, IContainsGraph
    {
        #region Parameters
        public NEATNetwork Network { get; set; }
        public override NormBuffMLDataSet Data
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
        public override AlgorithmEnum Type { get { return AlgorithmEnum.Recursive; } }
        public List<DateTime> VerificationDates { get; private set; }

        #endregion Parameters

        #region Public
        /// <summary>
        /// Constructs a new Recurrent Configuration
        /// </summary>
        public NEATConfiguration() : base()
        {
            InputDataProviders = new List<IRecurrentDataProvider>();
            OutputDataProviders = new List<IRecurrentDataProvider>();
            VerificationDates = new List<DateTime>();
            Settings = new NeuralSettings();
        }

        public override void TrainNetwork(object state)
        {
            base.TrainNetwork(state);
            Network = (NEATNetwork)((TrainEA)Training).CODEC.Decode(((TrainEA)Training).BestGenome);
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override IMLData Compute(IMLData Data)
        {
            return Network.Compute(Data);
        }

        public override void Dispose()
        {
            base.Dispose();
            Data = null;
        }
        #endregion Public

        #region Initialization
        public void Initialize(object sender, DoWorkEventArgs e)
        {
            using (BackgroundWorker worker = sender as BackgroundWorker)
            {
                Logger.AddEntry("Started Initializing Recursive Configuration");
                List<DateTime> Dates = new List<DateTime>();
                worker.ReportProgress(0, "Creating Graph...");
                Graph = new SimplifiedGraph();
                while (Graph.RunningThreads > 0)
                {
                    Thread.Sleep(1);
                }
                DataSet = new DelayCombinationSet(DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Value.Count > 0).Select(x => x.Key), PerDay);
                DataSet.Split(Settings.VerificationSize);                    
                Logger.AddEntry("Initialization Finished");
            }
        }

        public override IList<IMLDataSet> CreateData()
        {
            throw new NotImplementedException();
        }

        public static NEATConfiguration Test()
        {
            NEATConfiguration conf = new NEATConfiguration();
            conf.OutputDataProviders.Add(new TotalDelayOutputRecurrentProvider());
            conf.InputDataProviders.Add(new DoubleTrackInputRecurrentProvider());
            conf.InputDataProviders.Add(new RouteLengthInputRecurrentProvider());
            conf.InputDataProviders.Add(new StationTracksInputRecurrentProvider());
            conf.InputDataProviders.Add(new EdgeDepartureCountInputRecurrentProvider());
            conf.InputDataProviders.Add(new VertexDepartureCountInputRecurrentProvider());
            conf.InputDataProviders.Add(new SwitchCountInputRecurrentProvider());
            conf.PerDay = true;
            conf.ExcludeEmpty = true;
            conf.Propagator = PropagatorFactory.Create(conf, PropagatorEnum.FollowTrain, true);

            Logger.AddEntry("Started Initializing Recursive Configuration");
            List<DateTime> Dates = new List<DateTime>();
            conf.Graph = new SimplifiedGraph();
            while (conf.Graph.RunningThreads > 0)
            {
                Thread.Sleep(1);
            }
            conf.DataSet = new DelayCombinationSet(DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Value.Count > 0).Select(x => x.Key), conf.PerDay);
            conf.DataSet.Split(conf.Settings.VerificationSize);
            Logger.AddEntry("Initialization Finished");
            return conf;

        }

        public override void Process(SimplifiedGraph Graph, DelayCombination DC)
        {
            IPropagator prop = Propagator.OpenAdditional();
            Graph.GenerateGraph(DC, true);
            Propagator.NewCycle(Graph, DC, false);
            while (Propagator.HasNext)
            {
                IMLDataPair pair = Propagator.MoveNext();
                EdgeTrainRepresentation rep = Propagator.Current as EdgeTrainRepresentation;
                IMLData output = Network.Compute(pair.Input);
                if (!Propagator.IgnoreCurrent)
                {
                    Propagator.Update(output);
                }
            }
        }
        #endregion Public

        #region Private

        #endregion Private



        #region Serialization
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        #endregion Serialization




        public override double RunVerification()
        {
            throw new NotImplementedException();
        }
    }
}

