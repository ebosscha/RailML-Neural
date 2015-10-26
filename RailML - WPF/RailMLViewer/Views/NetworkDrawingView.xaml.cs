using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RailML___WPF.RailMLViewer.ViewModels;
using RailML___WPF.Data;
using System.ComponentModel;


namespace RailML___WPF.RailMLViewer.Views
{
    /// <summary>
    /// Interaction logic for NetworkDrawingView.xaml
    /// </summary>
    public partial class NetworkDrawingView : UserControl
    {
        ScaleTransform st;
        TranslateTransform tt;
        TransformGroup tg;
        Point start;
        Polyline selectedline;
        Ellipse selectednode;
        Rectangle selectedswitch;
        NetworkDrawingViewModel viewmodel;
        static double scalerate = 1.1;
        public NetworkDrawingView()
        {
            InitializeComponent();
            st = new ScaleTransform();
            tt = new TranslateTransform();
            tg = new TransformGroup();
            tg.Children.Add(st); tg.Children.Add(tt);
            this.Loaded += new RoutedEventHandler(view_Loaded);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(View_DataContextChanged);
            itemscontrol1.RenderTransform = tg;
        }

        void view_Loaded(object sender, RoutedEventArgs e)
        {
            viewmodel = this.DataContext as NetworkDrawingViewModel;
            ResizeToFit();
        }
        void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewmodel = this.DataContext as NetworkDrawingViewModel;
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
                tt.X -= (oldx - e.GetPosition(itemscontrol1).X)*st.ScaleX;
                tt.Y += (oldy - e.GetPosition(itemscontrol1).Y)*st.ScaleX;
                counter++;
               
            }
            else
            {
                double oldx = e.GetPosition(itemscontrol1).X;
                double oldy = e.GetPosition(itemscontrol1).Y;
                st.ScaleX /= scalerate;
                st.ScaleY /= scalerate;
                tt.X -= (oldx - e.GetPosition(itemscontrol1).X)*st.ScaleX;
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
            
            start = e.GetPosition(itemscontrol1);
            itemscontrol1.CaptureMouse();

        }

        public void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(itemscontrol1.IsMouseCaptured)
            {
                Vector v = start - e.GetPosition(itemscontrol1);
                tt.X -= v.X * st.ScaleX;
                tt.Y += v.Y * st.ScaleX;
            }
        }

        public void canvas_LeftMouseButtonUp(object sender,MouseButtonEventArgs e)
        {
            itemscontrol1.ReleaseMouseCapture();
        }

        public void line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null; }
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectedline = sender as Polyline;
            selectedline.StrokeDashArray = new DoubleCollection(new double[2]{6,3});

            PropertiesContentControl.Content =new SelectedPropertiesViewModel(selectedline.Tag);
        }

        public void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null;}
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectednode = sender as Ellipse;
            selectednode.Fill = Brushes.Red;

            PropertiesContentControl.Content = new SelectedPropertiesViewModel(selectednode.Tag);
        }

        public void Switch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedline != null) { selectedline.StrokeDashArray = null; selectedline = null; }
            if (selectednode != null) { selectednode.Fill = Brushes.Blue; selectednode = null; }
            if (selectedswitch != null) { selectedswitch.Fill = Brushes.Red; selectedswitch = null; }

            selectedswitch = sender as Rectangle;
            selectedswitch.Fill = Brushes.Yellow;

            PropertiesContentControl.Content = new SelectedPropertiesViewModel(selectedswitch.Tag);
        }



        public void ResizeToFit()
        {
            double origscale = st.ScaleX;
            st.ScaleX = 1;
            st.ScaleY = -1;
            tt.X = 0;
            tt.Y = 0;

            double[] outline = new double[4];
            outline[0] = viewmodel.tracklines[0].X1;
            outline[1] = viewmodel.tracklines[0].X2;
            outline[2] = viewmodel.tracklines[0].Y1;
            outline[3] = viewmodel.tracklines[0].Y2;
            double height = Grid1.ActualHeight;
            double width = Grid1.ColumnDefinitions[1].ActualWidth;

            foreach(Track track in viewmodel.tracklines)
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
