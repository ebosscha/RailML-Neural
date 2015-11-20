using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for CreateNeuralView.
    /// </summary>
    public partial class CreateNeuralView : Window
    {
        /// <summary>
        /// Initializes a new instance of the CreateNeuralView class.
        /// </summary>
        public CreateNeuralView()
        {
            InitializeComponent();
            // View is not hittestable when preprocessing
            Messenger.Default.Register<IsBusyMessage>(this, (action) => { this.IsHitTestVisible = !action.IsBusy; });
        }

        private void HiddenLayerSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CreateNeuralViewModel vm = (CreateNeuralViewModel)this.DataContext;
            vm.HiddenLayerSize_ValueChanged(sender, e);
        }

        private void Algorithm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CreateNeuralViewModel vm = (CreateNeuralViewModel)this.DataContext;
            vm.Algorithm_SelectionChanged();
        } 

        
    }
}