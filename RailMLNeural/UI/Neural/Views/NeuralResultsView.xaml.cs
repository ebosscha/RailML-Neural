using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for NeuralResultsView.
    /// </summary>
    public partial class NeuralResultsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the NeuralResultsView class.
        /// </summary>
        public NeuralResultsView()
        {
            InitializeComponent();
        }

        private void ChartPlotter_Loaded(object sender, RoutedEventArgs e)
        {
            ChartPlotter chart = sender as ChartPlotter;
            chart.DataTransform = new Log10YTransform();
            VerticalAxis axis = new VerticalAxis();
            axis.TicksProvider = new LogarithmNumericTicksProvider(10);
            chart.MainVerticalAxis = axis;
        }
    }
}