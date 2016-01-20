using Encog.ML.Data;
using RailMLNeural.Neural.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class RecurrentDataSet : IMLDataSet
    {
        private RecurrentConfiguration _owner;
        public int Count { get; set; }
        public int IdealSize { get; set; }
        public int InputSize { get; set; }
        public bool Supervised { get; set; }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IMLDataPair> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IMLDataSet OpenAdditional()
        {
            throw new NotImplementedException();
        }

        public IMLDataPair this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    
    }
}
