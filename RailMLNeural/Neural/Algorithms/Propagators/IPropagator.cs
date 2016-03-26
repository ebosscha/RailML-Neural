using Encog.ML.Data;
using RailMLNeural.Data;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Propagators
{
    public interface IPropagator
    {
        IMLDataPair MoveNext();
        PropagatorEnum Type { get; }
        void NewCycle(SimplifiedGraph Graph, DelayCombination DelayCombination, bool LimitTime);
        bool HasNext { get; }
        void Update(IMLData Data);
        object Current { get; }
        bool IgnoreCurrent { get; }
        bool UseSubGraph { get; }
        IPropagator OpenAdditional();
        void PreProcess(ref IMLData Output, ref IMLDataPair Pair);
        IMLDataPair PreprocessedPair { get; }
        double[] PreprocessedOutput { get; }
        bool CurrentCorrupted { get; }
    }

    [Serializable]
    public enum PropagatorEnum
    {
        Chronological,
        FollowTrain
    }
}
