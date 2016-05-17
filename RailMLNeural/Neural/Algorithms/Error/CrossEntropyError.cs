using Encog.Neural.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Error
{
    class CrossEntropyError : IErrorFunction
    {
        void IErrorFunction.CalculateError(Encog.ML.Data.IMLData ideal, double[] actual, double[] error)
        {
            for(int i = 0; i < actual.Length; i++)
            {
                error[i] = (actual[i] - ideal[i]) / ((actual[i] - 1) * actual[i]);
            }
        }
    }
}
