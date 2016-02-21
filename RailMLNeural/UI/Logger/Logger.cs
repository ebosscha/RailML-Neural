using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RailMLNeural.UI.Logger
{
    static class Logger
    {
        public static LogViewer View = new LogViewer();
        public static void AddEntry(LogEntry Entry)
        {
            View.AddEntry(Entry);
        }

        public static void AddEntry(string Entry)
        {
            AddEntry(new LogEntry() { DateTime = DateTime.Now, Message = Entry, Index = View.LogEntries.Count });
        }
    }

    public class LogEntry: PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry: LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }

    public class PropertyChangedBase:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action) (() =>
                                                                     {
                                                                         PropertyChangedEventHandler handler = PropertyChanged;
                                                                         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                                                                     }));
        }
    }
}
