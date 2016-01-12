using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data.RecurrentDataProviders
{
    class DelaySizeInputRecurrentProvider : IRecurrentDataProvider
    {
        public int Size { get; private set; }
        public int StartIndex { get; private set; }
    }
}
