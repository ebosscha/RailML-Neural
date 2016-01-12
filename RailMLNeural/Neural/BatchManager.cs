using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RailMLNeural.Neural
{
    static class BatchManager
    {
        #region Variables
        private static List<NeuralNetwork> _queue = new List<NeuralNetwork>();

        public static List<NeuralNetwork> Queue { get { return _queue; } }

        public static event EventHandler QueueChanged;
        #endregion Variables

        #region Public
        /// <summary>
        /// Adds a NeuralNetwork to train to the Queue
        /// </summary>
        /// <param name="N"></param>
        public static void Add(NeuralNetwork N)
        {
            _queue.Add(N);
            OnChanged();
        }

        public static void Remove(NeuralNetwork N)
        {
            if(_queue.Contains(N))
            {
                _queue.Remove(N);
                OnChanged();
            }
        }

        public static void RunBatch()
        {
            while(_queue.Count > 0)
            {
                NeuralNetwork N = _queue[0];
                _queue.RemoveAt(0);
                OnChanged();
                ThreadPool.QueueUserWorkItem(N.TrainNetwork);
            }
        }
        #endregion Public

        #region Private
        private static void OnChanged()
        {
            if(QueueChanged != null)
            {
                QueueChanged(null, EventArgs.Empty);
            }

        }

        #endregion Private
    }
}
