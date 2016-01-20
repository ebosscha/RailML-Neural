using Encog.ML;
using Encog.Neural.Networks.Training;
using RailMLNeural.Data;
using RailMLNeural.Neural.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
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
                return true;
            }
        }

        private RecurrentConfiguration _owner;

        public RecurrentCalculateScore(RecurrentConfiguration Owner)
        {
            _owner = Owner;
        }

        public double CalculateScore(IMLMethod Network)
        {
            double score = 0;
            int n = 1;
            foreach(var l in DataContainer.DelayCombinations.dict.Values.Where(x => x != null && x.Count > 0))
            {
                foreach(var dc in l)
                {
                    _owner.Graph.GenerateGraph(dc);
                    _owner.Propagator.NewCycle(dc);
                    score = (score * n + _owner.Network.CalculateError()) / (n + 1);
                    n++;
                }
            }
            return score;
        }
    }
}
