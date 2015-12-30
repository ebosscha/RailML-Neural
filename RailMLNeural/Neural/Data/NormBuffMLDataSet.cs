using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Data.Buffer;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class NormBuffMLDataSet : BufferedMLDataSet
    {
        #region Parameters
        private bool _isNormalized;

        public bool IsNormalized
        {
            get { return _isNormalized; }
        }

        private Normalizer _normalizer;

        public Normalizer Normalizer
        {
            get { return _normalizer; }
        }

        private int _recordCount;
        private FileStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private int RecordSize;
        private const int DoubleSize = sizeof(double);
        private const int HeaderSize = DoubleSize*3;

        #endregion Parameters

        #region Public
        public NormBuffMLDataSet(string binaryfile) : base(binaryfile)
        {

        }

        public void Normalize()
        {
            this.Close();
            _stream = new FileStream(BinaryFile, FileMode.Open, FileAccess.ReadWrite);
            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);
            RecordSize = (InputSize + IdealSize + 1 ) * DoubleSize;

            for(int i = 0; i < EGB.RecordCount; i++)
            {
                IMLDataPair newpair = Normalizer.Normalize(ReadPair(i));
                Replace(newpair, i);
            }
        }

        #endregion Public
        
        #region Private
        private void Replace(IMLDataPair pair, int row)
        {
            SetLocation(row);
            Write(pair.Input);
            Write(pair.Ideal);
        }

        private void Replace(IMLData data, int row)
        {
            EGB.SetLocation(row);
            EGB.Write(data);
        }

        private long CalculateIndex(long row)
        {
            return (long)HeaderSize + (row * (long)RecordSize);
        }
        
        private void SetLocation(int row)
        {
            _stream.Position = CalculateIndex(row);
        }

        private void Write(IMLData v)
        {
            for(int i = 0; i < v.Count; i++)
            {
                _writer.Write(v[i]);
            }
        }

        private IMLDataPair ReadPair(int row)
        {
            var input = new double[InputSize];
            var ideal = new double[IdealSize];

            SetLocation(row);
            for(int i = 0; i < InputSize; i++)
            {
                input[i] = _reader.ReadDouble();
            }
            for (int i = 0; i < IdealSize; i++)
            {
                ideal[i] = _reader.ReadDouble();
            }

            var inputData = new BasicMLData(input);
            var idealData = new BasicMLData(ideal);

            return new BasicMLDataPair(inputData, idealData);
        }


        #endregion Private

        #region Override

        #endregion Override
        public new void Add(IMLData data1)
        {
            base.Add(data1);
            Normalizer.Add(new BasicMLDataPair(data1));
            _recordCount++;
        }

        public new void Add(IMLData inputData, IMLData idealData)
        {
            base.Add(inputData, idealData);
            Normalizer.Add(new BasicMLDataPair(inputData, idealData));
            _recordCount++;
        }

        public new void Add(IMLDataPair pair)
        {
            base.Add(pair);
            Normalizer.Add(pair);
            _recordCount++;
        }

        public new void BeginLoad(int inputSize, int idealSize)
        {
            base.BeginLoad(inputSize, idealSize);
            _normalizer = new Normalizer(inputSize, idealSize);
        }



    }
}
