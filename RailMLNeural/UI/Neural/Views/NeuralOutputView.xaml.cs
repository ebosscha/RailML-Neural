using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for NeuralOutputView.
    /// </summary>
    public partial class NeuralOutputView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the NeuralOutputView class.
        /// </summary>
        public NeuralOutputView()
        {
            InitializeComponent();
        }

        private void DataGridControl_EditEnded(object sender, RoutedEventArgs e)
        {
            ((NeuralOutputViewModel)DataContext).UpdateOutput();
            e.Handled = true;
        }
    }
}