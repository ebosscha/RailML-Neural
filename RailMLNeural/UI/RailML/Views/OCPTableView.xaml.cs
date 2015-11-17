using RailMLNeural.UI.RailML.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Description for OCPTableView.
    /// </summary>
    public partial class OCPTableView : UserControl
    {
        public event EventHandler<SelectedPropertyChangedEventArgs> SelectionChanged;
        private List<object> _selecteditems = new List<object>();
        /// <summary>
        /// Initializes a new instance of the OCPTableView class.
        /// </summary>
        public OCPTableView()
        {
            InitializeComponent();
        }

        private void DataGridControl_SelectionChanged(object sender, Xceed.Wpf.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            //foreach(var info in e.SelectionInfos)
            //{
            //    _selecteditems.Remove(info.RemovedItems);
            //    _selecteditems.Add(info.AddedItems);
            //}
            if(SelectionChanged != null)
            {
                SelectionChanged(this, new SelectedPropertyChangedEventArgs(true, e.SelectionInfos[0].DataGridContext.CurrentItem));
            }
            e.Handled = true;
        }
    }
}