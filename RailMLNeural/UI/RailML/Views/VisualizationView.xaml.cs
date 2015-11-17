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
        private double Scale = 1;
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
            //_canvas.IsVirtualizing = false;
            _canvas.ApplyTransform = false;
            //ZoomToBounds();
        }
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(MyListBox);
            if (e.LeftButton == MouseButtonState.Pressed
                && !(e.OriginalSource is Thumb)) // Don't block the scrollbars.
            {
                CaptureMouse();
                _canvas.Offset -= position - LastMousePosition;
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
            _canvas.Scale *= x;

            // Adjust the offset to make the point under the mouse stay still.
            var position = (Vector)e.GetPosition(MyListBox);
            _canvas.Offset = (Point)((Vector)
                (_canvas.Offset + position) * x - position);

            e.Handled = true;
        }

        private void ZoomToBounds()
        {
            Rect extent = _canvas.Extent;
            _canvas.Viewbox = extent;
            _canvas.Viewbox = Rect.Empty;
            //double rescale = Math.Min(extent.Height / this.ActualHeight, extent.Width / this.ActualWidth);
            //_canvas.Scale *= rescale;
            //extent = _canvas.Extent;
            //Point viewcenter = new Point(this.ActualWidth, this.ActualHeight);
            //_canvas.Offset += extent.GetCenter() - viewcenter;

        }
    }
}