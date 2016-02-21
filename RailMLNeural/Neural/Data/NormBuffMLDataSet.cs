using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Data.Buffer;
using RailMLNeural.Neural.Normalization;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    [Serializable]
    public class NormBuffMLDataSet : BufferedMLDataSet
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
        private int _inputSize;
        private int _idealSize;
        private int _trainingCount
        { 
            get
            { return _recordCount - _verificationCount; }
        }
        private int _verificationCount;

        public new int Count
        {
            get
            {
                return _trainingCount;
            }
        }

        public int VerificationCount
        {
            get
            {
                return _verificationCount;
            }
        }

        #endregion Parameters

        #region Public
        public NormBuffMLDataSet(string binaryfile) : base(binaryfile)
        {
            _recordCount = EGB.NumberOfRecords;
        }

        public void Normalize(List<IDataProvider> inputproviders, List<IDataProvider> outputproviders)
        {
            this.TryEndLoad();
            this.TryOpen();
            Normalizer.Analyze(this);
            EGB.Close();
            Normalizer.Generate(inputproviders, outputproviders);
            Normalize();
        }

        public void Normalize(List<IRecurrentDataProvider> inputproviders, List<IRecurrentDataProvider> outputproviders)
        {
            this.TryEndLoad();
            this.TryOpen();
            Normalizer.Analyze(this);
            EGB.Close();
            Normalizer.Generate(inputproviders, outputproviders);
            Normalize();
        }

        private void Normalize()
        {
            _stream = new FileStream(BinaryFile, FileMode.Open, FileAccess.ReadWrite);
            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);
            RecordSize = (_inputSize + _idealSize + 1 ) * DoubleSize;

            for(int i = 0; i < _recordCount; i++)
            {
                IMLDataPair newpair = Normalizer.Normalize(ReadPair(i));
                Replace(newpair, i);
            }
            _isNormalized = true;
            _writer.Close();
            _writer = null;
            _reader.Close();
            _reader = null;
            _stream.Close();
            _stream = null;

        }

        public void Divide(double verificationPart)
        {
            SplitData(verificationPart);
        }

        /// <summary>
        /// Method that returns a sub Dataset that reads from the same file. Starts from index after trainingcount, and ends at endfile.
        /// </summary>
        /// <returns></returns>
        public IMLDataSet VerificationDataSet()
        {
            var result = new BufferedSubDataSet(this, _trainingCount, _recordCount);
            return result;
        }

        public void TryOpen()
        {
            try
            {
                this.Open();
            }
            catch { }
        }

        public void TryEndLoad()
        {
            try
            {
                this.EndLoad();
            }
            catch { }
        }
        #endregion Public
        
        #region Private
        private void Replace(IMLDataPair pair, int row)
        {
            SetLocation(row);
            Write(pair.Input);
            Write(pair.Ideal);
            _writer.Write(1.0);
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
            var input = new double[_inputSize];
            var ideal = new double[_idealSize];

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

        public void Add(IMLDataSet Set)
        {
            foreach(IMLDataPair pair in Set)
            {
                Add(pair);
            }
        }

        public new void BeginLoad(int inputSize, int idealSize)
        {
            base.BeginLoad(inputSize, idealSize);
            _recordCount = 0;
            _normalizer = new Normalizer(inputSize, idealSize);
            _inputSize = inputSize;
            _idealSize = idealSize;
        }

        public new void Open()
        {
            base.Open();
            _recordCount = EGB.NumberOfRecords;
        }

        #region SplitData

        private void SplitData(double verificationsize)
        {
            _verificationCount = (int)(_recordCount * verificationsize);
            Shuffle();
        }

        /// <summary>
        /// Implementation of Fisher-Yates shuffle algorithm to shuffle the dataset.
        /// </summary>
        private void Shuffle()
        {
            EGB.Close();
            _stream = new FileStream(BinaryFile, FileMode.Open, FileAccess.ReadWrite);
            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);

            Random rand = new Random();
            int n = _recordCount;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                IMLDataPair value = ReadPair(k);
                Replace(ReadPair(n), k);
                Replace(value, n);
            }

            _writer.Close();
            _writer = null;
            _reader.Close();
            _reader = null;
            _stream.Close();
            _stream = null;
        }

        
        #endregion SplitData
    }
}
