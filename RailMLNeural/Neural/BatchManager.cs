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
        private static Queue<NeuralNetwork> _queue { get; set; }
        #endregion Variables

        #region Public
        /// <summary>
        /// Adds a NeuralNetwork to train to the Queue
        /// </summary>
        /// <param name="N"></param>
        public static void Add(NeuralNetwork N)
        {
            _queue.Enqueue(N);
        }

        public static void RunBatch()
        {
            while(_queue.Count > 0)
            {
                NeuralNetwork N = _queue.Dequeue();
                ThreadPool.QueueUserWorkItem(N.RunNetwork);
            }
        }
        #endregion Public

    }
}
