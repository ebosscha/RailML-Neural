using Encog.MathUtil.Error;
using Encog.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    public interface IErrorCalculation
    {
        void UpdateError(IMLData Output, IMLData Ideal, double Significance);
        double CalculateError();
    }

    public class MSEErrorCalculation : IErrorCalculation
    {
        private ErrorCalculation calc;

        public MSEErrorCalculation()
        {
            calc = new ErrorCalculation();
        }

        public void UpdateError(IMLData Output, IMLData Ideal, double Significance)
        {
            calc.UpdateError(Output, Ideal, Significance);
        }

        public double CalculateError()
        {
            return calc.CalculateMSE();
        }
    }

    public class WeightedMSEErrorCalculation : IErrorCalculation
    {
        private ErrorCalculation calc;
        double[] A;
        double[] B;
        int c = 0;

        public WeightedMSEErrorCalculation(int OutputSize)
        {
            calc = new ErrorCalculation();
            A = new double[OutputSize];
            B = new double[OutputSize];
            c = 0;
        }

        public void UpdateError(IMLData Output, IMLData Ideal, double Significance)
        {
            calc.UpdateError(Output, Ideal, Significance);
            c++;
            for (int i = 0; i < Output.Count; i++)
            {
                A[i] += Ideal[i];
                B[i] += Ideal[i] * Ideal[i];
            }
        }

        public double CalculateError()
        {
            double var = 0;
            for (int i = 0; i < A.Length; i++)
            {
                var += (B[i] / (double)c - Math.Pow(A[i] / (double)c, 2));
            }
            var /= A.Length;

            return calc.CalculateMSE() / (var == 0.0 ? 1 : var);
        }
    }

    public class ThresholdErrorCalculation : IErrorCalculation
    {
        double score = 0;
        int c = 0;
        public ThresholdErrorCalculation()
        {

        }

        public void UpdateError(IMLData Output, IMLData Ideal, double Significance)
        {
            for (int i = 0; i < Output.Count; i++)
            {
                c++;
                if (Math.Abs(Output[i] - Ideal[i]) > 0.1)
                {
                    score++;
                }
            }
        }

        public double CalculateError()
        {
            return score / c;
        }
    }
}
