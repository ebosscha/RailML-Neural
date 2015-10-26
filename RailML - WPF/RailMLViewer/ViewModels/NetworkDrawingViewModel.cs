using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using RailML___WPF.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows;

namespace RailML___WPF.RailMLViewer.ViewModels
{
    class NetworkDrawingViewModel : BaseViewModel
    {
    
        public CompositeCollection rendercoll {get; set;}

        public ObservableCollection<Track> tracklines { get; set; }
        public ObservableCollection<OCP> OCPcollection {get; set; }
        public ObservableCollection<Switch> switchcollection { get; set; }
        public double penscale { get; set; }
        public NetworkDrawingViewModel()
        {
            rendercoll = new CompositeCollection();
            tracklines = new ObservableCollection<Track>();
            OCPcollection = new ObservableCollection<OCP>();
            switchcollection = new ObservableCollection<Switch>();
            penscale = 1;
            for(int i = 0; i < DataContainer.model.infrastructure.tracks.Count; i++)
            {
                Track temptrack = new Track();
                eTrack track = DataContainer.model.infrastructure.tracks[i];
                if (track.trackTopology.trackEnd.geoCoord.coord.Count != 0 && track.trackTopology.trackBegin.geoCoord.coord.Count != 0)
                {
                    
                    temptrack.track = track;
                    temptrack.index = i;
                    temptrack.X1 = track.trackTopology.trackBegin.geoCoord.coord[0];
                    temptrack.X2 = track.trackTopology.trackEnd.geoCoord.coord[0];
                    temptrack.Y1 = track.trackTopology.trackBegin.geoCoord.coord[1];
                    temptrack.Y2 = track.trackTopology.trackEnd.geoCoord.coord[1];

                    
                    temptrack.points.Add(new Point(track.trackTopology.trackBegin.geoCoord.coord[0],track.trackTopology.trackBegin.geoCoord.coord[1]));

                    foreach(tPlacedElement point in track.trackElements.geoMappings)
                    {
                        temptrack.points.Add(new Point(point.geoCoord.coord[0], point.geoCoord.coord[1]));
                    }
                    temptrack.points.Add(new Point(track.trackTopology.trackEnd.geoCoord.coord[0], track.trackTopology.trackEnd.geoCoord.coord[1]));


                    tracklines.Add(temptrack);

                    foreach(tCommonSwitchAndCrossingData connection in track.trackTopology.connections)
                    {
                        if (connection.geoCoord.coord.Count == 2)
                        {
                            Switch sw = new Switch();
                            sw.element = connection;
                            sw.X = connection.geoCoord.coord[0];
                            sw.Y = connection.geoCoord.coord[1];
                            switchcollection.Add(sw);
                        }
                    }




                }
                    
            }

            foreach(eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                OCP tempocp = new OCP();
                tempocp.ocp = ocp;
                if (ocp.geoCoord.coord.Count == 2)
                {
                    tempocp.X = ocp.geoCoord.coord[0] - tempocp.diameter / 2;
                    tempocp.Y = ocp.geoCoord.coord[1] - tempocp.diameter / 2;
                    OCPcollection.Add(tempocp);
                }
            }
            rendercoll.Add(new CollectionContainer(){Collection = tracklines});
            rendercoll.Add(new CollectionContainer() { Collection = OCPcollection });
            rendercoll.Add(new CollectionContainer() {Collection = switchcollection});
        }

       
    }

    class Track
    {
        public eTrack track { get; set; }
        public int index { get; set; }
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public PointCollection points { get; set; }
        public double thickness { get; set; }

        public Track()
        {
            thickness = 1;
            points = new PointCollection();
        }

    }

    public class OCP
    {
        public eOcp ocp {get; set;}
        public double diameter {get; set;}
        public double X {get;set;}
        public double Y {get;set;}
        
        public OCP()
        {
            diameter = 10;
        }
    }

    public class Switch
    {
        public double X { get; set; }
        public double Y { get; set; }
        public tCommonSwitchAndCrossingData element {get; set; }
    }

}
