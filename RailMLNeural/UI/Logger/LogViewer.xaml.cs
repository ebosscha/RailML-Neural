using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RailMLNeural.UI.Logger
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : Window
    {

        private int index;

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public LogViewer()
        {
            InitializeComponent();

            DataContext = LogEntries = new ObservableCollection<LogEntry>();

        }

        private System.Threading.Timer Timer;
        private System.Random random;
        public void AddEntry(LogEntry Entry)
        {
            Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(Entry)));
        }

    }
}
