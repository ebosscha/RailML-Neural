using Encog.ML.Data;
using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Normalization
{
    class Normalizer
    {

        #region Parameters
        private int _inputSize;
        private int _idealSize;
        private double _min = -0.8;
        private double _max = 0.8;

        private List<double> _minValues = new List<double>();
        private List<double> _maxValues = new List<double>();

        #endregion Parameters

        #region Public
        public Normalizer(int inputSize, int idealSize)
        {
            _inputSize = inputSize;
            _idealSize = idealSize;
        }

        public void Add(IMLDataPair pair)
        {
            if(_minValues.Count == 0)
            {
                for(int i = 0; i < _inputSize + _idealSize; i++)
                {
                    if(i < _inputSize)
                    {
                        _minValues.Add(pair.Input[i]);
                        _maxValues.Add(pair.Input[i]);
                    }
                    else
                    {
                        _minValues.Add(pair.Ideal[i-_inputSize]);
                        _maxValues.Add(pair.Ideal[i-_inputSize]);
                    }
                }
            }
            for(int i = 0; i < _inputSize + _idealSize; i++)
            {
                if(i<_inputSize)
                {
                    _minValues[i] = Math.Min(_minValues[i], pair.Input[i]);
                    _maxValues[i] = Math.Max(_maxValues[i], pair.Input[i]);
                }
                else
                {
                    _minValues[i] = Math.Min(_minValues[i], pair.Ideal[i-_inputSize]);
                    _maxValues[i] = Math.Max(_maxValues[i], pair.Ideal[i-_inputSize]);
                }
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
                    input[i] = ((pair.Input[i] - _minValues[i]) * (_max - _min) / (_maxValues[i] - _minValues[i])) + _min;
                }
                else
                {
                    ideal[i-_inputSize] = ((pair.Ideal[i-_inputSize] - _minValues[i]) * (_max - _min) / (_maxValues[i] - _minValues[i])) + _min;
                }
            }
            BasicMLData inputData = new BasicMLData(input);
            BasicMLData idealData = new BasicMLData(ideal);
            return new BasicMLDataPair(inputData, idealData);
        }

        public BasicMLData DeNormalize(IMLData data, bool IsInput)
        {
            int pos = 0;
            if (!IsInput) { pos = _inputSize; } // Adjust position to read from output columns
            double[] output = new double[data.Count];
            for (int i = 0; i < data.Count; i++ )
            {
                // DeNormalize value to real value
                output[i] = ((data[i] - _min) * (_maxValues[i + pos] - _minValues[i + pos]) / (_max - _min)) + _minValues[i + pos];
            }
            return new BasicMLData(output);
        }
    
        #endregion Public

        #region Private

        #endregion Private


    }
}
