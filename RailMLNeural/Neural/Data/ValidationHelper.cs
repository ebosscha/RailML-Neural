using Encog.ML.Data;
using RailMLNeural.Neural.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class ValidationHelper
    {
        private List<IMLData> _outputs;
        private List<IMLData> _ideals;

        public ValidationHelper()
        {
            _outputs = new List<IMLData>();
            _ideals = new List<IMLData>();
        }

        public void Add(IMLData Output, IMLData Ideal)
        {
            _outputs.Add(Output);
            _ideals.Add(Ideal);
        }

        public void Analyze()
        {

        }

    }
}
