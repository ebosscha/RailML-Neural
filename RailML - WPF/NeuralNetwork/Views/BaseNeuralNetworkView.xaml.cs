using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RailML___WPF.NeuralNetwork.ViewModels;
using Encog.Neural.Networks;
using System.ComponentModel;
using RailML___WPF.NeuralNetwork.Algorithms;

namespace RailML___WPF.NeuralNetwork.Views
{
    /// <summary>
    /// Interaction logic for BaseNeuralNetworkView.xaml
    /// </summary>
    public partial class BaseNeuralNetworkView : UserControl
    {
        private BaseNeuralNetworkViewModel viewmodel;
        private BackgroundWorker worker;

        public BaseNeuralNetworkView()
        {
            InitializeComponent();
            this.DataContext = new BaseNeuralNetworkViewModel();
            ParameterContentControl.Content = new ParameterViewModel();

        }


        public void RunPerLine_Click(object sender, EventArgs e)
        {
            PerLineClassification alg = new PerLineClassification();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            //alg.Train(worker, new DoWorkEventArgs("test"));
            worker.DoWork += new DoWorkEventHandler(alg.Train);
            worker.ProgressChanged += new ProgressChangedEventHandler(Neural_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Neural_Completed);
            worker.RunWorkerAsync();
            Mouse.OverrideCursor = Cursors.AppStarting;
            Application.Current.MainWindow.IsHitTestVisible = false;
        }

        private void Neural_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusText.Text = e.UserState as string;
        }

        private void Neural_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message + "      Inner Exception: " + e.Error.InnerException ?? "");
            }
            StatusText.Text = null;
            Mouse.OverrideCursor = null;
            Application.Current.MainWindow.IsHitTestVisible = true;
        }

        private void CancelNeural_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
            
        }

        public void CreateNetwork_Click(object sender, EventArgs e)
        {
            viewmodel.CreateNetwork();


        }
    }
}
