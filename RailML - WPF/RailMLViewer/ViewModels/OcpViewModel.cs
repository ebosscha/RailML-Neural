using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailML___WPF.Data;
using System.Collections.ObjectModel;

namespace RailML___WPF.RailMLViewer.ViewModels
{
    class OcpViewModel : BaseViewModel
    {
        public eOcp selecteditem { get; set; }
        public ObservableCollection<eOcp> ocplist { get; set; }

        public OcpViewModel()
        {
            ocplist = new ObservableCollection<eOcp>(DataContainer.model.infrastructure.operationControlPoints);
        }
    }
}
