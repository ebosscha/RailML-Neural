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

namespace RailML___WPF.RailMLViewer.Views
{
    /// <summary>
    /// Interaction logic for TracksView.xaml
    /// </summary>
    public partial class TracksView : UserControl
    {
        private TracksViewModel _viewmodel;
        public TracksView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(TracksView_Loaded);
            TracksGrid.SelectionChanged += new SelectionChangedEventHandler(TracksGrid_SelectionChanged);
        }

        private void TracksView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = this.DataContext as TracksViewModel;
        }

        private void TracksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(TracksGrid.SelectedItems.Count == 1 && TracksGrid.SelectedItems[0].ToString() != "{NewItemPlaceholder}")
            {
                PropertiesContentControl.Content = new SelectedPropertiesViewModel(TracksGrid.SelectedItems[0]);
                PropertiesContentControl.Visibility = System.Windows.Visibility.Visible;
            }
            else { PropertiesContentControl.Visibility = System.Windows.Visibility.Hidden; }
        }
    }
}
