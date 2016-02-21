using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace RailMLNeural.UI.Neural.OutputVisualization.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GraphPropertiesViewModel : ViewModelBase
    {
        private dynamic _selectedItem;

        public dynamic SelectedItem { 
            get { return _selectedItem; } 
            set { _selectedItem = value; RaisePropertyChanged("SelectedItem"); } 
        }
        /// <summary>
        /// Initializes a new instance of the GraphPropertiesViewModel class.
        /// </summary>
        public GraphPropertiesViewModel()
        {
            Messenger.Default.Register<GraphSelectionChangedMessage>(this, (msg) => SelectionChanged(msg));
        }

        public void Dispose()
        {
            Messenger.Default.Unregister(this);
        }

        private void SelectionChanged(GraphSelectionChangedMessage msg)
        {
            SelectedItem = msg.Item;
        }
    }
}