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

namespace RailML___WPF.NeuralNetwork.Views
{
    /// <summary>
    /// Interaction logic for BaseNeuralNetworkView.xaml
    /// </summary>
    public partial class BaseNeuralNetworkView : UserControl
    {
        private BaseNeuralNetworkViewModel viewmodel;

        public BaseNeuralNetworkView()
        {
            InitializeComponent();
            this.DataContext = new BaseNeuralNetworkViewModel();
            ParameterContentControl.Content = new ParameterViewModel();

        }


        public void RunPerLine_Click(object sender, EventArgs e)
        {

        }

        public void CreateNetwork_Click(object sender, EventArgs e)
        {
            viewmodel.CreateNetwork();


        }
    }
}
