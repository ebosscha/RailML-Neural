using Encog.ML.Data;
using Encog.ML.Train;
using RailMLNeural.Data;
using RailMLNeural.Neural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Configurations
{
    [Serializable]
    class ConvolutionalConfiguration : INeuralConfiguration
    {
        #region Members
        public string Name { get; set; }
        public string Description { get; set; }
        public AlgorithmEnum Type { get { return AlgorithmEnum.Convolutional; } }
        public IMLTrain Training { get; set; }
        public NormBuffMLDataSet Data { get { throw new NotImplementedException(); } }
        public NeuralSettings Settings { get; set; }
        public bool IsRunning { get; set; }
        public List<double> ErrorHistory { get; set; }
        public List<double> VerificationHistory { get; set; }
        public List<string> InputMap { get; set; }
        public List<string> OutputMap { get; set; }
        public event EventHandler ProgressChanged;
        #endregion Members

        #region Public

        public void TrainNetwork()
        {
            throw new NotImplementedException();
        }

        public void TrainNetwork(object state)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void StopTraining()
        {

        }

        public IMLData Compute(IMLData Data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion Public

        #region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        #endregion Serialization


        public double RunVerification()
        {
            throw new NotImplementedException();
        }
    }
}
