using RailMLNeural.Data;
using RailMLNeural.Neural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.UI.Neural.ViewModel
{
    public class NeuralSelectionChangedMessage
    {
        private INeuralConfiguration _network;
        public INeuralConfiguration NeuralNetwork
        {
            get { return _network; }
            set { _network = value; }
        }
    }

    public class AddNeuralNetworkMessage
    {
        private INeuralConfiguration _network;
        public INeuralConfiguration NeuralNetwork
        {
            get { return _network; }
            set { _network = value; }
        }
    }

    public class IsBusyMessage
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; }
        }
    }

    public class RefreshCollectionMessage
    {

    }

}
