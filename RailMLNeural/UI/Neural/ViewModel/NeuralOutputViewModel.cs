﻿using Encog.ML.Data;
using Encog.ML.Data.Basic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using System.Collections.ObjectModel;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NeuralOutputViewModel : ViewModelBase
    {
        #region Parameters
        private ObservableCollection<IOdef> _outputCollection;

        public ObservableCollection<IOdef> OutputCollection
        {
            get
            { return _outputCollection; }
            set
            {
                _outputCollection = value;
                RaisePropertyChanged("OutputCollection");
            }
        }

        private ObservableCollection<IOdef> _inputCollection;

        public ObservableCollection<IOdef> InputCollection
        {
            get
            { return _inputCollection; }
            set
            {
                _inputCollection = value;
                RaisePropertyChanged("InputCollection");
            }
        }

        private NeuralNetwork _selectedNetwork;

        #endregion Parameters

        /// <summary>
        /// Initializes a new instance of the NeuralOutputViewModel class.
        /// </summary>
        public NeuralOutputViewModel()
        {
            InputCollection = new ObservableCollection<IOdef>();
            OutputCollection = new ObservableCollection<IOdef>();
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (msg) => SelectionChanged(msg));
        }

        #region Private
        private void SelectionChanged(NeuralSelectionChangedMessage msg)
        {
            _selectedNetwork = msg.NeuralNetwork;
            UpdateInput();
            UpdateOutput();
        }

        private void UpdateInput()
        {
            InputCollection = new ObservableCollection<IOdef>();
            if (_selectedNetwork != null)
            {
                foreach (string map in _selectedNetwork.InputMap)
                {
                    InputCollection.Add(new IOdef { Label = map, Value = 0 });
                }
            }
        }

        public void UpdateOutput()
        {
            OutputCollection = new ObservableCollection<IOdef>();
            if(_selectedNetwork != null)
            {
                BasicMLData data = new BasicMLData(_selectedNetwork.Data.InputSize);
                for(int i = 0; i < InputCollection.Count; i++)
                {
                    data[i] = InputCollection[i].Value;
                }
                data = _selectedNetwork.Data.Normalizer.Normalize(data, true) as BasicMLData;
                var test = _selectedNetwork.Data.Normalizer.DeNormalize(data, true) as BasicMLData;
                IMLData output = _selectedNetwork.Compute(data);
                output = _selectedNetwork.Data.Normalizer.DeNormalize(output, false);
                for(int i = 0; i<output.Count ; i++)
                {
                    IOdef def = new IOdef { Value = output[i] };
                    if(i < _selectedNetwork.OutputMap.Count)
                    {
                        def.Label = _selectedNetwork.OutputMap[i];
                    }
                    OutputCollection.Add(def);
                }
            }

        }
        #endregion Private
    }

    public class IOdef
    {
        public string Label { get; set; }
        public double Value { get; set; }
        
    }
}