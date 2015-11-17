using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.UI.RailML.ViewModel
{
    public class SelectedPropertyChangedEventArgs
    {
        public bool IsSingleItemSelected { get; set; }
        public dynamic SelectedItem { get; set; }

        public SelectedPropertyChangedEventArgs(bool IsSingleItem, dynamic item)
        {
            IsSingleItemSelected = IsSingleItem;
            SelectedItem = item;
        }
    }
}
