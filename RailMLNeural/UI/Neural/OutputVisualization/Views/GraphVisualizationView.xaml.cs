using RailMLNeural.UI.Neural.OutputVisualization.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RailMLNeural.UI.Neural.OutputVisualization.Views
{
    /// <summary>
    /// Description for GraphVisualizationview.
    /// </summary>
    public partial class GraphVisualizationView : UserControl, INotifyPropertyChanged
    {
        private ZoomableCanvas _canvas;
        public ZoomableCanvas Canvas
        {
            get { return _canvas; }
            set
            {
                _canvas = value;
                RaisePropertyChanged("Canvas");
            }
        }


        /// <summary>
        /// Initializes a new instance of the GraphVisualizationview class.
        /// </summary>
        public GraphVisualizationView()
        {
            InitializeComponent();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas = (ZoomableCanvas)sender;
            ZoomToBounds();
        }

        Point LastMousePosition;
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(MyListBox);
            if (e.LeftButton == MouseButtonState.Pressed
                && !(e.OriginalSource is Thumb) && !(e.OriginalSource is Button)) // Don't block the scrollbars.
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
            if (Canvas.ItemsOwner.Items.Count > 0)
            {
                Rect extent = _canvas.Extent;
                if (extent == Rect.Empty)
                {
                    extent = ((RenderContainer)Canvas.ItemsOwner.Items[0]).Extent();
                    foreach (RenderContainer child in Canvas.ItemsOwner.Items)
                    {
                        extent.Union(child.Extent());
                    }
                }
                //_canvas.Scale = Math.Min(this.ActualHeight / extent.Height, this.ActualWidth / extent.Width)*0.9;
                //extent = _canvas.Extent;
                //Point viewcenter = new Point(this.ActualWidth/2, this.ActualHeight/2);
                //_canvas.Offset = (Point)(extent.GetCenter()-viewcenter);
                //int i = 1;
                _canvas.Viewbox = extent;
            }
            else
            {
                _canvas.Viewbox = new Rect(-1000, -1000, 2000, 2000);
            }

        }

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