using Encog.MathUtil.Error;
using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks.Training;
using RailMLNeural.Data;
using RailMLNeural.Neural.Algorithms.Propagators;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using RailMLNeural.Neural.Data.RecurrentDataProviders;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    [Serializable]
    class RecurrentCalculateScore : ICalculateScore
    {
        public bool ShouldMinimize
        {
            get
            {
                return true;
            }
        }

        public bool RequireSingleThreaded
        {
            get
            {
                return false;
            }
        }

        private IContainsGraph _owner;
        private int BatchSize;
        private DelayCombinationSet _data;
        private int _startIndex;
        private int _iterationNumber;
        private ThreadLocal<SimplifiedGraph> _graph; 

        public RecurrentCalculateScore(IContainsGraph Owner, DelayCombinationSet Data, int batchSize)
        {
            BatchSize = batchSize;
            _owner = Owner;
            _data = Data;
            _graph = new ThreadLocal<SimplifiedGraph>(() =>
            {
                return _owner.Graph.Clone();
            });
        }

        public double CalculateScore(IMLMethod Network)
        {
            FlatGRNN _network = Network as FlatGRNN;
            SimplifiedGraph Graph = _graph.Value;
            IPropagator Propagator = _owner.Propagator.OpenAdditional();
            ErrorCalculation calc = new ErrorCalculation();
            int BatchSize = 0;
            if(_owner.Training is IBatchSize)
            {
                BatchSize = ((IBatchSize)_owner.Training).BatchSize;
            }
            List<DelayCombination> l = new List<DelayCombination>();
            if(BatchSize == 0)
            {
                l.AddRange(_data.Collection);
            }
            else
            {
                int n = _startIndex;
                if(_owner.Training.IterationNumber != _iterationNumber)
                {
                    _iterationNumber = _owner.Training.IterationNumber;
                    _startIndex += BatchSize;
                    if(_startIndex > _data.Count)
                    {
                        _startIndex = 0;
                    }

                }
                for (int i = _startIndex; i < _startIndex + BatchSize; i++)
                {
                    if (n >= _data.Count)
                    {
                        n = 0;
                    }
                    l.Add(_data[n]);
                    n++;
                }
            }
                    
            foreach (var dc in l)
            {
                Graph.GenerateGraph(dc, true);
                Propagator.NewCycle(Graph, dc, true);
                _network.ClearContexts();
                while (Propagator.HasNext)
                {
                    IMLDataPair pair = Propagator.MoveNext();
                    EdgeTrainRepresentation rep = Propagator.Current as EdgeTrainRepresentation;
                    bool Reverse = false;
                    if (rep.Direction == DirectionEnum.Up)
                    {
                        Reverse = true;
                    }
                    IMLData output = _network.Process(pair.Input, rep.Edge.Index, Reverse);
                    if (!Propagator.IgnoreCurrent)
                    {
                        Propagator.PreProcess(ref output, ref pair);
                        calc.UpdateError(output, pair.Ideal, pair.Significance);
                        Propagator.Update(output);
                    }
                }
            }
            
            return calc.Calculate();
        }
    }
}
