using Encog.MathUtil.Error;
using Encog.ML;
using Encog.ML.Data;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Util.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    [Serializable]
    class DataSetCalculateScore : ICalculateScore
    {
        private double _error;
        private int n;
        private readonly IMLDataSet _training;
        private IMLRegression meth;
        public DataSetCalculateScore(IMLDataSet Data)
        {
            _training = Data;
        }

        
        public double CalculateScore(IMLMethod method)
        {
            meth = method as IMLRegression;
            _error = 0;
            n = 0;
            var determine = new DetermineWorkload(8, _training.Count);
            Parallel.ForEach(determine.CalculateWorkers(), x => CalculateRange(x.Low, x.High));
            return _error / n;
        }

        public void CalculateRange(int Low, int High)
        {
            BasicNetwork temp = (BasicNetwork)((BasicNetwork)meth).Clone();
            ErrorCalculation calc = new ErrorCalculation();
            IMLDataSet Data = _training.OpenAdditional();
            for(int i = Low; i < High; i++)
            {
                IMLDataPair pair = Data[i];
                IMLData output = temp.Compute(pair.Input);
                calc.UpdateError(output, pair.Ideal, pair.Significance);
            }
            _error += calc.Calculate();
            Interlocked.Increment(ref n);
        }

        public bool ShouldMinimize
        {
            get { return true; }
        }

        public bool RequireSingleThreaded
        {
            get { return false; }
        }
    }
    
}
