using Encog.Engine.Network.Activation;
using Encog.MathUtil.Error;
using Encog.ML;
using Encog.ML.Data;
using Encog.Neural.Freeform;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using RailMLNeural.UI.Neural.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    [Serializable]
    class RecurrentNetwork : IMLMethod, IMLEncodable
    {
        #region Parameters
        private BasicNetwork _network;

        public BasicNetwork Network { get { return _network; }
            set { _network = value; }
        }

        private SimplifiedGraph _graph
        {
            get
            {
                return _owner.Graph;
            }
        }

        private IPropagator _propagator
        {
            get
            {
                return _owner.Propagator;
            }
        }
        private RecurrentConfiguration _owner;

        #endregion Parameters

        #region Public
        public RecurrentNetwork(RecurrentConfiguration Owner)
        {
            _owner = Owner;
            ConstructNetwork();
        }

        public void Compute()
        {
            while(_propagator.HasNext)
            {
                IMLDataPair pair = _propagator.MoveNext();
                IMLData Output = _network.Compute(pair.Input);
                _propagator.Update(Output);
            }
        }

        public double CalculateError()
        {
            var errorCalculation = new ErrorCalculation();
            var actual = new double[_network.OutputCount];
            IMLDataPair pair;
            while(_propagator.HasNext)
            {
                pair = _propagator.MoveNext();
                _network.Flat.Compute(pair.Input, actual);
                errorCalculation.UpdateError(actual, pair.Ideal, pair.Significance);
            }
            return errorCalculation.Calculate();
        }
        

        #endregion Public

        #region Private

        private void ConstructNetwork()
        {
            //_network = new BasicNetwork();
            //_network.AddLayer(new BasicLayer(null, false, _dataProvider.InputCount));
            //foreach(LayerSize layer in layers)
            //{
            //    _network.AddLayer(layer.CreateLayer());
            //}
            //_network.AddLayer(new BasicLayer(new ActivationTANH(), true, _dataProvider.OutputCount));
        }

        #endregion Private

        #region Encoding
        public void EncodeToArray(double[] array)
        {
            _network.EncodeToArray(array);
        }

        public int EncodedArrayLength()
        {
            return _network.EncodedArrayLength();
        }

        public void DecodeFromArray(double[] array)
        {
            _network.DecodeFromArray(array);
        }

        #endregion Encoding
    }
}
