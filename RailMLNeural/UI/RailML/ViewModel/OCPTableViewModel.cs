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
    public class OCPTableViewModel : ViewModelBase
    {
        private ObservableCollection<eOcp> _OCPs;
        /// <summary>
        /// Initializes a new instance of the TrackTableViewModel class.
        /// </summary>
        public OCPTableViewModel()
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
                OCPs = new ObservableCollection<eOcp>(DataContainer.model.infrastructure.operationControlPoints);
            }
            else
            {
                OCPs = new ObservableCollection<eOcp>();
            }
        }
        public ObservableCollection<eOcp> OCPs
        {
            get { return _OCPs; }
            set
            {
                if (_OCPs == value)
                { return; }
                _OCPs = value;
                RaisePropertyChanged("OCPs");
            }
        }
    }
}