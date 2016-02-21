using System.Windows;
using RailMLNeural.UI.ViewModel;
using System;

namespace RailMLNeural.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}