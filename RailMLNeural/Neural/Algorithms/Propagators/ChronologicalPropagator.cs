using Encog.ML.Data;
using Encog.ML.Data.Basic;
using RailMLNeural.Data;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Propagators
{
    class ChronologicalPropagator : IPropagator
    {
        #region Parameters
        private int _currentIndex;
        private List<string> _ignoreHeaders;
        private RecurrentConfiguration _owner;
        private SimplifiedGraph _graph
        {
            get
            {
                return _owner.Graph;
            }
        }

        private List<IRecurrentDataProvider> _inputDataProviders
        {
            get
            {
                return _owner.InputDataProviders;
            }
        }

        private List<IRecurrentDataProvider> _outputDataProviders
        {
            get
            {
                return _owner.OutputDataProviders;
            }
        }

        public bool HasNext
        {
            get
            {
                return _currentIndex < _EdgeTrainRepresentations.Count - 1;
            }
        }

        private List<EdgeTrainRepresentation> _EdgeTrainRepresentations;
        #endregion Parameters

        #region Public
        public ChronologicalPropagator(RecurrentConfiguration Owner)
        {
            _owner = Owner;
            _ignoreHeaders = new List<string>();
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>();
        }

        public void NewCycle()
        {
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>();
            foreach(var Edge in _graph.Edges)
            {
                
                _EdgeTrainRepresentations.AddRange(Edge.Trains);
                
            }
            _EdgeTrainRepresentations.RemoveAll(x => _ignoreHeaders.Contains(x.TrainHeaderCode));
            _EdgeTrainRepresentations.OrderBy(x => x.PredictedDepartureTime);
            _currentIndex = 0;
        }

        public void NewCycle(DelayCombination DelayCombination)
        {
            _ignoreHeaders = new List<string>();
            foreach(Delay d in DelayCombination.primarydelays)
            {
                _ignoreHeaders.Add(d.traincode);
            }
            NewCycle();
        }

        public IMLDataPair MoveNext()
        {
            EdgeTrainRepresentation rep = null;
            if(HasNext)
            {
                List<double> inputlist = new List<double>();
                List<double> ideallist = new List<double>();
                _currentIndex++;
                rep = _EdgeTrainRepresentations[_currentIndex];
                foreach(var provider in _inputDataProviders)
                {
                    inputlist.AddRange(provider.Process(rep));
                }
                foreach(var provider in _outputDataProviders)
                {
                    ideallist.AddRange(provider.Process(rep));
                }
                IMLData input = new BasicMLData(inputlist.ToArray());
                IMLData ideal = new BasicMLData(ideallist.ToArray());
                return new BasicMLDataPair(input, ideal);
            }

            throw new Exception("Propagator can't move to next object");
        }

        public void Update(IMLData Data)
        {
            //TODO!!!
        }
        #endregion Public

        #region Private
        private void SetDateTime(DelayCombination dc)
        {
            
        }
        #endregion Private
    }
}
