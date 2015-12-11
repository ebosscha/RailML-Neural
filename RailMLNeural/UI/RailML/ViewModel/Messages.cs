using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RailMLNeural.UI.RailML.ViewModel
{
    class SelectionChangedMessage
    {
        public dynamic SelectedElement { get; set; }
        
        public SelectionChangedMessage(dynamic item)
        {
            SelectedElement = item;
        }

    }

    class ZoomToObjectMessage
    {
        public List<dynamic> Elements { get; set; }
        
        public ZoomToObjectMessage(List<dynamic> items)
        {
            Elements = items;
        }
    }

    class HighlightElementMessage
    {
        public dynamic Element { get; set; }
        public Brush Color { get; set; }

        public HighlightElementMessage(dynamic item, Brush color)
        {
            Element = item;
            Color = color;
        }
    }

    class ClearHighlightMessage
    {

    }
}
