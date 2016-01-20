using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using RailMLNeural.Neural;

namespace RailMLNeural.UI.Neural.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NeuralPropertiesViewModel : ViewModelBase
    {
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
        /// <summary>
        /// Initializes a new instance of the NeuralPropertiesViewModel class.
        /// </summary>
        public NeuralPropertiesViewModel()
        {
            Messenger.Default.Register<NeuralSelectionChangedMessage>(this, (action) => ChangeSelection(action));
        }

        private void ChangeSelection(NeuralSelectionChangedMessage msg)
        {
            SelectedNetwork = msg.NeuralNetwork;
        }
    
    }
}