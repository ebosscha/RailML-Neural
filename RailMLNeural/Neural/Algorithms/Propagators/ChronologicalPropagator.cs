using Encog.ML.Data;
using Encog.ML.Data.Basic;
using RailMLNeural.Data;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
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
        //private int _currentIndex;
        private List<string> _ignoreHeaders;
        //private int _stopIndex;
        private dynamic _owner;
        public object Current { get { return _currentRep; } }
        private EdgeTrainRepresentation _currentRep { get; set; }
        private SimplifiedGraph _graph;
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
                return (_EdgeTrainRepresentations.Count > 0);
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
            //_currentIndex = _EdgeTrainRepresentations.FindIndex(x => x.ScheduledDepartureTime.TimeOfDay > starttime.TimeOfDay) - 1;
            //_stopIndex = _EdgeTrainRepresentations.FindIndex(x => x.IdealArrivalTime.TimeOfDay > endtime.TimeOfDay);
            _EdgeTrainRepresentations.RemoveAll(x => x.ScheduledDepartureTime < starttime || x.ScheduledArrivalTime.TimeOfDay > endtime.TimeOfDay);
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
            Graph.SetSubGraph(UseSubGraph);
            
            foreach (var Edge in _graph.Edges.Where(x => !UseSubGraph || x.IsSubGraph))
            {
                _EdgeTrainRepresentations.AddRange(Edge.Trains);
            }
            _EdgeTrainRepresentations.Where(x => _ignoreHeaders.Contains(x.TrainHeaderCode)).ToList().ForEach(x => x.IsHandled = true);
            _EdgeTrainRepresentations = _EdgeTrainRepresentations.Where(x => !UseSubGraph || x.IsRelevant).ToList();
            _EdgeTrainRepresentations = new List<EdgeTrainRepresentation>(_EdgeTrainRepresentations.OrderBy(x => x.ForecastedDepartureTime.AddHours(-2).TimeOfDay));

            if (LimitTime)
            {
                NewCycle(DelayCombination.primarydelays.Min(x => x.ScheduledDeparture), DelayCombination.GetEndTime());
            }
            else
            {
                //_currentIndex = 0;
                //_stopIndex = _EdgeTrainRepresentations.Count - 1;
            }
        }

        public IMLDataPair MoveNext()
        {
            CurrentCorrupted = false;
            EdgeTrainRepresentation rep = null;
            if(HasNext)
            {
                int firstIndex = _EdgeTrainRepresentations.Where((x, i) => i < 10)
                    .Select((x, i) => new { Value = x.ForecastedDepartureTime.AddHours(-2).TimeOfDay, Index = i })
                    .Aggregate((a, b) => a.Value < b.Value ? a : b).Index;
                List<double> inputlist = new List<double>();
                List<double> ideallist = new List<double>();
                //_currentIndex++;
                rep = _EdgeTrainRepresentations[firstIndex];
                _EdgeTrainRepresentations.RemoveAt(firstIndex);
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
            //_currentRep.IsHandled = true;
        }

        public void PreProcess(ref IMLData Output, ref IMLDataPair Pair)
        {
            if (!_outputDataProviders.Any(x => x is DelaySizeOutputRecurrentProvider))
            {
                int outputindex = _outputDataProviders.FindIndex(x => x is IsDelayedOutputRecurrentProvider);
                int outputArrayIndex = _outputDataProviders.GetRange(0, outputindex).Sum(x => x.Size);
                if(Pair.Ideal[outputArrayIndex] != 0.0 || Pair.Ideal[outputArrayIndex] != 0.0)
                {
                    Pair.Significance = 10.0;
                }
            }
            PreprocessedOutput = new double[Output.Count];
            Output.CopyTo(PreprocessedOutput, 0, Output.Count);
            PreprocessedPair = Pair;
            if(!_outputDataProviders.Any(x => x is IsDelayedOutputRecurrentProvider))
            {
                return;
            }
            int providerindex = _outputDataProviders.FindIndex(x => x is IsDelayedOutputRecurrentProvider);
            int isDelayedIndex = _outputDataProviders.GetRange(0, providerindex).Sum(x => x.Size);
            double[] outputarray = new double[Output.Count];
            Output.CopyTo(outputarray, 0, Output.Count);
            outputarray[isDelayedIndex] = 0;
            double[] idealarray = new double[Pair.Ideal.Count];
            double[] adjustedIdeal = new double[Pair.Ideal.Count];
            Pair.Ideal.CopyTo(adjustedIdeal, 0, Pair.Ideal.Count);
            Pair.Ideal.CopyTo(idealarray, 0, Pair.Ideal.Count);
            idealarray[isDelayedIndex] = 0;
            if(Output[isDelayedIndex] < 0.5)
            {
                outputarray = new double[outputarray.Length];
                if(Pair.Ideal[isDelayedIndex] == 0)
                {
                    adjustedIdeal = new double[Pair.Ideal.Count];
                    PreprocessedOutput = new double[Output.Count];
                }
                else
                {
                    adjustedIdeal = new double[Pair.Ideal.Count];
                    adjustedIdeal[isDelayedIndex] = 1;
                    for (int i = 0; i < PreprocessedOutput.Length; i++)
                    {
                        if (i != isDelayedIndex) { PreprocessedOutput[i] = 0; }
                    }
                }
            }
            else
            {
                if (Pair.Ideal[isDelayedIndex] == 1)
                {
                    adjustedIdeal[isDelayedIndex] = 1;
                    PreprocessedOutput[isDelayedIndex] = 1;
                }
                else
                {
                    for (int i = 0; i < PreprocessedOutput.Length; i++)
                    {
                        if (i != isDelayedIndex) { PreprocessedOutput[i] = 0; }
                    }
                }
            }
            Output = new BasicMLData(outputarray);
            Pair = new BasicMLDataPair(Pair.Input, new BasicMLData(idealarray));
            PreprocessedPair = new BasicMLDataPair(Pair.Input, new BasicMLData(adjustedIdeal));
        }

        public IPropagator OpenAdditional()
        {
            ChronologicalPropagator result = new ChronologicalPropagator(_owner, UseSubGraph);
            return result;
        }
        #endregion Public

        #region Private
        
        #endregion Private
    }
}
