using Encog.Engine.Network.Activation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encog.Engine.Network.Activation
{
    [Serializable]
    class ActivationSteepFuzzyReLu : IActivationFunction
    {
        private readonly double[] _paras;

        public ActivationSteepFuzzyReLu()
        {
            _paras = new double[0];
        }

        public void ActivationFunction(double[] d, int start, int size)
        {
            //double sum = 0;
            for (int i = start; i < start + size; i++)
            {
                if (d[i] > 0)
                {
                    d[i] = d[i];
                }
                else
                {
                    d[i] = 0;
                }
                //sum += d[i];
            }
            //for(int i = start; i < start + size; i++)
            //{
            //    d[i] = d[i]/sum;  
            //}

        }

        public double DerivativeFunction(double b, double a)
        {
            if(b <= 0)
            {
                return 0.5;
            }
            else
            {
                return 1;
            } 
        }
        

        public bool HasDerivative
        {
            get { return true; }
        }

        public string[] ParamNames
        {
            get
            {
                string[] result = { };
                return result;
            }
        }

        public double[] Params
        {
            get { return _paras; }
        }

        public object Clone()
        {
            return new ActivationFuzzyReLu();
        }
    }
}
