using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks.Training.Propagation;
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

        private RecurrentNetwork _network;

        private RecurrentDataProvider _dataProvider;

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
        public RecurrentTraining(RecurrentNetwork Network, RecurrentDataProvider Provider)
        {
            _network = Network;
            _dataProvider = Provider;
        }

        public void Iteration()
        {

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
