using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for CreateFeedForwardView.
    /// </summary>
    public partial class CreateRecursiveNetworkView : Window
    {
        /// <summary>
        /// Initializes a new instance of the CreateFeedForwardView class.
        /// </summary>
        public CreateRecursiveNetworkView()
        {
            InitializeComponent();
            // View is not hittestable when preprocessing
            Messenger.Default.Register<IsBusyMessage>(this, (action) => { this.IsHitTestVisible = !action.IsBusy; });
        }

        private void HiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CreateRecursiveNetworkViewModel vm = (CreateRecursiveNetworkViewModel)this.DataContext;
            vm.HiddenLayerSize_ValueChanged(sender, e);
        }

        public CreateRecursiveNetworkViewModel VM
        {
            get
            {
                return (CreateRecursiveNetworkViewModel)this.DataContext;
            }
        }

        
    }
}