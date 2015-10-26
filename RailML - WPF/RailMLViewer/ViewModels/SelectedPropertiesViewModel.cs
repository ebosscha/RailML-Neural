using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailML___WPF.Data;
using System.Windows.Media;

namespace RailML___WPF.RailMLViewer.ViewModels
{
    public class SelectedPropertiesViewModel : BaseViewModel
    {
        public dynamic selectedobject {get; set;}
        public ObservableCollection<Property> propertylist {get; set;} 

        public SelectedPropertiesViewModel(dynamic input)
        {
            propertylist = new ObservableCollection<Property>();
            selectedobject = input;

            try
            {
                TrackProperties(input);
                return;
            }
            catch{}

            try
            {
                OCPproperties(input);
                return;
            }
            catch{}
             
        }

        public void TrackProperties(eTrack track)
        {
            propertylist.Add(new Property { label = "Name", source=track,path = "source.name"} );
            propertylist.Add(new Property { label = "ID", source=track, path = "source.id"});
            propertylist.Add(new Property { label = "TrackBegin", source=track, path = "source.trackTopology.trackBegin.pos" } );
            propertylist.Add(new Property { label = "TrackEnd", source = track, path = "source.trackTopology.trackEnd.pos" });
        }

        public void OCPproperties(eOcp ocp)
        {
            propertylist.Add(new Property { label = "Name", source = ocp, path = "source.name" });
            propertylist.Add(new Property { label = "ID", source = ocp, path = "source.id" });
            propertylist.Add(new Property { label = "Description", source = ocp, path = "source.description" });
        }
    }

    public class Property
    {

        public dynamic source { get; set; }
        public string label { get; set; }
        public string path { get; set; }
        

    }
}
