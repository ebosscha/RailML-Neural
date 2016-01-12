using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Propagators
{
    interface IPropagator
    {
        object MoveNext();
        void NewCycle();       
    }
}
