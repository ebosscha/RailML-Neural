//using Encog.ML.Data;
//using Encog.ML.Data.Basic;
//using Encog.ML.Data.Buffer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RailMLNeural.Neural.Normalization
//{
//    [Serializable]
//    public class NormalizationHelper
//    {
//        #region Parameters
//        private Normalizer _inputNormalizer;

//        private Normalizer _outputNormalizer;

//        #endregion Parameters

//        #region Public
//        public NormalizationHelper(double InputLower, double InputUpper, double OutputLower, double OutputUpper)
//        {
//            _inputNormalizer = new Normalizer(InputLower, InputUpper);
//            _outputNormalizer = new Normalizer(OutputLower, OutputUpper);
//        }

        

//        public BasicMLDataPair Normalize(IMLDataPair data)
//        {
//            BasicMLData input = new BasicMLData(_inputNormalizer.Normalize(data.Input));
//            BasicMLData output = new BasicMLData(_outputNormalizer.Normalize(data.Ideal));
//            return new BasicMLDataPair(input, output);
//        }

//        public void Normalize(BasicMLDataSet data)
//        {
//            for (int i = 0; i < data.Count; i++ )
//            {
//                data.Data[i] = Normalize(data[i]);
//            }
//        }

//        public BasicMLData Normalize(BasicMLData data, bool IsInput)
//        {
//            if(IsInput)
//            {
//                return new BasicMLData(_inputNormalizer.Normalize(data));
//            }
//            else
//            {
//                return new BasicMLData(_outputNormalizer.Normalize(data));
//            }
//        }

//        public IMLData DeNormalize(IMLData data, bool IsOutput)
//        {
//            if(IsOutput)
//            {
//                return new BasicMLData(_outputNormalizer.DeNormalize(data));
//            }
//            else
//            {
//                return new BasicMLData(_inputNormalizer.DeNormalize(data));
//            }
//        }

//        #endregion Public

//        #region Private

//        #endregion Private
//    }

//    class Normalizer
//    {
//        #region Parameters
//        private double _upper;
//        private double _lower;

//        #endregion Parameters

//        #region Public
//        public Normalizer(double Lower, double Upper)
//        {
//            _upper = Upper;
//            _lower = Lower;
//        }

//        public double[] Normalize(IMLData data)
//        {
//            double[] result = new double[data.Count];
//            for (int i = 0; i < data.Count; i++ )
//            {
//                result[i] = (data[i] - _lower) / (_upper - _lower);
//            }
//            return result;
//        }

//        public double[] DeNormalize(IMLData data)
//        {
//            double[] result = new double[data.Count];
//            for (int i = 0; i < data.Count; i++ )
//            {
//                result[i] = (data[i] * (_upper - _lower)) + _lower ; 
//            }
//            return result;
//        }
//        #endregion Public

//        #region Private

//        #endregion Private

//    }
//}
