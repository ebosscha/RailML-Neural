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

        private ObservableCollection<TrackWithExtents> _renderTracks;

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
                CreateTracksCollection();
                RenderData.Add(new CollectionContainer() { Collection = _renderOCP });
                RenderData.Add(new CollectionContainer() { Collection = _renderTracks });
            }
        }

        private void CreateTracksCollection()
        {
            _renderTracks = new ObservableCollection<TrackWithExtents>();
            foreach(eTrack track in DataContainer.model.infrastructure.tracks)
            {
                _renderTracks.Add(new TrackWithExtents(track));
            }

        }

    }

    public class TrackWithExtents
    {
        public eTrack track { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }

        public TrackWithExtents(eTrack t)
        {
            track = t;
            if(track.trackTopology.trackBegin.geoCoord.coord.Count == 2 && track.trackTopology.trackEnd.geoCoord.coord.Count == 2 )
            {
                Top = double.NegativeInfinity;
                Left = double.PositiveInfinity;
                foreach(eTrackNode x in new eTrackNode[]{track.trackTopology.trackBegin, track.trackTopology.trackEnd})
                {
                    Left = Math.Min(Left, x.geoCoord.coord[0]);
                    Top = Math.Max(Top, x.geoCoord.coord[1]);
                }
                foreach(var x in track.trackElements.geoMappings)
                {
                    Left = Math.Min(Left, x.geoCoord.coord[0]);
                    Top = Math.Max(Top, x.geoCoord.coord[1]);
                }
                Top = -Top;

            }
        }
    }
}