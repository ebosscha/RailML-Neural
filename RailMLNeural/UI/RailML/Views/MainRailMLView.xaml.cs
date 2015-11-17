using RailMLNeural.UI.RailML.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for MainRailMLView.
    /// </summary>
    public partial class MainRailMLView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MainRailMLView class.
        /// </summary>
        public MainRailMLView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(this_Loaded);
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            MainRailMLViewModel vm = DataContext as MainRailMLViewModel;
            int i = 1;
        }
    }
}