using RailMLNeural.Neural;
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
using System.Windows.Shapes;

namespace RailMLNeural.UI.Dialog.View
{
    /// <summary>
    /// Interaction logic for NeuralCreationDialog.xaml
    /// </summary>
    public partial class NeuralCreationDialog : Window
    {
        public AlgorithmEnum AlgorithmType { get; private set; }
        public NeuralCreationDialog()
        {
            InitializeComponent();
        }

        private void GraphRecurrent_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            AlgorithmType = AlgorithmEnum.GraphRecurrent;
            this.Close();
        }

        private void Normal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            AlgorithmType = AlgorithmEnum.FeedForward;
            this.Close();
        }

        private void Recursive_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            AlgorithmType = AlgorithmEnum.Recursive;
            this.Close();
        }

        private void NEAT_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            AlgorithmType = AlgorithmEnum.NEAT;
            this.Close();
        }

        private void LSTM_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            AlgorithmType = AlgorithmEnum.LSTM;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
