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
                return false;
            }
        }

        public object Current { get { return _currentRep; } }
        public bool IgnoreCurrent { get { return PrimaryHeaderCodes.Contains(_currentRep.TrainHeaderCode); } }
        private EdgeTrainRepresentation _currentRep { get; set; }

        private List<EdgeTrainRepresentation> _EdgeTrainRepresentations;
        #endregion Parameters

        #region Public
        public FollowTrainPropagator(IContainsGraph Owner)
        {
            _owner = Owner;
        }

        public void NewCycle(SimplifiedGraph Graph, DelayCombination DelayCombination, bool LimitTime)
        {
            _graph = Graph;
            _graph.GenerateGraph(DelayCombination, false);
            foreach (Delay d in DelayCombination.primarydelays)
            {
                PrimaryHeaderCodes.Add(d.traincode);
            }
            foreach (Delay d in DelayCombination.secondarydelays)
            {
                SecondaryHeaderCodes.Add(d.traincode);
            }
            BeginCycle();
        }

        public IMLDataPair MoveNext()
        {         
            throw new Exception("Propagator can't move to next object");
        }

        public void Update(IMLData Data)
        {
            //TODO!!!
        }

        public IPropagator OpenAdditional()
        {
            throw new NotImplementedException();
        }
        #endregion Public

        #region Private
        private void BeginCycle()
        {

        }
        #endregion Private
    }
}
