using GalaSoft.MvvmLight;
using RailMLNeural.UI.Neural.Views;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainNeuralViewModel : ViewModelBase
    {
        private static NeuralCollectionView _neuralcollection = new NeuralCollectionView();
        private static NeuralPropertiesView _neuralproperties = new NeuralPropertiesView();
        private static NeuralRunManagerView _neuralrunmanager = new NeuralRunManagerView();
        private static NeuralResultsView _neuralresults = new NeuralResultsView();
        private static NeuralOutputView _neuraloutput = new NeuralOutputView();

        public NeuralCollectionView NeuralCollection
        {
            get { return _neuralcollection; }
            set {
                if (_neuralcollection == value) { return; }
                _neuralcollection = value;
                RaisePropertyChanged("NeuralCollection");
            }
        }

        public NeuralPropertiesView NeuralProperties
        {
            get { return _neuralproperties; }
            set
            {
                if (_neuralproperties == value) { return; }
                _neuralproperties = value;
                RaisePropertyChanged("NeuralProperties");
            }
        }

        public NeuralRunManagerView NeuralRunManager
        {
            get { return _neuralrunmanager; }
            set
            {
                if (_neuralrunmanager == value) { return; }
                _neuralrunmanager = value;
                RaisePropertyChanged("NeuralRunManager");
            }
        }

        public NeuralResultsView NeuralResults
        {
            get { return _neuralresults; }
            set
            {
                if (_neuralresults == value) { return; }
                _neuralresults = value;
                RaisePropertyChanged("NeuralResults");
            }
        }

        public NeuralOutputView NeuralOutput
        {
            get { return _neuraloutput; }
            set
            {
                if (_neuraloutput == value) { return; }
                _neuraloutput = value;
                RaisePropertyChanged("NeuralOutput");
            }
        }
        
        
        /// <summary>
        /// Initializes a new instance of the MainNeuralViewModel class.
        /// </summary>
        public MainNeuralViewModel()
        {
           
        
        }
    }

    
}