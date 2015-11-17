using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.RailML.Views;

namespace RailMLNeural.UI.RailML.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class PropertiesViewModel : ViewModelBase
    {
        private PropertiesPresenter _propPresenter;

        public PropertiesPresenter PropPresenter
        {
            get { return _propPresenter;}
            set 
            {
                if(_propPresenter == value)
                {
                    return;
                }
                _propPresenter = value;
                RaisePropertyChanged("PropPresenter");
            }
        }

        private object _selectedElement;

        public dynamic SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                if(_selectedElement == value)
                {
                    return;
                }
                _selectedElement = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the PropertiesViewModel class.
        /// </summary>
        public PropertiesViewModel()
        {
            Messenger.Default.Register<SelectionChangedMessage>(this, (action) => SelectionChanged(action));
        }

        private void SelectionChanged(SelectionChangedMessage msg)
        {
            SelectedElement = msg.SelectedElement;
            PropPresenter = new PropertiesPresenter(_selectedElement, null, true);
        }
    }
}