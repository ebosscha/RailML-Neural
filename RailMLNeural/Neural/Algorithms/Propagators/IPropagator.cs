using Encog.ML.Data;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Propagators
{
    interface IPropagator
    {
        IMLDataPair MoveNext();
        void NewCycle(DelayCombination DelayCombination);
        bool HasNext { get; }
        void Update(IMLData Data);
    }
}
