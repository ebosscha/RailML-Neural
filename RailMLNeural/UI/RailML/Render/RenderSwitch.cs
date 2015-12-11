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
    public class RenderSwitch : Shape
    {
        #region DependencyProperties
        public static readonly DependencyProperty SwitchProperty = DependencyProperty.Register("Switch", typeof(eSwitch), typeof(RenderSwitch),new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(SwitchProperty_Changed)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RenderSwitch), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ScaleProperty_Changed)));
        #endregion DependencyProperties

        #region Properties
        public eSwitch Switch
        {
            get { return (eSwitch)GetValue(SwitchProperty); }
            set { SetValue(SwitchProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        private Geometry geometry {get; set;}
        private Brush OriginalBrush { get; set; }

        #endregion Properties

        #region Initialization
        public RenderSwitch()
        {
            this.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Switch_MouseLeftButtonDown);
            this.Cursor = Cursors.Hand;
            Messenger.Default.Register<SelectionChangedMessage>(this, action => Selection_Changed(action));
            Messenger.Default.Register<HighlightElementMessage>(this, action => Highlight(action));
            Messenger.Default.Register<ClearHighlightMessage>(this, action => ClearHighlight(action));
            this.Loaded += new RoutedEventHandler(Shape_Loaded);
        }

        private void Shape_Loaded(object sender, RoutedEventArgs e)
        {
            OriginalBrush = this.Fill;
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
        private static void SwitchProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderSwitch obj = (RenderSwitch)d;
            obj.CalculateGeometry();
        }

        private static void ScaleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderSwitch obj = (RenderSwitch)d;
            obj.CalculateGeometry();
        }

        private void CalculateGeometry()
        {
            if (Switch.geoCoord.coord.Count == 2)
            {
                geometry = new RectangleGeometry(new Rect(0,0, Width/Scale, Height/Scale));
                geometry.Transform = new RotateTransform(45, 0, 0);
                this.Margin = new Thickness(-(Width / Scale) / 2, -(Height / Scale) / 2, 0, 0);
                InvalidateVisual();
            }
        }
        #endregion Privates

        #region SelectionHandler
        private void Switch_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.Fill = Brushes.Red;
            Messenger.Default.Send(new SelectionChangedMessage(Switch));
        }

        private void Selection_Changed(SelectionChangedMessage msg)
        {
            if(msg.SelectedElement.id != Switch.id)
            {
                this.Fill = OriginalBrush;
            }
        }
        #endregion SelectionHandler

        #region Highlighting
        private void Highlight(HighlightElementMessage msg)
        {
            if (msg.Element.id == Switch.id)
            {
                this.Fill = msg.Color;
            }
        }

        private void ClearHighlight(ClearHighlightMessage msg)
        {
            this.Fill = OriginalBrush;
        }
        #endregion Highlighting


    }
}
