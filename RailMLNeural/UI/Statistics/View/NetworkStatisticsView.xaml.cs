using RailMLNeural.UI.Statistics.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.Statistics.View
{
    /// <summary>
    /// Description for NetworkStatisticsView.
    /// </summary>
    public partial class NetworkStatisticsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the NetworkStatisticsView class.
        /// </summary>
        public NetworkStatisticsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            NetworkStatisticsViewModel vm = (NetworkStatisticsViewModel)DataContext;
            vm.Loaded();
        }

    }
}