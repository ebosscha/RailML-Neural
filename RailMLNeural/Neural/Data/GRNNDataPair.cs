using Encog.ML.Data;
using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class GRNNDataPair : BasicMLDataPair, IMLDataPair
    {
        public int EdgeIndex { get; set; }
        public bool Reverse { get; set; }

        public GRNNDataPair(IMLData Input, IMLData Ideal, double significance, int edgeindex, bool reverse) : base(Input, Ideal)
        {
            this.Significance = significance;
            EdgeIndex = edgeindex;
            Reverse = reverse;
        }
    }
}
