using Encog.ML.Data;
using RailMLNeural.Neural.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Configurations
{
    class RecurrentConfiguration : INeuralConfiguration
    {
        #region Parameters
        public RecurrentNetwork Network { get; set; }
        public AlgorithmEnum Type { get { return AlgorithmEnum.Recurrent; } }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; private set; }
        public List<double> ErrorHistory { get; set; }
        public List<double> VerificationHistory { get; set; }
        public event EventHandler ProgressChanged;
        #endregion Parameters

        #region Public
        /// <summary>
        /// Constructs a new Recurrent Configuration
        /// </summary>
        public RecurrentConfiguration()
        {
            ErrorHistory = new List<double>();
            VerificationHistory = new List<double>();
        }

        public void TrainNetwork(object state)
        {

        }

        public IMLData Compute(IMLData Data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }


        #endregion Public

        #region Private

        #endregion Private


    }
}
