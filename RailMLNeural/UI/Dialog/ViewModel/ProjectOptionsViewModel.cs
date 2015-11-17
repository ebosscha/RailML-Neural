using GalaSoft.MvvmLight;
using RailMLNeural.Data;

namespace RailMLNeural.UI.Dialog.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectOptionsViewModel : ViewModelBase
    {
        private Settings _settings;

        public Settings Settings
        {
            get { return _settings; }
            set {
                if (_settings == value) { return; }
                _settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        /// <summary>
        /// Initializes a new instance of the ProjectOptionsViewModel class.
        /// </summary>
        public ProjectOptionsViewModel()
        {
            Settings = DataContainer.Settings;
        }
    }
}