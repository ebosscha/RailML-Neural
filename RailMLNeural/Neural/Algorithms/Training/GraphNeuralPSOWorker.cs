using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
     /// <summary>
    /// PSO multi-treaded worker.
    /// It allows PSO to offload all of the individual 
    /// particle calculations to a separate thread.
    /// 
    /// Contributed by:
    /// Geoffroy Noel
    /// https://github.com/goffer-looney 
    /// 
    /// </summary>

    [Serializable]
    public class GraphNeuralPSOWorker
    {
        private GraphNeuralPSO m_neuralPSO;
        private int m_particleIndex;
        private bool m_init = false;
        
        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="neuralPSO">the training algorithm</param>
        /// <param name="particleIndex">the index of the particle in the swarm</param>
        /// <param name="init">true for an initialisation iteration </param>
        public GraphNeuralPSOWorker(GraphNeuralPSO neuralPSO, int particleIndex, bool init)
        {
            m_neuralPSO = neuralPSO;
            m_particleIndex = particleIndex;
            m_init = init;
        }

        /// <summary>
        /// Update the particle velocity, position and personal best.
        /// </summary>
        public void Run()
        {
            m_neuralPSO.UpdateParticle(m_particleIndex, m_init);
        }

    }
}

