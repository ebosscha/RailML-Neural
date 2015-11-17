using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
