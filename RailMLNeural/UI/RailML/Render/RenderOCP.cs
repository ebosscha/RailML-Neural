using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.RailML;
using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.Render
{
    public class RenderOCP : Shape
    {
        #region DependencyProperties
        public static readonly DependencyProperty OCPProperty = DependencyProperty.Register("OCP", typeof(eOcp), typeof(RenderOCP),new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OCPProperty_Changed)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RenderOCP), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ScaleProperty_Changed)));
        #endregion DependencyProperties

        #region Properties
        public eOcp OCP
        {
            get { return (eOcp)GetValue(OCPProperty); }
            set { SetValue(OCPProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        private Geometry geometry {get; set;}

        #endregion Properties

        #region Initialization
        public RenderOCP()
        {
            this.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(OCP_MouseLeftButtonDown);
            this.Cursor = Cursors.Hand;
            Messenger.Default.Register<SelectionChangedMessage>(this, action => Selection_Changed(action));
        }
        #endregion Initialization

        #region Overrides
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return new System.Windows.Size(geometry.Bounds.Width, geometry.Bounds.Height);
        }

        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get { return geometry; }
        }
        #endregion Overrides

        #region Privates
        private static void OCPProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderOCP obj = (RenderOCP)d;
            obj.CalculateGeometry();
        }

        private static void ScaleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderOCP obj = (RenderOCP)d;
            obj.CalculateGeometry();
        }

        private void CalculateGeometry()
        {
            if (OCP.geoCoord.coord.Count == 2)
            {
                geometry = new RectangleGeometry(new Rect(0, 0, Width / Scale, Height / Scale));
                this.Margin = new Thickness(-(Width / Scale) / 2, -(Height / Scale) / 2, 0, 0);
                InvalidateVisual();
            }
        }
        #endregion Privates

        #region SelectionHandler
        private void OCP_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.Stroke = Brushes.Red;
            Messenger.Default.Send(new SelectionChangedMessage(OCP));
        }

        private void Selection_Changed(SelectionChangedMessage msg)
        {
            if(msg.SelectedElement.id != OCP.id)
            {
                this.Stroke = Brushes.Blue;
            }
        }
        #endregion SelectionHandler


    }
}
