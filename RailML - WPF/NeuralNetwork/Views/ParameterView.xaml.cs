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

namespace RailML___WPF.NeuralNetwork.Views
{
    /// <summary>
    /// Interaction logic for ParameterView.xaml
    /// </summary>
    public partial class ParameterView : UserControl
    {
        private ParameterViewModel _viewmodel;
        public ParameterView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ParameterView_Loaded);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ParameterView_DataContextChanged);
        }

        private void ParameterView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = this.DataContext as ParameterViewModel;
        }

        private void ParameterView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewmodel = this.DataContext as ParameterViewModel;
        }
    }
}
