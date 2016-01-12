using Encog.ML.Data;
using Encog.ML.Data.Basic;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Normalization
{
    [Serializable]
    public class Normalizer
    {

        #region Parameters

        private int _inputSize;
        private int _idealSize;
        private double _min = -0.8;
        private double _max = 0.8;
        private int n = 0;

        private List<double> _minValues = new List<double>();
        private List<double> _maxValues = new List<double>();
        private List<double> _mean = new List<double>();
        List<Func<double, double>> NormalizationFunctions = new List<Func<double, double>>();
        List<Func<double, double>> DeNormalizationFunctions = new List<Func<double, double>>();

        #endregion Parameters

        #region Public
        public Normalizer(int inputSize, int idealSize)
        {
            _inputSize = inputSize;
            _idealSize = idealSize;
        }

        public void Generate(List<IDataProvider> inputproviders, List<IDataProvider> outputproviders)
        {
            NormalizationFunctions.Clear();
            DeNormalizationFunctions.Clear();
            var AllProviders = new List<IDataProvider>(inputproviders.Count + outputproviders.Count);
            AllProviders.AddRange(inputproviders);
            AllProviders.AddRange(outputproviders);
            int i = 0;
            foreach (IDataProvider provider in AllProviders)
            {
                for (int n = 0; n < provider.Size; n++)
                {
                    switch (provider.NormalizationType)
                    {
                        case NormalizationTypeEnum.Linear:
                            GenerateLinear(i);
                            break;
                        case NormalizationTypeEnum.Quadratic:
                            GenerateQuadratic(i);
                            break;
                        case NormalizationTypeEnum.None:
                            GenerateNone(i);
                            break;
                        default:
                            break;
                    }
                    i++;
                }
            }
        }

        public void Add(IMLDataPair pair)
        {

            if (_minValues.Count == 0)
            {
                n++;
                for (int i = 0; i < _inputSize + _idealSize; i++)
                {
                    if (i < _inputSize)
                    {
                        _minValues.Add(pair.Input[i]);
                        _maxValues.Add(pair.Input[i]);
                        _mean.Add(pair.Input[i]);
                    }
                    else
                    {
                        _minValues.Add(pair.Ideal[i - _inputSize]);
                        _maxValues.Add(pair.Ideal[i - _inputSize]);
                        _mean.Add(pair.Ideal[i - _inputSize]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _inputSize + _idealSize; i++)
                {
                    if (i < _inputSize)
                    {
                        _minValues[i] = Math.Min(_minValues[i], pair.Input[i]);
                        _maxValues[i] = Math.Max(_maxValues[i], pair.Input[i]);
                        _mean[i] = (_mean[i] * (double)n + pair.Input[i]) / ((double)n + 1);
                    }
                    else
                    {
                        _minValues[i] = Math.Min(_minValues[i], pair.Ideal[i - _inputSize]);
                        _maxValues[i] = Math.Max(_maxValues[i], pair.Ideal[i - _inputSize]);
                        _mean[i] = (_mean[i] * (double)n + pair.Ideal[i - _inputSize]) / ((double)n + 1);
                    }
                }
                n++;
            }
        }

        public IMLDataPair Normalize(IMLDataPair pair)
        {
            double[] input = new double[pair.Input.Count];
            double[] ideal = new double[pair.Ideal.Count];
            for(int i = 0; i<_inputSize + _idealSize; i++)
            {
                // Normalize value to range [_min, _max]
                if(i < _inputSize)
                {
                    input[i] = NormalizationFunctions[i].Invoke(pair.Input[i]);
                }
                else
                {
                    ideal[i - _inputSize] = NormalizationFunctions[i].Invoke(pair.Ideal[i - _inputSize]);
                }
            }
            BasicMLData inputData = new BasicMLData(input);
            BasicMLData idealData = new BasicMLData(ideal);
            return new BasicMLDataPair(inputData, idealData);
        }

        public IMLData Normalize(IMLData data, bool IsInput)
        {
            var v = new double[data.Count];
            int pos = 0;
            if(!IsInput) {pos = _inputSize;}
            for(int i = 0; i < data.Count; i++)
            {
                // Normalize Data to range [_min, _max]
                v[i] = NormalizationFunctions[i + pos].Invoke(data[i]);
            }
            return new BasicMLData(v);
        }


        public BasicMLData DeNormalize(IMLData data, bool IsInput)
        {
            int pos = 0;
            if (!IsInput) { pos = _inputSize; } // Adjust position to read from output columns
            double[] output = new double[data.Count];
            for (int i = 0; i < data.Count; i++ )
            {
                // DeNormalize value to real value
                output[i] = DeNormalizationFunctions[i + pos].Invoke(data[i]);
            }
            return new BasicMLData(output);
        }
    
        #endregion Public

        #region Private

        private double Diff(double minvalue, double maxvalue)
        {
            if (minvalue == maxvalue)
            {
                return 1.0;
            }
            return maxvalue - minvalue;
        }

        /// <summary>
        /// Generates Linear normalization and denormalization functions based on _minvalue, _maxvalue and scales to [_min, _max]
        /// </summary>
        private void GenerateLinear(int i)
        {
            double maxvalue = _maxValues[i];
            double minvalue = _minValues[i];
            NormalizationFunctions.Add((x) => LinearNormalizer(x, minvalue, maxvalue, _min, _max));
            DeNormalizationFunctions.Add((x) => LinearDeNormalizer(x, minvalue, maxvalue, _min, _max));           
        }

        /// <summary>
        /// Generates Quadratic normalization and denormalization functions fitting [_minvalue, _min], [_mean, 0] and [__maxvalue, _max]
        /// </summary>
        private void GenerateQuadratic(int i)
        {
            double maxvalue = _maxValues[i];
            double minvalue = _minValues[i];
            double denom = (_min - 0.0) * (_min - _max) * (0.0 - _max);
            double A = (_max * (_mean[i] - _minValues[i]) + 0.0 * (_minValues[i] - _maxValues[i]) + _min * (_maxValues[i] - _mean[i])) / denom;
            double B = (_max * _max * (_minValues[i] - _mean[i]) + 0.0 * 0.0 * (_maxValues[i] - _minValues[i]) + _min * _min * (_mean[i] - _maxValues[i])) / denom;
            double C = (0.0 * _max * (0.0 - _max) * _minValues[i] + _max * _min * (_max - _min) * _mean[i] + _min * 0.0 * (_min - 0.0) * _maxValues[i]) / denom;


            if (double.IsNaN(A) || double.IsNaN(B) || double.IsNaN(C) ||
                (A == 0 && B == 0 && C == 0))
            {
                NormalizationFunctions.Add((x) => 0.0);
                DeNormalizationFunctions.Add((x) => 0.0);
            }
            else
            {
                NormalizationFunctions.Add((x) => QuadraticNormalizer(x, A, B, C));
                DeNormalizationFunctions.Add((x) => QuadraticDeNormalizer(x, A, B, C, minvalue, maxvalue));
            }
        }

        private double LinearNormalizer(double value, double minvalue, double maxvalue, double minrange, double maxrange)
        {
            return ((value - minvalue) * (maxrange - minrange)) / Diff(minvalue, maxvalue) + minrange;
        }


        private double LinearDeNormalizer(double value, double minvalue, double maxvalue, double minrange, double maxrange)
        {
            return (value + minrange) * Diff(minvalue, maxvalue) / (maxrange - minrange) + minvalue;
        }

        private void GenerateNone(int i)
        {
            NormalizationFunctions.Add((x) => x);
            DeNormalizationFunctions.Add((x) => x);
        }
        
        /// <summary>
        /// Normalization function based on the AX^2 + BX + C Quadratic format fitted to the data.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        private double QuadraticNormalizer(double value, double A, double B, double C)
        {
            return A * Math.Pow(value, 2) + B * value + C;
        }

        /// <summary>
        /// Uses ABC formula to recalculate the original value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="minvalue"></param>
        /// <param name="maxvalue"></param>
        /// <returns></returns>
        private double QuadraticDeNormalizer(double value, double A, double B, double C, double minvalue, double maxvalue)
        {
            double D = Math.Sqrt(Math.Pow(B, 2) - 4 * A * (C - value)) / (2 * A);
            double y1 = (-B/(2*A)) + D;
            if (y1 >= minvalue && y1 <= maxvalue) { return y1; }
            else 
            {
                return (-B/(2*A)) - D;
            }

        }
        #endregion Private

    }

    public enum NormalizationTypeEnum
    {
        Linear,
        Quadratic,
        None
    }
}
