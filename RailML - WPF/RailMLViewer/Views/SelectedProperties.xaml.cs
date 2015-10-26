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
using System.Reflection;

namespace RailML___WPF.RailMLViewer.Views
{
    /// <summary>
    /// Interaction logic for SelectedProperties.xaml
    /// </summary>
    public partial class SelectedProperties : UserControl
    {
        private SelectedPropertiesViewModel _viewmodel { get; set; }
        public SelectedProperties()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SelectedProperties_Loaded);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler (SelectedProperties_DataContextChanged);
        }

        void SelectedProperties_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewmodel = this.DataContext as SelectedPropertiesViewModel;
            CreateControls();
        }
        void SelectedProperties_Loaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = this.DataContext as SelectedPropertiesViewModel;
            CreateControls();
        }

        void CreateControls()
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            if (PropertiesDock.Children.Count > 0)
            {
                PropertiesDock.Children.Clear();
            }
            
            PropertiesDock.Children.Add(new PropertiesPresenter(_viewmodel.selectedobject, _viewmodel.selectedobject.id, true));
            Mouse.OverrideCursor = null;
          
            
            
        }
    }
}
