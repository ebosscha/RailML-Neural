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
    [Serializable]
    class ChronologicalPropagator : IPropagator
    {
        #region Parameters
        public PropagatorEnum Type { get { return PropagatorEnum.Chronological; } }
        private int _currentIndex;
        private List<string> _ignoreHeaders;
        private int _stopIndex;
        private dynamic _owner;
        public object Current { get { return _currentRep; } }
        private EdgeTrainRepresentation _currentRep { get; set; }
        private SimplifiedGraph _graph;
        public bool UseSubGraph;
        

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
                return (_currentIndex < _stopIndex);
            }
        }

        public bool IgnoreCurrent
        {
            get { return _ignoreHeaders.Contains(_currentRep.TrainHeaderCode); }
        }

        private List<EdgeTrainRepresentation> _EdgeTrainRepresentations;
        #endregion Parameters

        #region Public
        public ChronologicalPropagator(IContainsGraph Owner, bool useSubGraph)
        {
            _owner = Owner;
            _ignoreHeaders = new List<string>();
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>();
            UseSubGraph = useSubGraph;
        }

        private void NewCycle(DateTime starttime, DateTime endtime )
        {     
            //_EdgeTrainRepresentations.RemoveAll(x => _ignoreHeaders.Contains(x.TrainHeaderCode));
            //_currentIndex = 0;
            _currentIndex = _EdgeTrainRepresentations.FindIndex(x => x.ScheduledDepartureTime.TimeOfDay > starttime.TimeOfDay) - 1;
            _stopIndex = _EdgeTrainRepresentations.FindIndex(x => x.IdealArrivalTime.TimeOfDay > endtime.TimeOfDay);
        }

        public void NewCycle(SimplifiedGraph Graph, DelayCombination DelayCombination, bool LimitTime)
        {
            _graph = Graph;
            _ignoreHeaders = new List<string>();
            foreach(Delay d in DelayCombination.primarydelays)
            {
                _ignoreHeaders.Add(d.traincode);
            }
            
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>();
            
            foreach (var Edge in _graph.Edges.Where(x => !UseSubGraph || x.IsSubGraph))
            {
                _EdgeTrainRepresentations.AddRange(Edge.Trains);
            }
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>(_EdgeTrainRepresentations.OrderBy(x => x.PredictedDepartureTime));

            if (LimitTime)
            {
                NewCycle(DelayCombination.primarydelays.Min(x => x.ScheduledDeparture), DelayCombination.GetEndTime());
            }
            else
            {
                _currentIndex = 0;
                _stopIndex = _EdgeTrainRepresentations.Count - 1;
            }
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
                _currentRep = rep;
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
            int n = 0;
            for(int i = 0; i < _outputDataProviders.Count; i++)
            {
                double[] array = new double[_outputDataProviders[i].Size];
                for (int j = 0; j < _outputDataProviders[i].Size; j++ )
                {
                    array[j] = Data[n];
                    n++;
                }
                _outputDataProviders[i].Update(array, _currentRep);
            }
        }

        public IPropagator OpenAdditional()
        {
            ChronologicalPropagator result = new ChronologicalPropagator(_owner, UseSubGraph);
            return result;
        }
        #endregion Public

        #region Private
        private void SetDateTime(DelayCombination dc)
        {
            
        }
        #endregion Private
    }
}
