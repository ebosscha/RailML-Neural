using DynamicDataDisplay.Markers.DataSources;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using RailMLNeural.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

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


        private NeuralNetwork _selectedNetwork;

        public NeuralNetwork SelectedNetwork
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
            ErrorHistory = new ObservableDataSource<Point>();
            for(int i = 0; i < SelectedNetwork.ErrorHistory.Count; i++)
            {
                ErrorHistory.Collection.Add(new Point(i + 1, SelectedNetwork.ErrorHistory[i]));
            }
            ErrorHistory.SetXYMapping(p => p);
            VerificationHistory = new ObservableDataSource<Point>();
            for (int i = 0; i < SelectedNetwork.VerificationSetHistory.Count; i++)
            {
                VerificationHistory.Collection.Add(new Point(i + 1, SelectedNetwork.VerificationSetHistory[i]));
            }
            VerificationHistory.SetXYMapping(p => p);
        }
    }
}