using Encog.ML.Genetic;
using Encog.Neural.Networks.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Training
{
    class BatchSizeGeneticAlgorithm : MLMethodGeneticAlgorithm, IBatchSize
    {
        public int BatchSize { get; set; }

        public BatchSizeGeneticAlgorithm(MLMethodGenomeFactory.CreateMethod phenotypeFactory,
                ICalculateScore calculateScore, int populationSize) : base(phenotypeFactory, calculateScore, populationSize)
        {

        }
    }
}
