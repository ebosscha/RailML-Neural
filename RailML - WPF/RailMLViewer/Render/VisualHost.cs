using RailML___WPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RailML___WPF.RailMLViewer.Render
{
    class VisualHost : FrameworkElement
    {
        public VisualHost() 
        {
            foreach(eTrack track in DataContainer.model.infrastructure.tracks.Where(e => e.trackTopology.trackBegin.geoCoord.coord.Count == 2 && e.trackTopology.trackEnd.geoCoord.coord.Count == 2))
            {

            }
        }
    }
}
