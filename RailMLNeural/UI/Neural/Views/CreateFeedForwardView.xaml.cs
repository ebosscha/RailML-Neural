using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for CreateFeedForwardView.
    /// </summary>
    public partial class CreateFeedForwardView : Window
    {
        /// <summary>
        /// Initializes a new instance of the CreateFeedForwardView class.
        /// </summary>
        public CreateFeedForwardView()
        {
            InitializeComponent();
            // View is not hittestable when preprocessing
            Messenger.Default.Register<IsBusyMessage>(this, (action) => { this.IsHitTestVisible = !action.IsBusy; });
        }

        private void HiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CreateFeedForwardViewModel vm = (CreateFeedForwardViewModel)this.DataContext;
            vm.HiddenLayerSize_ValueChanged(sender, e);
        }

        private void Algorithm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CreateFeedForwardViewModel vm = (CreateFeedForwardViewModel)this.DataContext;
            vm.Algorithm_SelectionChanged();
        } 

        public CreateFeedForwardViewModel VM
        {
            get
            {
                return (CreateFeedForwardViewModel)this.DataContext;
            }
        }

        
    }
}