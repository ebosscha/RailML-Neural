using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    interface IRecurrentDataProvider
    {
        int Size { get; }
        int StartIndex { get; }
        double[] Process(EdgeTrainRepresentation rep);
        
    }
}
