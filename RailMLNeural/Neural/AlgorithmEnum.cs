using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural
{
    public enum AlgorithmEnum
    {
        PerLineClassification,
        PerLineExact,
        PerTrackClassification,
        PerTrackExact,
        RecurrentClassification,
        RecurrentExact
    }
}
