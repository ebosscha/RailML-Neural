using Encog.Neural.Data.Basic;
using Encog.Neural.NeuralData;
using RailML___WPF.NeuralNetwork.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.NeuralNetwork.PreProcessing
{
    class PreProcesser
    {
        double[] inputtemplate;
        double[] idealtemplate;

        public PreProcesser()
        {
            int linecount = Data.DataContainer.model.infrastructure.trackGroups.line.Count;
            idealtemplate = new double[linecount];
            inputtemplate = new double[linecount * 5];
        }
        public INeuralDataSet CreateDayDataSet(DayData data)
        {
            double[][] input = new double[1][];
            double[][] ideal = new double[1][];

            
            
            INeuralDataSet dataset = new BasicNeuralDataSet(input, ideal);

            return dataset;
        }
    }
}
