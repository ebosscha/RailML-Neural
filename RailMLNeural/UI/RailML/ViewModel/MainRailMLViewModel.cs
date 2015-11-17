using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.RailML.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RailMLNeural.UI.RailML.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainRailMLViewModel : ViewModelBase
    {
        private static VisualizationView _visualization;
        private static OCPTableView _ocptable;
        private static TrackTableView _tracktable;
        private static PropertiesView _properties;
        private static NetworkRenderView _networkrender;
        /// <summary>
        /// Initializes a new instance of the MainRailMLViewModel class.
        /// </summary>
        public MainRailMLViewModel()
        {
            Initialize();
            Data.DataContainer.ModelChanged += new EventHandler(Data_ModelChanged);
        }

        private void Initialize()
        {
            Visualization = new VisualizationView();
            OCPTable = new OCPTableView();
            OCPTable.SelectionChanged += new EventHandler<SelectedPropertyChangedEventArgs>(RailML_SelectionChanged);
            TrackTable = new TrackTableView();
            TrackTable.SelectionChanged += new EventHandler<SelectedPropertyChangedEventArgs>(RailML_SelectionChanged);
            Properties = new PropertiesView();
            NetworkRender = new NetworkRenderView();
        }

        public VisualizationView Visualization
        {
            get{return _visualization;}
            set
            {
                if (_visualization == value)
                {
                    return;
                }
                _visualization = value;
                RaisePropertyChanged("Visualization");
            }
        }

        public TrackTableView TrackTable
        {
            get { return _tracktable; }
            set
            {
                if (_tracktable == value)
                {
                    return;
                }
                _tracktable = value;
                RaisePropertyChanged("TrackTable");
            }
        }

        public OCPTableView OCPTable
        {
            get { return _ocptable; }
            set
            {
                if (_ocptable == value)
                {
                    return;
                }
                _ocptable = value;
                RaisePropertyChanged("OCPTable");
            }
        }

        public PropertiesView Properties
        {
            get { return _properties; }
            set
            {
                if (_properties == value)
                {
                    return;
                }
                _properties = value;
                RaisePropertyChanged("Properties");
            }
        }

        public NetworkRenderView NetworkRender
        {
            get {return _networkrender;}
            set{
                if(_networkrender == value){return;}
                _networkrender = value;
                RaisePropertyChanged("NetworkRender");
            }
        }

        private void Data_ModelChanged(object sender, EventArgs e)
        {
           // Initialize();
        }

        private void RailML_SelectionChanged(object sender, SelectedPropertyChangedEventArgs e)
        {
            Messenger.Default.Send<SelectionChangedMessage>(new SelectionChangedMessage(e.SelectedItem));
        }
    }
}