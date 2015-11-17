using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for TrackTableView.
    /// </summary>
    public partial class TrackTableView : UserControl
    {
        public event EventHandler<SelectedPropertyChangedEventArgs> SelectionChanged;
        private List<object> _selecteditems = new List<object>();
        /// <summary>
        /// Initializes a new instance of the TrackTableView class.
        /// </summary>
        public TrackTableView()
        {
            InitializeComponent();
        }

        private void DataGridControl_SelectionChanged(object sender, Xceed.Wpf.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            //foreach(var info in e.SelectionInfos)
            //{
            //    _selecteditems.Add(info.AddedItems);
            //    _selecteditems.Remove(info.RemovedItems);

            //}
            
            if (this.SelectionChanged != null)
            {
                SelectionChanged(this, new SelectedPropertyChangedEventArgs(true, e.SelectionInfos[0].DataGridContext.CurrentItem));
            }
            
            e.Handled = true;

        }
    }
}