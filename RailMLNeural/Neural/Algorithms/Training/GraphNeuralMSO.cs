using Encog.MathUtil;
using Encog.MathUtil.Randomize;
using Encog.ML;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Structure;
using Encog.Neural.Networks.Training;
using Encog.Util;
using Encog.Util.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
    class GraphNeuralMSO : BasicTraining, IMultiThreadable, IBatchSize
    {
        protected VectorAlgebra m_va;
        /// <summary>
        /// Maximum change one particle can take during one iteration.
        /// Imposes a limit on the maximum absolute value of the velocity 
        /// components of a particle. 
        /// Affects the granularity of the search.
        /// If too high, particle can fly past optimum solution.
        /// If too low, particle can get stuck in local minima.
        /// Usually set to a fraction of the dynamic range of the search
        /// space (10% was shown to be good for high dimensional problems).
        /// -1 is a special value that represents boundless velocities. 
        /// </summary>
        protected double m_maxVelocity = 2;

        protected double m_maxPosition = -1;

        protected PSOTypeEnum _PSOType = PSOTypeEnum.PSO;

        public PSOTypeEnum PSOType
        {
            get
            {
                return _PSOType;
            }
            set
            {
                _PSOType = value;
            }
        }

        /// <summary>
        /// c1, cognitive learning rate >= 0
        /// tendency to return to personal best position
        /// </summary>
        protected double m_c1 = 2.0;

        /// <summary>
        /// c2, social learning rate >= 0
        /// tendency to move towards the swarm best position
        /// </summary>
        protected double m_c2 = 2.0;

        /// <summary>
        /// Exclusion Rate
        /// </summary>
        protected double m_rexcl = 10;

        /// <summary>
        /// Quantum Cloud Radius
        /// </summary>
        protected double r_cloud = 1;



        /// <summary>
        /// w, inertia weight.
        /// Controls global (higher value) vs local exploration 
        /// of the search space. 
        /// Analogous to temperature in simulated annealing.
        /// Must be chosen carefully or gradually decreased over time.
        /// Value usually between 0 and 1.
        /// </summary>
        protected double m_inertiaWeight = 0.4;

        /// <summary>
        /// If true, the position of the previous global best position 
        /// can be updated *before* the other particles have been modified.
        /// </summary>
        private bool m_pseudoAsynchronousUpdate = false;

        public int SwarmCount { get; set; }

        public int NeutralSwarmPopulation { get; set; }

        public int ChargedSwarmPopulation { get; set; }

        public IMLEncodable Network {get; set;}

        private IRandomizer _randomizer;

        private ICalculateScore _calculateScore;

        private bool InitRun = true;

        private GraphNeuralMSOSwarm[] _swarms;
        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="network">an initialised Encog network. 
        ///                          The networks in the swarm will be created with 
        ///                          the same topology as this network.</param>
        /// <param name="randomizer">any type of Encog network weight initialisation
        ///                          object.</param>
        /// <param name="calculateScore">any type of Encog network scoring/fitness object.</param>
        /// <param name="populationSize">the swarm size.</param>
        public GraphNeuralMSO(IMLEncodable network,
                IRandomizer randomizer, ICalculateScore calculateScore,
                int swarmCount, int populationSize)
            : this(network, randomizer, calculateScore, swarmCount, populationSize, 0)
        {
            
        }

        public GraphNeuralMSO(IMLEncodable network,
                IRandomizer randomizer, ICalculateScore calculateScore,
                int swarmCount, int neutralPopulationSize, int chargedPopulationSize) 
            : base(TrainingImplementationType.Iterative)
        {
            Network = network;
            NeutralSwarmPopulation = neutralPopulationSize;
            ChargedSwarmPopulation = chargedPopulationSize;
            SwarmCount = swarmCount;
            _swarms = new GraphNeuralMSOSwarm[SwarmCount];
            _randomizer = randomizer;
            _calculateScore = calculateScore;
            m_va = new VectorAlgebra();
        }

        private void InitPopulation()
        {
            for (int i = 0; i < _swarms.Length; i++ )
            {
                _swarms[i] = new GraphNeuralMSOSwarm(this, i);
                _swarms[i].Init();
            }
            IterationPSO(true);
            InitRun = false;
        }

        public override void Iteration()
        {
            if (InitRun)
            {
                InitPopulation();
            }
            PreIteration();
            IterationPSO(false);
            PostIteration();
        }

        private void IterationPSO(bool init)
        {
            if(ThreadCount == 1)
            {
                for(int i = 0; i < SwarmCount; i++)
                {
                    _swarms[i].Iteration(init);
                }
            }
            else
            {
                Parallel.For(0, SwarmCount, i => _swarms[i].Iteration(init));
            }

            Error = double.PositiveInfinity;
            int bestindex = 0;
            for (int i = 0; i < SwarmCount; i++ )
            {
                if(IsScoreBetter( _swarms[i].m_bestErrors[_swarms[i].m_bestVectorIndex], Error))
                {
                    Error = _swarms[i].m_bestErrors[_swarms[i].m_bestVectorIndex];
                    bestindex = i;
                }
            }
            Network.DecodeFromArray(_swarms[bestindex].m_bestVectors[_swarms[bestindex].m_bestVectorIndex]);
        }

        private void Exclusion(int swarmIndex)
        {
            if (m_rexcl > 0)
            {
                for (int i = 0; i < SwarmCount; i++)
                {
                    if (i == swarmIndex) { continue; }
                    double[] temp = new double[_swarms[swarmIndex].m_bestVector.Length];
                    m_va.Copy(temp, _swarms[swarmIndex].m_bestVector);
                    m_va.Sub(temp, _swarms[i].m_bestVector);
                    for (int j = 0; j < temp.Length; j++)
                    {
                        temp[j] = temp[j] * temp[j];
                    }
                    double dist = Math.Sqrt(temp.Sum());
                    if (dist < m_rexcl)
                    {
                        if (_swarms[i].m_bestErrors[_swarms[i].m_bestVectorIndex] < _swarms[swarmIndex].m_bestErrors[_swarms[swarmIndex].m_bestVectorIndex])
                        {
                            _swarms[i].Init();
                            //_swarms[i].Iteration(true);
                        }
                        else
                        {
                            _swarms[swarmIndex].Init();
                            //_swarms[swarmIndex].Iteration(true);
                        }
                    }
                    
                }
            }
                
        }

        /// <summary>
        /// Compares two scores. 
        /// </summary>
        /// <param name="score1">a score</param>
        /// <param name="score2">a score</param>
        /// <returns>true if score1 is better than score2</returns>
        bool IsScoreBetter(double score1, double score2)
        {
            return ((_calculateScore.ShouldMinimize && (score1 < score2)) || ((!_calculateScore.ShouldMinimize) && (score1 > score2)));
        }

        /// <summary>
        /// Update the velocity of a particle  
        /// </summary>
        /// <param name="particleIndex">index of the particle in the swarm</param>
        /// <param name="particlePosition">the particle current position vector</param>
        protected double[] UpdatePosition(int swarmIndex, int particleIndex, double[] particlePosition)
        {
            if (particleIndex < NeutralSwarmPopulation || PSOType == PSOTypeEnum.PSO || PSOType == PSOTypeEnum.CPSO)
            {
                int i = particleIndex;
                double[] vtmp = new double[particlePosition.Length];

                // Standard PSO formula

                // inertia weight
                m_va.Mul(_swarms[swarmIndex].m_velocities[i], m_inertiaWeight);

                // cognitive term
                m_va.Copy(vtmp, _swarms[swarmIndex].m_bestVectors[i]);
                m_va.Sub(vtmp, particlePosition);
                MulRand(vtmp, m_c1);
                m_va.Add(_swarms[swarmIndex].m_velocities[i], vtmp);

                // social term
                if (i != _swarms[swarmIndex].m_bestVectorIndex)
                {
                    m_va.Copy(vtmp, m_pseudoAsynchronousUpdate ? _swarms[swarmIndex].m_bestVectors[_swarms[swarmIndex].m_bestVectorIndex] : _swarms[swarmIndex].m_bestVector);
                    m_va.Sub(vtmp, particlePosition);
                    MulRand(vtmp, m_c2);
                    m_va.Add(_swarms[swarmIndex].m_velocities[i], vtmp);
                }

                if (_PSOType == PSOTypeEnum.CPSO)
                {

                }

                // velocity clamping
                m_va.ClampComponents(_swarms[swarmIndex].m_velocities[i], m_maxVelocity);

                // new position (Xt = Xt-1 + Vt)
                m_va.Add(particlePosition, _swarms[swarmIndex].m_velocities[i]);    
            }
            else if(_PSOType == PSOTypeEnum.QSO)
            {
                // Assign the particle a random position within the quantum cloud
                Randomise(particlePosition, r_cloud);
                m_va.Add(particlePosition, _swarms[swarmIndex].m_bestVector);
            }
            // pin the particle against the boundary of the search space.
            // (only for the components exceeding maxPosition)
            m_va.ClampComponents(particlePosition, m_maxPosition);

            return particlePosition;
        }

        /// <summary>
        /// Batch Size used for calculate score object.
        /// </summary>
        public int BatchSize { get; set; }

        public int ThreadCount { get; set; }


        #region Swarm
        /// <summary>
        /// Class describing a single swarm in a multi-swarm configuration.
        /// </summary>
        class GraphNeuralMSOSwarm
        {
            private int index;
            protected VectorAlgebra m_va;
            /// <summary>
            /// Swarm state and memories.
            /// </summary>
            protected IMLEncodable[] m_networks;
            protected IMLEncodable[] m_networks2;
            public double[][] m_velocities;
            public double[][] m_bestVectors;
            public double[] m_bestErrors;
            public int m_bestVectorIndex;
            protected int m_populationSize;

            /// <summary>
            /// Although this is redundant with m_bestVectors[m_bestVectorIndex],
            /// m_bestVectors[m_bestVectorIndex] is not thread safe.
            /// </summary>
            public double[] m_bestVector;
            public double m_bestError;
            IMLEncodable m_bestNetwork = null;

            /// <summary>
            /// The governing multiple swarm method.
            /// </summary>
            private GraphNeuralMSO _owner;

            public GraphNeuralMSOSwarm(GraphNeuralMSO Owner, int i) 
            {
                index = i;
                _owner = Owner;
                m_bestError = double.PositiveInfinity;
                m_populationSize = _owner.NeutralSwarmPopulation + _owner.ChargedSwarmPopulation;
                m_networks = new IMLEncodable[m_populationSize];
                m_networks2 = new IMLEncodable[m_populationSize];
                m_velocities = null;
                m_bestVectors = new double[m_populationSize][];
                m_bestVector = new double[_owner.Network.EncodedArrayLength()];
                m_bestErrors = new double[m_populationSize];
                m_bestVectorIndex = -1;
                m_bestNetwork = _owner.Network;

                m_va = new VectorAlgebra();

            }

            public void Init()
            {
                int dimensionality = m_bestVector.Length;
                m_velocities = EngineArray.AllocateDouble2D(m_populationSize, dimensionality);
                // run an initialisation iteration

                for (int i = 0; i < m_populationSize; i++)
                {
                    if (m_networks[i] == null)
                    {
                        if(_owner.Network is ICloneable)
                        {
                            m_networks[i] = (IMLEncodable)((ICloneable)_owner.Network).Clone();
                        }
                        else if(_owner.Network is BasicNetwork)
                        {
                            m_networks[i] = (IMLEncodable)((BasicNetwork)_owner.Network).Clone();
                        }
                    }
                    if (index > 0 || i > 0)
                    {
                        lock (_owner._randomizer)
                        {
                            double[] d = new double[m_networks[i].EncodedArrayLength()];
                            _owner._randomizer.Randomize(d);
                            m_networks[i].DecodeFromArray(d);
                        }
                    _owner.Randomise(m_velocities[i], _owner.m_maxVelocity); 
                    }
                    m_bestVectors[i] = GetNetworkState(i);
                             
                }
                CheckObjectiveChanged();
            }

            public void Iteration(bool init)
            {
                if (_owner.BatchSize > 0 && !init)
                {
                    CheckObjectiveChanged();
                }

                if (_owner.ThreadCount == 1)
                {
                    for (int i = 0; i < m_populationSize; i++)
                    {
                        HandleParticle(i, init);
                    }
                }
                else
                {
                    Parallel.For(0, m_populationSize, i =>
                    {
                        HandleParticle(i, init);
                    });
                }

                UpdateGlobalBestPosition();
                _owner.Exclusion(index);
            }

            private void CheckObjectiveChanged()
            {
                if (m_bestVectorIndex >= 0)
                {
                    if (m_networks2[m_bestVectorIndex] == null)
                    {
                        if (_owner.Network is ICloneable)
                        {
                            m_networks2[m_bestVectorIndex] = (IMLEncodable)((ICloneable)_owner.Network).Clone();
                        }
                        else if (_owner.Network is BasicNetwork)
                        {
                            m_networks2[m_bestVectorIndex] = (IMLEncodable)((BasicNetwork)_owner.Network).Clone();
                        }
                    }
                    m_networks2[m_bestVectorIndex].DecodeFromArray(m_bestVector);
                    double score = _owner._calculateScore.CalculateScore(m_networks2[m_bestVectorIndex]);
                    if (score != m_bestErrors[m_bestVectorIndex])
                    {
                        m_bestErrors[m_bestVectorIndex] = score;
                        Parallel.For(0, m_populationSize, (i) =>
                        {
                            if (i == m_bestVectorIndex) { return; }
                            RecalculateScore(i);
                        });
                        m_bestVectorIndex = Array.FindIndex(m_bestErrors, x => x == m_bestErrors.Min());
                        m_bestVector = m_bestVectors[m_bestVectorIndex];
                    }
                }

            }

            private void RecalculateScore(int i)
            {
                if (m_networks2[i] == null)
                {
                    if (_owner.Network is ICloneable)
                    {
                        m_networks2[i] = (IMLEncodable)((ICloneable)_owner.Network).Clone();
                    }
                    else if (_owner.Network is BasicNetwork)
                    {
                        m_networks2[i] = (IMLEncodable)((BasicNetwork)_owner.Network).Clone();
                    }
                }
                m_networks2[i].DecodeFromArray(m_bestVectors[i]);
                m_bestErrors[i] = _owner._calculateScore.CalculateScore(m_networks2[i]);
            }

            private void HandleParticle(int i, bool init)
            {
                GraphNeuralMSOWorker worker = new GraphNeuralMSOWorker(this, i, init);
                worker.Run();
            }

            private void UpdateGlobalBestPosition()
            {
                bool bestUpdated = false;
                for (int i = 0; i < m_populationSize; i++)
                {
                    if ((m_bestVectorIndex == -1) || _owner.IsScoreBetter(m_bestErrors[i], m_bestError))
                    {
                        m_bestVectorIndex = i;
                        m_bestError = m_bestErrors[i];
                        bestUpdated = true;
                    }
                }
                if (bestUpdated)
                {
                    m_va.Copy(m_bestVector, m_bestVectors[m_bestVectorIndex]);
                    m_bestNetwork.DecodeFromArray(m_bestVector);
                }
            }

            /// <summary>
            /// Update the velocity, position and personal 
            /// best position of a particle.
            /// </summary>
            /// <param name="particleIndex">index of the particle in the swarm</param>
            /// <param name="init">if true, the position and velocity
            ///                          will be initialised. </param>
            public void UpdateParticle(int particleIndex, bool init)
            {
                int i = particleIndex;
                double[] particlePosition = null;
                if (init)
                {
                    // Create a new particle with random values.
                    // Except the first particle which has the same values 
                    // as the network passed to the algorithm.
                    if (m_networks[i] == null)
                    {
                        if (_owner.Network is ICloneable)
                        {
                            m_networks[i] = (IMLEncodable)((ICloneable)_owner.Network).Clone();
                        }
                        else if (_owner.Network is BasicNetwork)
                        {
                            m_networks[i] = (IMLEncodable)((BasicNetwork)_owner.Network).Clone();
                        }
                        if (i > 0)
                        {
                            lock (_owner._randomizer)
                            {
                                double[] d = new double[m_networks[i].EncodedArrayLength()];
                                _owner._randomizer.Randomize(d);
                                m_networks[i].DecodeFromArray(d);
                            }
                        }
                    }
                    particlePosition = GetNetworkState(i);
                    m_bestVectors[i] = particlePosition;

                    // randomise the velocity
                    _owner.Randomise(m_velocities[i], _owner.m_maxVelocity);
                }
                else
                {
                    particlePosition = GetNetworkState(i);
                    particlePosition = _owner.UpdatePosition(index, i, particlePosition);


                    SetNetworkState(i, particlePosition);
                }
                UpdatePersonalBestPosition(i, particlePosition);
            }

            /// <summary>
            /// Update the personal best position of a particle. 
            /// </summary>
            /// <param name="particleIndex">index of the particle in the swarm</param>
            /// <param name="particlePosition">the particle current position vector</param>
            protected void UpdatePersonalBestPosition(int particleIndex, double[] particlePosition)
            {
                // set the network weights and biases from the vector
                double score = _owner._calculateScore.CalculateScore(m_networks[particleIndex]);

                // update the best vectors (g and i)
                if ((m_bestErrors[particleIndex] == 0) || _owner.IsScoreBetter(score, m_bestErrors[particleIndex]))
                {
                    m_bestErrors[particleIndex] = score;
                    m_va.Copy(m_bestVectors[particleIndex], particlePosition);
                }
            }

            /// <summary>
            /// Returns the state of a network in the swarm  
            /// </summary>
            /// <param name="particleIndex">index of the network in the swarm</param>
            /// <returns>an array of weights and biases for the given network</returns>
            protected double[] GetNetworkState(int particleIndex)
            {
                return NetworkCODEC.NetworkToArray(m_networks[particleIndex]);
            }

            /// <summary>
            /// Sets the state of the networks in the swarm
            /// </summary>
            /// <param name="particleIndex">index of the network in the swarm</param>
            /// <param name="state">an array of weights and biases</param>
            protected void SetNetworkState(int particleIndex, double[] state)
            {
                NetworkCODEC.ArrayToNetwork(state, m_networks[particleIndex]);
            }

            #region Worker
            [Serializable]
            public class GraphNeuralMSOWorker
            {
                private GraphNeuralMSOSwarm m_neuralPSO;
                private int m_particleIndex;
                private bool m_init = false;

                /// <summary>
                /// Constructor. 
                /// </summary>
                /// <param name="neuralPSO">the training algorithm</param>
                /// <param name="particleIndex">the index of the particle in the swarm</param>
                /// <param name="init">true for an initialisation iteration </param>
                public GraphNeuralMSOWorker(GraphNeuralMSOSwarm swarm, int particleIndex, bool init)
                {
                    m_neuralPSO = swarm;
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

            #endregion Worker
        }
        #endregion Swarm

        /// <summary>
        /// Sets the maximum velocity.
        /// </summary>
        public double MaxVelocity
        {
            get
            {
                return m_maxVelocity;
            }
            set
            {
                m_maxVelocity = value;
            }
        }

        /// <summary>
        /// Set the boundary of the search space (Xmax)
        /// </summary>
        public double MaxPosition
        {
            get
            {
                return m_maxPosition;
            }
            set
            {
                m_maxPosition = value;
            }
        }

        /// <summary>
        /// Sets the cognition coefficient (c1).
        /// </summary>
        public double C1
        {
            get
            {
                return m_c1;
            }
            set
            {
                m_c1 = value;
            }
        }


        /// <summary>
        /// Set the social coefficient (c2).
        /// </summary>
        public double C2
        {
            get
            {
                return m_c2;
            }
            set
            {
                m_c2 = value;
            }
        }

        /// <summary>
        /// Get the inertia weight (w) 
        /// </summary>
        public double InertiaWeight
        {
            get
            {
                return m_inertiaWeight;
            }
            set
            {
                m_inertiaWeight = value;
            }
        }

        public double RCloud
        {
            get { return r_cloud; }
            set { r_cloud = value; }
        }

        public double ExclusionRange
        {
            get { return m_rexcl; }
            set { m_rexcl = value; }
        }

        static ThreadLocal<Random> rand = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
        private void Randomise(double[] v, double maxValue)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (2.0 * rand.Value.NextDouble() - 1.0) * maxValue;
            }
        }

        private void MulRand(double[] v, double k)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] *= k * rand.Value.NextDouble();
            }
        }

        #region Inherited
        public override bool CanContinue
        {
            get { throw new NotImplementedException(); }
        }

        public override IMLMethod Method
        {
            get { return Network; }
        }

        public override Encog.Neural.Networks.Training.Propagation.TrainingContinuation Pause()
        {
            throw new NotImplementedException();
        }

        public override void Resume(Encog.Neural.Networks.Training.Propagation.TrainingContinuation state)
        {
            throw new NotImplementedException();
        }

        #endregion Inherited
    }

    public enum PSOTypeEnum
    {
        PSO,
        CPSO,
        QSO
    }
}
