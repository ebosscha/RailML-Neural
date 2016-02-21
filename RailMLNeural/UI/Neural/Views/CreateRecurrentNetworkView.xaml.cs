using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for CreateRecurrentNetworkView.
    /// </summary>
    public partial class CreateRecurrentNetworkView : Window
    {
        public CreateRecurrentNetworkView()
        {
            InitializeComponent();
            // View is not hittestable when preprocessing
            Messenger.Default.Register<IsBusyMessage>(this, (action) => { this.IsHitTestVisible = !action.IsBusy; });
        }

        private void EdgeHiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CreateRecurrentNetworkViewModel vm = (CreateRecurrentNetworkViewModel)this.DataContext;
            vm.EdgeHiddenLayerSize_ValueChanged(sender, e);
        }

        private void VertexHiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CreateRecurrentNetworkViewModel vm = (CreateRecurrentNetworkViewModel)this.DataContext;
            vm.VertexHiddenLayerSize_ValueChanged(sender, e);
        }

        public CreateRecurrentNetworkViewModel VM
        {
            get
            {
                return (CreateRecurrentNetworkViewModel)this.DataContext;
            }
        }
    }
}