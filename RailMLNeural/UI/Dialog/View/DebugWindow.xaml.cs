using RailMLNeural.UI.Dialog.ViewModel;
using System.Windows;

namespace RailMLNeural.UI.Dialog.View
{
    /// <summary>
    /// Description for DebugWindow.
    /// </summary>
    public partial class DebugWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the DebugWindow class.
        /// </summary>
        public DebugWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DebugViewModel vm = (DebugViewModel)DataContext;
            vm.Start();
        }
    }
}