using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.Render
{
    public class RenderOCP : Shape
    {
        public static readonly DependencyProperty OCPProperty = DependencyProperty.Register("OCP", typeof(eOcp), typeof(RenderOCP),new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OCPProperty_Changed)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RenderOCP), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ScaleProperty_Changed)));

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

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return new System.Windows.Size(Width, Height);
        }

        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get { return geometry; }
        }

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
                geometry = new RectangleGeometry(new Rect(new Point(OCP.geoCoord.coord[0] * Scale - 0.5 * Height, OCP.geoCoord.coord[1] * Scale - 0.5 * Width), new Size(Width, Height)));
            }
        }
    
    }
}
