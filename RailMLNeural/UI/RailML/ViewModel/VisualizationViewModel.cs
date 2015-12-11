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
using System.Linq;

namespace RailMLNeural.UI.RailML.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>_NonScalingRenderData
    public class VisualizationViewModel : ViewModelBase
    {
        private ObservableCollection<RenderItemContainer> _nonScalingRenderData;

        public ObservableCollection<RenderItemContainer> NonScalingRenderData
        {
            get { return _nonScalingRenderData; }
            set
            {
                if (_nonScalingRenderData == value) { return; }
                _nonScalingRenderData = value;
                RaisePropertyChanged("NonScalingRenderData");
            }
        }

        private ObservableCollection<RenderItemContainer> _renderData;

        public ObservableCollection<RenderItemContainer> RenderData
        {
            get { return _renderData; }
            set
            {
                if (_renderData == value) { return; }
                _renderData = value;
                RaisePropertyChanged("RenderData");
            }
        }


        

        
        /// <summary>
        /// Initializes a new instance of the VisualizationViewModel class.
        /// </summary>
        public VisualizationViewModel()
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
            RenderData = new ObservableCollection<RenderItemContainer>();
            NonScalingRenderData = new ObservableCollection<RenderItemContainer>();

            
            if (DataContainer.model != null)
            {
                Populate();
            }
        }

        private void Populate()
        {
            foreach(eTrack track in DataContainer.model.infrastructure.tracks)
            {
                RenderData.Add(new RenderItemContainer(track));
                foreach(eSwitch sw in track.trackTopology.connections.Where(x => x is eSwitch))
                {
                    NonScalingRenderData.Add(new RenderItemContainer(sw));
                }
            }
            foreach(eOcp ocp in DataContainer.model.infrastructure.operationControlPoints.Where(x => x.geoCoord.coord.Count == 2))
            {
                NonScalingRenderData.Add(new RenderItemContainer(ocp));
            }
        }


    }

    public class RenderItemContainer
    {
        public dynamic Item { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public string DataType { get; set; }

        public RenderItemContainer(dynamic item)
        {
            Item = item;
            if (item is eTrack)
            {
                eTrack track = Item;
                if (track.trackTopology.trackBegin.geoCoord.coord.Count == 2 && track.trackTopology.trackEnd.geoCoord.coord.Count == 2)
                {
                    DataType = "Track";

                    Top = double.PositiveInfinity;
                    Left = double.PositiveInfinity;
                    Right = double.NegativeInfinity;
                    Bottom = double.NegativeInfinity;
                    foreach (eTrackNode x in new eTrackNode[] { track.trackTopology.trackBegin, track.trackTopology.trackEnd })
                    {
                        Left = Math.Min(Left, x.geoCoord.coord[0]);
                        Top = Math.Min(Top, -x.geoCoord.coord[1]);
                        Right = Math.Max(Right, x.geoCoord.coord[0]);
                        Bottom = Math.Max(Bottom, -x.geoCoord.coord[1]);
                    }
                    foreach (var x in track.trackElements.geoMappings)
                    {
                        Left = Math.Min(Left, x.geoCoord.coord[0]);
                        Top = Math.Min(Top, -x.geoCoord.coord[1]);
                        Right = Math.Max(Right, x.geoCoord.coord[0]);
                        Bottom = Math.Max(Bottom, -x.geoCoord.coord[1]);
                    }
                }
                else item = null;

            }
            if(Item is eOcp)
            {
                eOcp ocp = Item;
                if(ocp.geoCoord.coord.Count == 2)
                {
                    DataType = "OCP";
                    Left = ocp.geoCoord.coord[0];
                    Top = -ocp.geoCoord.coord[1];
                    Right = Left + 10;
                    Bottom = Top + 10;
                }
            }
            if(item is eSwitch)
            {
                eSwitch sw = Item;
                if (sw.geoCoord.coord.Count == 2)
                {
                    DataType = "Switch";
                    Left = sw.geoCoord.coord[0];
                    Top = -sw.geoCoord.coord[1];
                    Right = Left + 10;
                    Bottom = Top + 10;
                }
            }

        }

        public Rect Extent()
        {
            return new Rect(Left, Top, Right - Left, Bottom - Top);

        }
    }
}