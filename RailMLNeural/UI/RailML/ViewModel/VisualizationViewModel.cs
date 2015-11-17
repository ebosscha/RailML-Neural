using GalaSoft.MvvmLight;
using RailMLNeural.Data;
using RailMLNeural.RailML;
using RailMLNeural.UI.RailML.Render;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class VisualizationViewModel : ViewModelBase
    {
        private CompositeCollection _renderData;

        public CompositeCollection RenderData
        {
            get { return _renderData; }
            set
            {
                if (_renderData == value) { return; }
                _renderData = value;
                RaisePropertyChanged("RenderData");
            }
        }

        private ObservableCollection<eTrack> _renderTracks;

        private ObservableCollection<eOcp> _renderOCP;

        
        /// <summary>
        /// Initializes a new instance of the VisualizationViewModel class.
        /// </summary>
        public VisualizationViewModel()
        {
            RenderData = new CompositeCollection();
            Initialize();
            DataContainer.ModelChanged += new EventHandler(Data_ModelChanged);
        }

        private void Data_ModelChanged(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            RenderData = new CompositeCollection();
            
            if (DataContainer.model != null)
            {
                _renderOCP = new ObservableCollection<eOcp>(DataContainer.model.infrastructure.operationControlPoints);
                _renderTracks = new ObservableCollection<eTrack>(DataContainer.model.infrastructure.tracks);
                RenderData.Add(new CollectionContainer() { Collection = _renderOCP });
                RenderData.Add(new CollectionContainer() { Collection = _renderTracks });
            }
        }
    }
}