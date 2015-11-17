using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for NetworkRenderView.
    /// </summary>
    public partial class NetworkRenderView : UserControl
    {
        Canvas canvas;
        ScaleTransform st;
        TranslateTransform tt;
        TransformGroup tg;
        Point LastMousePosition;
        Polyline selectedline;
        Ellipse selectednode;
        Rectangle selectedswitch;
        NetworkRenderViewModel viewmodel;
        static double scalerate = 1.1;
        public NetworkRenderView()
        {
            InitializeComponent();        
            this.Loaded += new RoutedEventHandler(view_Loaded);
        }

        void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            canvas = (Canvas)sender;
            st = new ScaleTransform();
            tt = new TranslateTransform();
            tg = new TransformGroup();
            tg.Children.Add(st); tg.Children.Add(tt);
            itemscontrol1.RenderTransform = tg;
            ResizeToFit();
        }

         

        void view_Loaded(object sender, RoutedEventArgs e)
        {
            viewmodel = this.DataContext as NetworkRenderViewModel;
        }

        int counter = 0;
        public void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
            {
                double oldx = e.GetPosition(itemscontrol1).X;
                double oldy = e.GetPosition(itemscontrol1).Y;
                st.ScaleX *= scalerate;
                st.ScaleY *= scalerate;
                tt.X -= (oldx - e.GetPosition(itemscontrol1).X) * st.ScaleX;
                tt.Y += (oldy - e.GetPosition(itemscontrol1).Y) * st.ScaleX;
                counter++;
               
            }
            else
            {
                double oldx = e.GetPosition(itemscontrol1).X;
                double oldy = e.GetPosition(itemscontrol1).Y;
                st.ScaleX /= scalerate;
                st.ScaleY /= scalerate;
                tt.X -= (oldx - e.GetPosition(itemscontrol1).X) * st.ScaleX;
                tt.Y += (oldy - e.GetPosition(itemscontrol1).Y)*st.ScaleX;
                counter--;
                
            }

            if(counter > 5 || counter < -5)
            {
                for (int i = 0; i < itemscontrol1.Items.Count; i++)
                {
                    //BackgroundWorker worker = new BackgroundWorker();
                    //worker.DoWork += new DoWorkEventHandler(RenderEngine_Scale);
                    //worker.RunWorkerAsync(counter);
                    var uiElement = itemscontrol1.ItemContainerGenerator.ContainerFromIndex(i);
                    try
                    {
                        Polyline line = (Polyline)VisualTreeHelper.GetChild(uiElement, 0);
                        if (counter > 0) { line.StrokeThickness /= Math.Pow(scalerate, 6); }
                        else { line.StrokeThickness *= Math.Pow(scalerate, 6); }
                        continue;
                    }
                    catch { }
                    try
                    {
                        Ellipse node = (Ellipse)VisualTreeHelper.GetChild(uiElement, 0);
                        double oldwidth = node.Width; double oldheight = node.Height;
                        if (counter > 0) { node.Height /= Math.Pow(scalerate, 6); node.Width /= Math.Pow(scalerate, 6); }
                        else { node.Height *= Math.Pow(scalerate, 6); node.Width *= Math.Pow(scalerate, 6); }
                        ((TranslateTransform)node.RenderTransform).X += (oldwidth - node.Width) / 2;
                        ((TranslateTransform)node.RenderTransform).Y += (oldheight - node.Height) / 2;
                        continue;
                    }
                    catch { }
                }

                counter = 0;
           
            }
        }

        //private void RenderEngine_Scale(object sender, DoWorkEventArgs e)
        //{
        //    for (int i = 0; i < itemscontrol1.Items.Count; i++)
        //    {

        //        int c = (int)e.Argument;
                
        //        var uiElement = itemscontrol1.ItemContainerGenerator.ContainerFromIndex(i);
        //        uiElement.Dispatcher.BeginInvoke((Action)(() =>
        //        {


        //            try
        //            {
        //                Polyline line = (Polyline)VisualTreeHelper.GetChild(uiElement, 0);
        //                if (c > 0) { line.StrokeThickness /= Math.Pow(scalerate, 6); }
        //                else { line.StrokeThickness *= Math.Pow(scalerate, 6); }
        //                return;
        //            }
        //            catch { }
        //            try
        //            {
        //                Ellipse node = (Ellipse)VisualTreeHelper.GetChild(uiElement, 0);
        //                double oldwidth = node.Width; double oldheight = node.Height;
        //                if (c > 0) { node.Height /= Math.Pow(scalerate, 6); node.Width /= Math.Pow(scalerate, 6); }
        //                else { node.Height *= Math.Pow(scalerate, 6); node.Width *= Math.Pow(scalerate, 6); }
        //                ((TranslateTransform)node.RenderTransform).X += (oldwidth - node.Width) / 2;
        //                ((TranslateTransform)node.RenderTransform).Y += (oldheight - node.Height) / 2;
        //                return;
        //            }
        //            catch { }
        //        }));
        //    }
        //}

        public void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            //start = e.GetPosition(this);
            //canvas.CaptureMouse();

        }

        public void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                Vector v = position - LastMousePosition;
                tt.X -= v.X * st.ScaleX;
                tt.Y += v.Y * st.ScaleX;
                e.Handled = true;
            }
            else
            {
                ReleaseMouseCapture();
            }
            LastMousePosition = position;

        }

        public void canvas_LeftMouseButtonUp(object sender,MouseButtonEventArgs e)
        {
            //canvas.ReleaseMouseCapture();
        }

        public void line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null; }
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectedline = sender as Polyline;
            selectedline.StrokeDashArray = new DoubleCollection(new double[2]{6,3});

            //PropertiesContentControl.Content =new SelectedPropertiesViewModel(selectedline.Tag);
        }

        public void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null;}
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectednode = sender as Ellipse;
            selectednode.Fill = Brushes.Red;

            //PropertiesContentControl.Content = new SelectedPropertiesViewModel(selectednode.Tag);
        }

        public void Switch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null; }
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectedswitch = sender as Rectangle;
            selectedswitch.Fill = Brushes.Yellow;

            //PropertiesContentControl.Content = new SelectedPropertiesViewModel(selectedswitch.Tag);
        }



        public void ResizeToFit()
        {
            double origscale = st.ScaleX;
            st.ScaleX = 1;
            st.ScaleY = -1;
            tt.X = 0;
            tt.Y = 0;

            double[] outline = new double[4];
            outline[0]=9999999999999999999;
            outline[1]=-999999999999999999;
            outline[2] =9999999999999999999;
            outline[3] =-999999999999999999;
            double height = this.ActualHeight;
            double width = this.ActualWidth;

            foreach(RailMLNeural.UI.RailML.ViewModel.Track track in viewmodel.tracklines)
            {
                if (track.X1 < outline[0]) { outline[0] = track.X1; }
                if (track.X2 < outline[0]) { outline[0] = track.X2; }
                if (track.X1 > outline[1]) { outline[1] = track.X1; }
                if (track.X2 > outline[1]) { outline[1] = track.X2; }
                if (track.Y1 < outline[2]) { outline[2] = track.Y1; }
                if (track.Y2 < outline[2]) { outline[2] = track.Y2; }
                if (track.Y1 > outline[3]) { outline[3] = track.Y1; }
                if (track.Y2 > outline[3]) { outline[3] = track.Y2; }
            }

            

            if((outline[1]-outline[0])/width > (outline[3]-outline[2])/height )
            {
                st.ScaleX /= ((outline[1] - outline[0]) / (0.9*width));
                st.ScaleY /= ((outline[1] - outline[0]) / (0.9*width));
            }
            else
            {
                st.ScaleX /= ((outline[3] - outline[2]) / (0.9*height));
                st.ScaleY /= ((outline[3] - outline[2]) / (0.9*height));
            }

            tt.X = (outline[0] *st.ScaleX + 0.1 * width);
            tt.Y = -((outline[2])*st.ScaleX - 0.9 * height);

            for (int i = 0; i < itemscontrol1.Items.Count; i++)
            {
                var uiElement = itemscontrol1.ItemContainerGenerator.ContainerFromIndex(i);
                try
                {
                    Polyline line = (Polyline)VisualTreeHelper.GetChild(uiElement, 0);
                    line.StrokeThickness /= (st.ScaleX / origscale);
                    continue;
                }
                catch { }
                try
                {
                    Ellipse node = (Ellipse)VisualTreeHelper.GetChild(uiElement, 0);
                    double oldwidth = node.Width; double oldheight = node.Height;
                    node.Width /= (st.ScaleX / origscale);
                    node.Height /= (st.ScaleX / origscale);
                    ((TranslateTransform)node.RenderTransform).X += (oldwidth - node.Width) / 2;
                    ((TranslateTransform)node.RenderTransform).Y += (oldheight - node.Height) / 2;
                    continue;
                }
                catch { }

            }
            
        }

        
    }
    
}