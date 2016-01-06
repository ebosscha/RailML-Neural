using Encog.ML.Data;
using Encog.ML.Data.Buffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class BufferedSubDataSet : IMLDataSet
    {
        #region Parameters
        private IMLDataSet _owner;
        private int _startIndex;
        private int _endIndex;

        public int Count
        {
            get
            {
                return _endIndex - _startIndex;
            }
        }

        public int IdealSize
        {
            get
            { return _owner.IdealSize; }
        }

        public int InputSize
        {
            get
            { return _owner.InputSize; }
        }

        public bool Supervised
        {
            get
            {
                return _owner.Supervised;
            }
        }

        #endregion Parameters

        #region Public
        public BufferedSubDataSet(IMLDataSet owner, int startIndex, int endIndex)
        {
            _owner = owner;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }

        public IMLDataPair this[int x]
        {
            get
            {
                return _owner[x + _startIndex];
            }
        }

        public IEnumerator<IMLDataPair> GetEnumerator()
        {
            return new SubDataSetEnumerator(this);
        }

        public void Close()
        {

        }

        public IMLDataSet OpenAdditional()
        {
            return null;
        }
        #endregion Public
        

        #region Private

        #endregion Private


    }

    class SubDataSetEnumerator : IEnumerator<IMLDataPair>
    {
        private readonly BufferedSubDataSet _owner;

        private int _current;

        private IMLDataPair _currentRecord;

        public SubDataSetEnumerator(BufferedSubDataSet owner)
        {
            _owner = owner;
            _current = 0;
        }

        public IMLDataPair Current
        {
            get { return _currentRecord; }
        }

        public void Dispose()
        {

        }

        object IEnumerator.Current
        {
            get
            {
                if(_currentRecord == null)
                {
                    throw new IMLDataError("Can't read current record until MoveNext is called once");
                }
                return _currentRecord;
            }
        }

        public bool MoveNext()
        {
            try
            {
                if (_current >= _owner.Count)
                    return false;

                _currentRecord = _owner[_current++];
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void Reset()
        {
            _current = 0;
        }

        public void Close()
        {

        }
    }
}
