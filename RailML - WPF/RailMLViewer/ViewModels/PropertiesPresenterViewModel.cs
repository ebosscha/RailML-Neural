﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.RailMLViewer.ViewModels
{
    class PropertiesPresenterViewModel : BaseViewModel
    {
        private dynamic _element;
        public dynamic element 
        {
            get { return _element; }
            set { _element = value; }
        }
    }
}
