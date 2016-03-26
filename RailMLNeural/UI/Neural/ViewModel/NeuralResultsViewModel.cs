using DynamicDataDisplay.Markers.DataSources;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using RailMLNeural.Data;
using RailMLNeural.Neural;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NeuralResultsViewModel : ViewModelBase
    {
        private ObservableDataSource<Point> _errorHistory { get; set; }
        public ObservableDataSource<Point> ErrorHistory
        {
            get { return _errorHistory; }
            set { _errorHistory = value;
            RaisePropertyChanged("ErrorHistory");
            }
        }

        private ObservableDataSource<Point> _verificationHistory { get; set; }
        public ObservableDataSource<Point> VerificationHistory
        {
            get { return _verificationHistory; }
            set
            {
                _verificationHistory = value;
                RaisePropertyChanged("VerificationHistory");
            }
        }


        private INeuralConfiguration _selectedNetwork;

        public INeuralConfiguration SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                if (_selectedNetwork == value) { return; }
                _selectedNetwork = value;
                RaisePropertyChanged("SelectedNetwork");
            }
        }

        public NeuralResultsViewModel()
        {
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (action) => ChangeSelection(action));
            InitializeCommands();
        }

        private void ChangeSelection(NeuralSelectionChangedMessage msg)
        {
            if(SelectedNetwork != null)
            {
                SelectedNetwork.ProgressChanged -= new EventHandler(ProgressChanged);
            }
            if (msg.NeuralNetwork != null)
            {
                SelectedNetwork = msg.NeuralNetwork;
                SelectedNetwork.ProgressChanged += new EventHandler(ProgressChanged);
                ProgressChanged(this, EventArgs.Empty);
            }
        }

        private void ProgressChanged(object sender, EventArgs e)
        {
            if(ErrorHistory == null || ErrorHistory.Collection.Count != SelectedNetwork.ErrorHistory.Count - 1)
            {
                ErrorHistory = new ObservableDataSource<Point>();
                VerificationHistory = new ObservableDataSource<Point>();
            }
            for(int i = ErrorHistory.Collection.Count; i < SelectedNetwork.ErrorHistory.Count; i++)
            {
                ErrorHistory.Collection.Add(new Point(i + 1, SelectedNetwork.ErrorHistory[i]));
            }
            ErrorHistory.SetXYMapping(p => p);

            for (int i = VerificationHistory.Collection.Count; i < SelectedNetwork.VerificationHistory.Count; i++)
            {
                VerificationHistory.Collection.Add(new Point(i + 1, SelectedNetwork.VerificationHistory[i]));
            }
            VerificationHistory.SetXYMapping(p => p);
        }

        private void WriteErrorHistory(string filename)
        {
            File.Delete(filename);
            var stream = File.CreateText(filename);
            stream.WriteLine("Epoch Number \t Error");
            for(int i = 0; i < SelectedNetwork.ErrorHistory.Count; i++)
            {
                stream.WriteLine(i + "\t" + SelectedNetwork.ErrorHistory[i]);
            }
        }

        #region Commands
        public ICommand WriteOutputCommand { get; private set; }
        public ICommand RunVerificationCommand { get; private set; }

        public void InitializeCommands()
        {
            WriteOutputCommand = new RelayCommand(ExecuteWriteOutput);
            RunVerificationCommand = new RelayCommand(ExecuteRunVerification, canExecuteRunVerification);
        }

        private void ExecuteWriteOutput()
        {
             Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.AddExtension = true;
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Documents (.txt.)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                WriteErrorHistory(filename);
            }
        }

        private void ExecuteRunVerification()
        {
            _selectedNetwork.RunVerification();
        }

        private bool canExecuteRunVerification()
        {
            return _selectedNetwork == null ? false : !_selectedNetwork.IsRunning;
        }

        #endregion Commands
    }
}