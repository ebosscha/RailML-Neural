using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
        private ObservableCollection<double> _errorHistory { get; set; }
        public ObservableCollection<double> ErrorHistory
        {
            get { return _errorHistory; }
            set { _errorHistory = value;
            RaisePropertyChanged("ErrorHistory");
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
            ErrorHistory = new ObservableCollection<double>();
        }

        private void ChangeSelection(NeuralSelectionChangedMessage msg)
        {
            if(SelectedNetwork != null)
            {
                SelectedNetwork.ProgressChanged -= new EventHandler(ProgressChanged);
            }
            
            SelectedNetwork = msg.NeuralNetwork;
            SelectedNetwork.ProgressChanged += new EventHandler(ProgressChanged);
            
        }

        private void ProgressChanged(object sender, EventArgs e)
        {
            ErrorHistory = new ObservableCollection<double>(SelectedNetwork.ErrorHistory);
        }
    }
}