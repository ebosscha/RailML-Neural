using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.RailML;
using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.Render
{
    public class RenderTrackLine2 : Shape
    {
        public RenderTrackLine2()
        {
            this.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Track_MouseLeftButtonDown);
            this.Cursor = Cursors.Hand;
        }

        #region DependencyProperties
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register("Track", typeof(eTrack), typeof(RenderTrackLine), new FrameworkPropertyMetadata(new eTrack(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(TrackProperty_Changed)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RenderTrackLine), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ScaleProperty_Changed)));
        #endregion DependencyProperties

        #region CLS Properties
        public eTrack Track
        {
            get { return (eTrack)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        #endregion CLS Properties

        #region Privates

        public PathGeometry line { get; set; }

        protected override Geometry DefiningGeometry
        {
            get
            {
                return line;
            }
        }

        private PathGeometry GetGeometry(eTrack track)
        {
            if (track == null || track.trackTopology.trackBegin.geoCoord.coord.Count != 2 || track.trackTopology.trackEnd.geoCoord.coord.Count != 2)
            {
                return null;
            }
            PathGeometry geom = new PathGeometry();
            PathFigure figure = new PathFigure();
            PointCollection coll = new PointCollection();
            coll.Add(new Point(track.trackTopology.trackBegin.geoCoord.coord[0] * Scale, track.trackTopology.trackBegin.geoCoord.coord[1] * Scale));
            foreach (var p in track.trackElements.geoMappings)
            {
                coll.Add(new Point(p.geoCoord.coord[0] * Scale, p.geoCoord.coord[1] * Scale));
            }
            coll.Add(new Point(track.trackTopology.trackEnd.geoCoord.coord[0] * Scale, track.trackTopology.trackEnd.geoCoord.coord[1] * Scale));

            figure.Segments.Add(new PolyLineSegment(coll, true));
            figure.StartPoint = coll[0];
            geom.Figures.Add(figure);

            return geom;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (DefiningGeometry != null)
            {
                Rect bounds = DefiningGeometry.Bounds;
                return new Size(bounds.Width, bounds.Height);
            }
            return new Size(0, 0);
        }

        private static void TrackProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderTrackLine2 obj = (RenderTrackLine2)d;
            obj.line = obj.GetGeometry((eTrack)e.NewValue);
        }

        private static void ScaleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderTrackLine obj = (RenderTrackLine)d;
            double translatescale = (double)e.NewValue / (double)e.OldValue;
            foreach (var a in obj.line.Figures)
            {
                a.StartPoint = new Point(a.StartPoint.X * translatescale, a.StartPoint.Y * translatescale);
                foreach (PolyLineSegment b in a.Segments)
                {
                    PointCollection coll = new PointCollection();
                    for (int i = 0; i < b.Points.Count; i++)
                    {
                        Point p = b.Points[i];
                        coll.Add(new Point(p.X * translatescale, p.Y * translatescale));

                    }
                    b.Points = coll;
                }
            }
        }


        #endregion Privates

        #region SelectionHandler
        private void Track_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.Stroke = Brushes.Red;
            Messenger.Default.Send(new SelectionChangedMessage(Track));
        }

        #endregion SelectionHandler
    }


    public class RenderTrackLine : Shape, INotifyPropertyChanged
    {
        public RenderTrackLine()
        {
            this.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Track_MouseLeftButtonDown);
            this.Cursor = Cursors.Hand;
        }
        
#region DependencyProperties
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register("Track", typeof(eTrack), typeof(RenderTrackLine), new FrameworkPropertyMetadata(new eTrack(),FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(TrackProperty_Changed)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RenderTrackLine), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ScaleProperty_Changed)));
#endregion DependencyProperties

#region CLS Properties
        public eTrack Track
        {
            get { return (eTrack)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public double Scale
        {
            get {return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public double Top
        {
            get;
            set;

        }

        public double Left
        {
            get;
            set;
        }


#endregion CLS Properties

        #region Privates

        public PathGeometry line { get; set; }
        public PathGeometry correctedline { get; set; }

        protected override Geometry DefiningGeometry
        {
            get
            {
                return correctedline;
            }
        }

        private PathGeometry GetGeometry(eTrack track)
        {
            Top = double.PositiveInfinity;
            Left = double.PositiveInfinity;
            if(track == null || track.trackTopology.trackBegin.geoCoord.coord.Count != 2 || track.trackTopology.trackEnd.geoCoord.coord.Count != 2 )
            {
                return null;
            }
            PathGeometry geom = new PathGeometry();
            PathFigure figure = new PathFigure();
            PointCollection coll = new PointCollection();
            coll.Add(new Point(track.trackTopology.trackBegin.geoCoord.coord[0], -track.trackTopology.trackBegin.geoCoord.coord[1]));
            foreach(var p in track.trackElements.geoMappings)
            {
                coll.Add(new Point(p.geoCoord.coord[0], -p.geoCoord.coord[1]));
            }
            coll.Add(new Point(track.trackTopology.trackEnd.geoCoord.coord[0], -track.trackTopology.trackEnd.geoCoord.coord[1]));
            foreach(Point p in coll)
            {
                Left = Math.Min(Left, p.X);
                Top = Math.Min(Top, p.Y);
            }
            figure.Segments.Add(new PolyLineSegment(coll, true));
            figure.StartPoint = coll[0];
            geom.Figures.Add(figure);
            

            return geom;
        }


        protected override Size MeasureOverride(Size constraint)
        {
            if (DefiningGeometry != null)
            {
                Rect bounds = DefiningGeometry.Bounds;
                return new Size(bounds.Width, bounds.Height);
            }
            return new Size(0, 0);
        }


        private static void TrackProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderTrackLine obj = (RenderTrackLine)d;
            obj.line = obj.GetGeometry((eTrack)e.NewValue);
            obj.correctedline = obj.line;
            obj.correctedline.Transform = new TranslateTransform(-obj.Left, -obj.Top);
            obj.RaisePropertyChanged("Top");
            obj.RaisePropertyChanged("Left");
            obj.InvalidateVisual();
        }

        private static void ScaleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderTrackLine obj = (RenderTrackLine)d;
            obj.StrokeThickness /= (double)e.NewValue/(double)e.OldValue;
        }


        #endregion Privates

        #region SelectionHandler
        private void Track_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.Stroke = Brushes.Red;
            Messenger.Default.Send(new SelectionChangedMessage(Track));
        }

        #endregion SelectionHandler

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion PropertyChanged Implementation



    }
}
