using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.UI.Neural.ViewModel
{
    public class NeuralSelectionChangedMessage
    {
        private NeuralNetwork _network;
        public NeuralNetwork NeuralNetwork
        {
            get { return _network; }
            set { _network = value; }
        }
    }

    public class AddNeuralNetworkMessage
    {
        private NeuralNetwork _network;
        public NeuralNetwork NeuralNetwork
        {
            get { return _network; }
            set { _network = value; }
        }
    }
}
