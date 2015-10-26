using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailML___WPF.Data;

namespace RailML___WPF.RailMLViewer.ViewModels
{
    class TracksViewModel : BaseViewModel
    {
        public dynamic selecteditem { get; set; }

        public ObservableCollection<eTrack> tracklist { get; set; }

        public TracksViewModel()
        {
            tracklist = new ObservableCollection<eTrack>(DataContainer.model.infrastructure.tracks);
        }


    }
    
}

