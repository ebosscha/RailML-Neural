using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks.Training.Propagation;
using RailMLNeural.Data;
using RailMLNeural.Neural.Configurations;
using RailMLNeural.Neural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    class RecurrentTraining : IMLTrain
    {
        #region Parameters

        private RecurrentConfiguration _owner;

        public bool TrainingDone { get; private set; }

        public double Error { get; set; }

        public int IterationNumber { get; set; }

        public bool CanContinue { get; private set; } 

        #endregion Parameters

        #region Public
        /// <summary>
        /// Constructs a new instance of the RecurrentTraining Class
        /// </summary>
        /// <param name="Network"></param>
        /// <param name="Provider"></param>
        public RecurrentTraining(RecurrentConfiguration Owner)
        {
            _owner = Owner;
        }

        public void Iteration()
        {
            int n = 0;
            while (n < DataContainer.DelayCombinations.dict.Sum(x => x.Value.Count))
            {

                n++;
            }

        }

        public void Iteration(int count)
        {

        }

        

        #endregion Public

        #region Private

        #endregion Private

        #region NotImplemented
        /// <summary>
        /// Collection of not implemented interface members and methods. 
        /// </summary>
        public IMLDataSet Training { get { throw new NotImplementedException(); } }

        public IList<IStrategy> Strategies { get { throw new NotImplementedException(); } }

        public TrainingImplementationType ImplementationType { get { throw new NotImplementedException(); } }

        public IMLMethod Method { get { throw new NotImplementedException(); } }

        public void FinishTraining()
        {
            throw new NotImplementedException();
        }

        public TrainingContinuation Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume(TrainingContinuation state)
        {
            throw new NotImplementedException();
        }

        public void AddStrategy(IStrategy strategy)
        {
            throw new NotImplementedException();
        }
        #endregion NotImplemented


    }
}
