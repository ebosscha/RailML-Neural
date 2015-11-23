using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for VisualizationView.
    /// </summary>
    public partial class VisualizationView : UserControl
    {
        private ZoomableCanvas _canvas;
        private Point LastMousePosition;
        /// <summary>
        /// Initializes a new instance of the VisualizationView class.
        /// </summary>
        public VisualizationView()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Visualization_Loaded);
        }

        private void Visualization_Loaded(object sender, RoutedEventArgs e)
        {
            //ZoomToBounds();
            MyListBox.Focusable = false;
        }
        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _canvas = (ZoomableCanvas)sender;
            _canvas.IsVirtualizing = true;
            //_canvas.ApplyTransform = false;
            //_canvas.Stretch = System.Windows.Media.Stretch.None;
            ZoomToBounds();
        }
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(MyListBox);
            if (e.LeftButton == MouseButtonState.Pressed
                && !(e.OriginalSource is Thumb)) // Don't block the scrollbars.
            {
                CaptureMouse();
                //_canvas.Offset -= position - LastMousePosition;
                Rect viewbox = _canvas.Viewbox;
                viewbox.Location -= (position - LastMousePosition) / _canvas.Scale;
                _canvas.Viewbox = viewbox;
                e.Handled = true;
            }
            else
            {
                ReleaseMouseCapture();
            }
            LastMousePosition = position;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            var x = Math.Pow(2, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);
            //_canvas.Scale *= x;
            Rect viewbox = _canvas.Viewbox;
            viewbox.Height /= x;
            viewbox.Width /= x;

            // Adjust the offset to make the point under the mouse stay still.
            var position = (Vector)e.GetPosition(MyListBox);
            //_canvas.Offset = (Point)((Vector)(_canvas.Offset + position) * x - position);
            double offsetX = ((position.X / this.ActualWidth) * viewbox.Width * (x - 1));
            double offsetY = ((position.Y / this.ActualHeight) * viewbox.Height * (x - 1));
            viewbox.Location += new Vector(offsetX, offsetY);
            _canvas.Viewbox = viewbox;
            e.Handled = true;
        }

        private void ZoomToBounds()
        {
            if (_canvas.Children.Count > 0)
            {
                Rect extent = _canvas.Extent;
                //_canvas.Scale = Math.Min(this.ActualHeight / extent.Height, this.ActualWidth / extent.Width)*0.9;
                //extent = _canvas.Extent;
                //Point viewcenter = new Point(this.ActualWidth/2, this.ActualHeight/2);
                //_canvas.Offset = (Point)(extent.GetCenter()-viewcenter);
                //int i = 1;
                _canvas.Viewbox = _canvas.Extent;
            }
            else
            {
                _canvas.Viewbox = new Rect(-1000, -1000, 2000, 2000);
            }

        }
    }
}