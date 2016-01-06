using RailMLNeural.UI.Neural.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace RailMLNeural.UI.Neural.Views
{
    /// <summary>
    /// Description for NeuralCollectionView.
    /// </summary>
    public partial class NeuralCollectionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the NeuralCollectionView class.
        /// </summary>
        public NeuralCollectionView()
        {
            InitializeComponent();
        }

        public NeuralCollectionViewModel VM
        {
            get { return (NeuralCollectionViewModel)DataContext; }
        }
    }
}