using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using RailML___WPF.Data;
using RailML___WPF.NeuralNetwork.PreProcessing;
using Encog.Neural.Networks;
using Encog.Engine.Network.Activation;
using Encog.Neural.Networks.Layers;
using System.ComponentModel;
using System.Threading;

namespace RailML___WPF.NeuralNetwork.Algorithms
{
    class PerLineClassification
    {
        private BackgroundWorker worker;
        private bool completed = false; 

        public void Train(object sender, DoWorkEventArgs e)
        {   
            worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "Starting... Preprocessing DataSet.");
            PreProcesser pproc = new PreProcesser();
            BackgroundWorker worker2 = new BackgroundWorker();
            worker2.WorkerReportsProgress = true;
            worker2.DoWork += new DoWorkEventHandler(pproc.PerLineClassification);
            worker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Preprocessing_Completed);
            worker2.ProgressChanged += new ProgressChangedEventHandler(Preprocessing_ProgressChanged);
            //pproc.PerLineClassification(worker2, new DoWorkEventArgs(DoWorkEventArgs.Empty));
            worker2.RunWorkerAsync();
            while(!completed)
            {
                Thread.Sleep(100);
            }

        }

        private void Preprocessing_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            worker.ReportProgress(0, e.UserState);
        }


        private void Preprocessing_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.ReportProgress(0, "Creating Network...");
            BasicNetwork Network = new BasicNetwork();
            Network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, DataContainer.NeuralNetwork.Data.InputSize));
            Network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 50));
            Network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, DataContainer.NeuralNetwork.Data.IdealSize));
            Network.Structure.FinalizeStructure();
            Network.Reset();
            DataContainer.NeuralNetwork.Network = Network;

            ResilientPropagation training = new ResilientPropagation(DataContainer.NeuralNetwork.Network, DataContainer.NeuralNetwork.Data);
            worker.ReportProgress(0, "Running Training: Epoch 0");
            for(int i = 0; i < 200; i++)
            {
                training.Iteration();
                worker.ReportProgress(0, "Running Training: Epoch " + (i+1).ToString() + "     Current Training Error : " + training.Error.ToString());
                if(worker.CancellationPending == true)
                {
                    completed = true;
                    return;
                }

            }
            completed = true;
        }

    }
}
