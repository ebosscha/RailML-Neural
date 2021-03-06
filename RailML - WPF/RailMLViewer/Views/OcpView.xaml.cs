﻿using System;
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
    /// Interaction logic for OcpView.xaml
    /// </summary>
    public partial class OcpView : UserControl
    {
        private OcpViewModel _viewmodel;
        public OcpView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OcpView_Loaded);
        }

        private void OcpView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = this.DataContext as OcpViewModel;
        }
    }
}
