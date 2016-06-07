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
    class FollowTrainPropagator : IPropagator
    {
        #region Parameters
        public PropagatorEnum Type { get { return PropagatorEnum.FollowTrain; } }
        private dynamic _owner;
        private SimplifiedGraph _graph { get; set; }
        public List<string> PrimaryHeaderCodes;
        public List<string> SecondaryHeaderCodes;
        public bool UseSubGraph { get; private set; }
        public IMLDataPair PreprocessedPair { get; set; }
        public double[] PreprocessedOutput { get; set; }
        public bool CurrentCorrupted { get; set; }

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
                return _EdgeTrainRepresentations.Any();
            }
        }

        public object Current { get { return _currentRep; } }
        public bool IgnoreCurrent { get { return _currentRep.Next != null; } }
        private EdgeTrainRepresentation _currentRep { get; set; }

        private List<EdgeTrainRepresentation> _EdgeTrainRepresentations;
        #endregion Parameters

        #region Public
        public FollowTrainPropagator(IContainsGraph Owner, bool useSubGraph)
        {
            _owner = Owner;
            UseSubGraph = useSubGraph;
        }

        public void NewCycle(SimplifiedGraph Graph, DelayCombination DelayCombination, bool LimitTime)
        {
            _graph = Graph;
            _graph.GenerateGraph(DelayCombination, true);
            PrimaryHeaderCodes = new List<string>();
            SecondaryHeaderCodes = new List<string>();
            foreach (Delay d in DelayCombination.primarydelays)
            {
                PrimaryHeaderCodes.Add(d.traincode);
            }
            foreach (Delay d in DelayCombination.secondarydelays)
            {
                SecondaryHeaderCodes.Add(d.traincode);
            }
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>();
            Graph.SetSubGraph(UseSubGraph);

            foreach (var Edge in _graph.Edges.Where(x => !UseSubGraph || x.IsSubGraph))
            {
                _EdgeTrainRepresentations.AddRange(Edge.Trains);
            }
            _EdgeTrainRepresentations = _EdgeTrainRepresentations.Where(x => PrimaryHeaderCodes.Contains(x.TrainHeaderCode)).ToList();
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>(_EdgeTrainRepresentations.OrderBy(x => x.ForecastedDepartureTime));
            
            // Remove non-delayed parts of train sequence
            if (UseSubGraph)
            {
                while (_EdgeTrainRepresentations.Count > 1
                    && Math.Abs((_EdgeTrainRepresentations[0].IdealDepartureTime - _EdgeTrainRepresentations[1].ScheduledDepartureTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[0].IdealArrivalTime - _EdgeTrainRepresentations[1].ScheduledArrivalTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[1].IdealDepartureTime - _EdgeTrainRepresentations[1].ScheduledDepartureTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[1].IdealArrivalTime - _EdgeTrainRepresentations[1].ScheduledArrivalTime).TotalMinutes) < 3)
                {
                    _EdgeTrainRepresentations.RemoveAt(0);
                }
                while (_EdgeTrainRepresentations.Count > 1
                    && Math.Abs((_EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 2].IdealDepartureTime - _EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 2].ScheduledDepartureTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 2].IdealArrivalTime - _EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 2].ScheduledArrivalTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 1].IdealDepartureTime - _EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 1].ScheduledDepartureTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 1].IdealArrivalTime - _EdgeTrainRepresentations[_EdgeTrainRepresentations.Count - 1].ScheduledArrivalTime).TotalMinutes) < 3)
                {
                    _EdgeTrainRepresentations.RemoveAt(_EdgeTrainRepresentations.Count - 1);
                }
                if (_EdgeTrainRepresentations.Count == 1
                    && Math.Abs((_EdgeTrainRepresentations[0].IdealDepartureTime - _EdgeTrainRepresentations[0].ScheduledDepartureTime).TotalMinutes) < 3
                    && Math.Abs((_EdgeTrainRepresentations[0].IdealArrivalTime - _EdgeTrainRepresentations[0].ScheduledArrivalTime).TotalMinutes) < 3)
                {
                    _EdgeTrainRepresentations.RemoveAt(0);
                }
            }

        }

        public IMLDataPair MoveNext()
        {
            CurrentCorrupted = false;
            EdgeTrainRepresentation rep = null;
            if(HasNext)
            {
                List<double> inputlist = new List<double>();
                List<double> ideallist = new List<double>();
                //_currentIndex++;
                rep = _EdgeTrainRepresentations[0];
                _EdgeTrainRepresentations.RemoveAt(0);
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
                if(ideallist.Concat(inputlist).Any(x => x == double.PositiveInfinity || x == double.NegativeInfinity || x == double.NaN))
                {
                    CurrentCorrupted = true;
                }
                _currentRep.IsHandled = true;
                return new BasicMLDataPair(input, ideal);
            }

            throw new Exception("Propagator can't move to next object");
        }
        

        public void Update(IMLData Data)
        {
            return;
        }

        public void PreProcess(ref IMLData Output, ref IMLDataPair Pair)
        {
            //TODO !!
        }

        public IPropagator OpenAdditional()
        {
            FollowTrainPropagator result = new FollowTrainPropagator(_owner, UseSubGraph);
            return result;
        }
        #endregion Public

        #region Private
        
        #endregion Private
    }
}
