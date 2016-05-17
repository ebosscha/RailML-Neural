using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
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
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.UI.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RailMLNeural.Neural.Configurations
{
    [Serializable]
    public class RecursiveConfiguration : BaseContainsGraph, INeuralConfiguration, IContainsGraph
    {
        #region Parameters
        public BasicNetwork Network { get; set; }
        public override AlgorithmEnum Type { get { return AlgorithmEnum.Recursive; } }
        public override NormBuffMLDataSet Data { get; set; }
        public List<DateTime> VerificationDates { get; private set; }

        #endregion Parameters

        #region Public
        /// <summary>
        /// Constructs a new Recurrent Configuration
        /// </summary>
        public RecursiveConfiguration()
            : base()
        {
            VerificationDates = new List<DateTime>();
        }

        public override void Reset()
        {
            if (!IsRunning)
            {
                Network.Flat.Randomize(0.1, -0.1);
                ErrorHistory.Clear();
                VerificationHistory.Clear();
            }
            else
            {
                MessageBox.Show("Error: Network still running.");
            }
        }

        public override IMLData Compute(IMLData Data)
        {
            return Network.Compute(Data);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Data != null)
            {
                Data.Close();
            }
            Data = null;
        }

        #region Initialization
        public void Initialize(object sender, DoWorkEventArgs e)
        {
            using (BackgroundWorker worker = sender as BackgroundWorker)
            {
                List<DateTime> Dates = new List<DateTime>();
                worker.ReportProgress(0, "Creating Graph...");
                Graph = new SimplifiedGraph();
                while (Graph.RunningThreads > 0)
                {
                    Thread.Sleep(1);
                }
                DataSet = new DelayCombinationSet(DataContainer.DelayCombinations.dict.Where(x => x.Value != null && x.Value.Count > 0).Select(x => x.Key), PerDay, ExcludeEmpty, JustFirstOrder);
                DataSet.Split(Settings.VerificationSize);
            }
        }

        private void CreateDataSet()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.AddExtension = true;
            dlg.DefaultExt = ".txt";
            dlg.Filter = "txt files (.txt.)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                Data = new NormBuffMLDataSet(dlg.FileName);
            }
        }
        #endregion Initialization

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

        public override IList<IMLDataSet> CreateData()
        {
            List<IMLDataSet> result = new List<IMLDataSet>();
            SimplifiedGraph graph = Graph.Clone();
            IPropagator propagator = Propagator.OpenAdditional();
            foreach (var dc in DataSet.Collection)
            {
                BasicMLDataSet set = new BasicMLDataSet();
                //graph.GenerateGraph(dc, true);
                propagator.NewCycle(graph, dc, true);
                bool flag = true;
                while (propagator.HasNext)
                {
                    IMLDataPair pair = propagator.MoveNext();
                    if (!propagator.IgnoreCurrent)
                    {
                        ClampOutput(ref pair);
                    }
                    if (!propagator.CurrentCorrupted)
                    {
                        if(pair.Ideal[0] != 0)
                        {
                            pair.Significance = 1;
                        }
                        set.Add(pair);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    result.Add(set);
                }
            }

            return result;
        }
        #endregion Public

        #region Private
        private void ClampOutput(ref IMLDataPair pair)
        {
            var outputact = Network.Flat.ActivationFunctions[0];
            double[] output = new double[pair.Ideal.Count];
            pair.Ideal.CopyTo(output, 0, output.Length);
            double min = double.NegativeInfinity;
            double max = double.PositiveInfinity;
            if(outputact is ActivationSoftPlus || outputact is ActivationReLu)
            {
                min = 0;
            }
            else if(outputact is ActivationSigmoid || outputact is ActivationBiPolar || outputact is ActivationSoftMax)
            {
                min = 0;
                max = 1;
            }
            else if(outputact is ActivationTANH || outputact is ActivationSteepenedSigmoid)
            {
                min = -1;
                max = 1;
            }
            for(int i = 0; i < output.Length; i++)
            {
                output[i] = Math.Max(min, Math.Min(output[i], max));
            }
            pair = new BasicMLDataPair(pair.Input, new BasicMLData(output));

        }

        #endregion Private

        #region Serialization
        public RecursiveConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            MemoryStream stream = new MemoryStream((byte[])info.GetValue("Network", typeof(byte[])));
            Network = (BasicNetwork)Encog.Persist.EncogDirectoryPersistence.LoadObject(stream);
            try
            {
                Network.Flat.Weights = (double[])info.GetValue("Weights", typeof(double[]));
            }
            catch(Exception ex){}
            
            
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            MemoryStream stream = new MemoryStream();
            Encog.Persist.EncogDirectoryPersistence.SaveObject(stream, Network);
            info.AddValue("Network", stream.ToArray(), typeof(byte[]));
            info.AddValue("Weights", Network.Flat.Weights, typeof(double[]));
        }
        #endregion Serialization


        public override double RunVerification()
        {
            SimplifiedGraph _graph = Graph.Clone();
            IPropagator prop = Propagator.OpenAdditional();
            ValidationHelper valHelp = new ValidationHelper(OutputDataProviders.Sum(x => x.Size));
            foreach(var DC in DataSet.VerificationCollection)
            {
                //_graph.GenerateGraph(DC, true);
                prop.NewCycle(_graph, DC, true);
                while(prop.HasNext)
                {
                    var pair = prop.MoveNext();
                    var output = Network.Compute(pair.Input);
                    if(!prop.IgnoreCurrent)
                    {
                        prop.Update(output);
                        valHelp.Add(output, pair.Ideal);
                    }
                }
            }
            var result = MessageBox.Show(valHelp.PublishMSE(), "Validation Output", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                valHelp.SaveHistogram();
            }
            return 0.0;

        }
    }

    public static class Extentions
    {
        public static List<List<T>> Split<T>(this List<T> source, int ChunckSize)
        {
            return source
         .Select((x, i) => new { Index = i, Value = x })
         .GroupBy(x => x.Index / ChunckSize)
         .Select(x => x.Select(v => v.Value).ToList())
         .ToList();
        }
    }
}

