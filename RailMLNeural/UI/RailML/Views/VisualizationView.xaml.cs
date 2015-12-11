using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.RailML.Render;
using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RailMLNeural.RailML;
using System.Diagnostics;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for VisualizationView.
    /// </summary>
    public partial class VisualizationView : UserControl, INotifyPropertyChanged
    {
        private dynamic _selectedObject = null;

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
    
        private Point LastMousePosition;
        /// <summary>
        /// Initializes a new instance of the VisualizationView class.
        /// </summary>
        public VisualizationView()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Visualization_Loaded);
            Messenger.Default.Register<SelectionChangedMessage>(this, (msg) => { _selectedObject = msg.SelectedElement; });
        }

        private void Visualization_Loaded(object sender, RoutedEventArgs e)
        {
            //ZoomToBounds();
            MyListBox.Focusable = false;
        }
        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas = (ZoomableCanvas)sender;
            _canvas.IsVirtualizing = false;
            ZoomToBounds();
            Messenger.Default.Register<ZoomToObjectMessage>(this, (action) => ZoomToDestination(action));

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

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if(_selectedObject is eOcp || _selectedObject is eSwitch)
            {
                Point coord = new Point(_selectedObject.geoCoord.coord[0], -_selectedObject.geoCoord.coord[1]);
                Point pos = e.GetPosition(Canvas);
                Vector diff = coord - pos;
                string str = _selectedObject.id + "      X: " + (-diff.X).ToString() + "    Y: " + diff.Y.ToString();
                Debug.WriteLine(str);
            }
        }

        

        private void ZoomToBounds()
        {
            if (Canvas.ItemsOwner.Items.Count > 0)
            {
                Rect extent = _canvas.Extent;
                if(extent == Rect.Empty)
                {
                    extent = ((RenderItemContainer)Canvas.ItemsOwner.Items[0]).Extent();
                    foreach(RenderItemContainer child in Canvas.ItemsOwner.Items)
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

        #region ZoomToDestination
        private void ZoomToDestination(ZoomToObjectMessage msg)
        {
            Rect totalbounds = new Rect();

            foreach (dynamic obj in msg.Elements)
            {
                foreach (RenderItemContainer child in Canvas.ItemsOwner.Items)
                {
                    if (child.Item.id == obj.id)
                    {
                        Rect bounds = child.Extent();
                        if (totalbounds == new Rect())
                        {
                            totalbounds = bounds;
                        }
                        else
                        {
                            totalbounds.Union(bounds);
                        }
                    }
                }
            }
            Canvas.Viewbox = totalbounds;
        }

        #endregion ZoomToDestination

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