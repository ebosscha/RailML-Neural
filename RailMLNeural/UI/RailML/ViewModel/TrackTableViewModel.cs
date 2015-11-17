using GalaSoft.MvvmLight;
using RailMLNeural.Data;
using RailMLNeural.RailML;
using System;
using System.Collections.ObjectModel;

namespace RailMLNeural.UI.RailML.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TrackTableViewModel : ViewModelBase
    {
        private ObservableCollection<eTrack> _tracks;
        /// <summary>
        /// Initializes a new instance of the TrackTableViewModel class.
        /// </summary>
        public TrackTableViewModel()
        {
            Initialize();
            DataContainer.ModelChanged += new EventHandler(Data_ModelChanged);
        }

        private void Data_ModelChanged(object sender, EventArgs e)
        {
            Initialize();
        }
    
        private void Initialize()
        {
            if (DataContainer.model != null)
            {
                Tracks = new ObservableCollection<eTrack>(DataContainer.model.infrastructure.tracks);
            }
            else
            {
                Tracks = new ObservableCollection<eTrack>();
            }
        }
        public ObservableCollection<eTrack> Tracks
        {
            get { return _tracks; }
            set
            {
                if (_tracks == value)
                { return; }
                _tracks = value;
                RaisePropertyChanged("Tracks");
            }
        }



    }
}