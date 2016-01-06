using Encog.Engine.Network.Activation;
using Encog.ML;
using Encog.Neural.Freeform;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using RailMLNeural.Data;
using RailMLNeural.Neural.Data;
using RailMLNeural.UI.Neural.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    class RecurrentNetwork : IMLMethod
    {
        #region Parameters
        private BasicNetwork _network;

        private RecurrentDataProvider _dataProvider;

        #endregion Parameters

        #region Public
        public RecurrentNetwork(RecurrentDataProvider provider, List<LayerSize> layers)
        {
            _dataProvider = provider;
            ConstructNetwork(layers);
        }

        #endregion Public

        #region Private

        private void ConstructNetwork(List<LayerSize> layers)
        {
            _network = new BasicNetwork();
            _network.AddLayer(new BasicLayer(null, false, _dataProvider.InputCount));
            foreach(LayerSize layer in layers)
            {
                _network.AddLayer(layer.CreateLayer());
            }
            _network.AddLayer(new BasicLayer(new ActivationTANH(), true, _dataProvider.OutputCount));
        }

        #endregion Private
    }
}
