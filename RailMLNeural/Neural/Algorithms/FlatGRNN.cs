using Encog.Engine.Network.Activation;
using Encog.MathUtil.Randomize;
using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Flat;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Util;
using RailMLNeural.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms
{
    [Serializable]
    public class FlatGRNN : IMLMethod, IMLEncodable, ICloneable
    {
        #region Parameters
        public List<FlatGRNNVertex> Vertices { get; set; }
        public List<FlatGRNNEdge> Edges { get; set; }

        public FlatNetwork EdgeNetwork { get; set; }
        public FlatNetwork VertexNetwork { get; set; }

        public BasicNetwork SerializationEdgeNetwork { get; set; }
        public BasicNetwork SerializationVertexNetwork { get; set; }
        public int SerializationEdgeToVertexLayerIndex { get; set; }

        public double[] OriginWeights { get; set; }
        public double[] DestinationWeights { get; set; }

        public double ConnectionLimit { get; set; }
        public bool Limited { get { return ConnectionLimit != 0.0; } }

        public int WeightCount
        {
            get
            {
                return EdgeNetwork.Weights.Length + VertexNetwork.Weights.Length + VertexNetwork.LayerFeedCounts[1] * VertexNetwork.LayerCounts[0];
            }
        }

        public int OutputSize { get; set; }
        public int InputSize { get; set; }
        public int EdgeInputSize { get; set; }
        public int EdgeToVertexFeedIndex { get; set; }
        public double[] EdgeLayerOutputTemplate { get; set; }
        public double[] VertexLayerOutputTemplate { get; set; }

        public FlatNetwork OriginNetwork 
        { 
            get 
            {
                FlatNetwork result = new FlatNetwork();
                VertexNetwork.CloneFlatNetwork(result);
                EngineArray.ArrayCopy(VertexNetwork.Weights, result.Weights);
                result.SetInputWeights(OriginWeights);
                return result;
            } 
        }

        public FlatNetwork DestinationNetwork
        {
            get
            {
                FlatNetwork result = new FlatNetwork();
                VertexNetwork.CloneFlatNetwork(result);
                EngineArray.ArrayCopy(VertexNetwork.Weights, result.Weights);
                result.SetInputWeights(DestinationWeights);
                return result;
            }
        }
        #endregion Parameters

        #region Public
        public FlatGRNN()
        {
            Vertices = new List<FlatGRNNVertex>();
            Edges = new List<FlatGRNNEdge>();
        }

        public void ResetWeights(int hi = 1, int lo = -1, int? seed = null)
        {
            BasicRandomizer rand = new RangeRandomizer(lo, hi);
            rand.Randomize(VertexNetwork.Weights);
            rand.Randomize(EdgeNetwork.Weights);
            rand.Randomize(DestinationWeights);
            rand.Randomize(OriginWeights);
        }

        public void ClearContexts()
        {
            foreach (FlatGRNNEdge edge in Edges)
            {
                EngineArray.ArrayCopy(EdgeLayerOutputTemplate, edge._layerOutputs);
                EngineArray.ArrayCopy(EdgeLayerOutputTemplate, edge._previousOutputs);
            }
            foreach (FlatGRNNVertex vertex in Vertices)
            {
                EngineArray.ArrayCopy(VertexLayerOutputTemplate, vertex._layerOutputs);
                EngineArray.ArrayCopy(VertexLayerOutputTemplate, vertex._previousOutputs);
                vertex._layerSums = new double[vertex._layerSums.Length];
                vertex._previousSums = new double[vertex._previousSums.Length];
            }
        }

        public void Init(SimplifiedGraph Graph, BasicNetwork EdgeBasicNetwork, BasicNetwork VertexBasicNetwork, int EdgeToVertexFeedLayerIndex)
        {
            SerializationEdgeNetwork = EdgeBasicNetwork;
            SerializationVertexNetwork = VertexBasicNetwork;
            SerializationEdgeToVertexLayerIndex = EdgeToVertexFeedLayerIndex;
            EdgeNetwork = EdgeBasicNetwork.Flat;
            VertexNetwork = VertexBasicNetwork.Flat;
            EdgeToVertexFeedIndex = EdgeBasicNetwork.LayerCount - EdgeToVertexFeedLayerIndex - 1;
            InputSize = EdgeBasicNetwork.InputCount - 2 * VertexBasicNetwork.OutputCount;
            OutputSize = EdgeBasicNetwork.OutputCount;
            EdgeInputSize = EdgeBasicNetwork.InputCount;
            EdgeLayerOutputTemplate = new double[EdgeNetwork.LayerOutput.Length];
            VertexLayerOutputTemplate = new double[VertexNetwork.LayerOutput.Length];
            EngineArray.ArrayCopy(EdgeNetwork.LayerOutput, EdgeLayerOutputTemplate);
            EngineArray.ArrayCopy(VertexNetwork.LayerOutput, VertexLayerOutputTemplate);

            OriginWeights = new double[VertexNetwork.LayerFeedCounts[VertexNetwork.LayerFeedCounts.Length - 2] * (VertexNetwork.InputCount)];
            DestinationWeights = new double[VertexNetwork.LayerFeedCounts[VertexNetwork.LayerFeedCounts.Length - 2] * (VertexNetwork.InputCount)];
            foreach(var vertex in Graph.Vertices)
            {
                Vertices.Add(new FlatGRNNVertex(this));
            }

            foreach(var edge in Graph.Edges)
            {
                FlatGRNNEdge Edge = new FlatGRNNEdge(this);
                Edge.OriginIndex = edge.Origin.Index;
                Edge.DestinationIndex = edge.Destination.Index;
                Edges.Add(Edge);
            }

            ResetWeights();
        }

        public double[] Process(double[] Input, int EdgeIndex, bool Reverse)
        {
            return Edges[EdgeIndex].Process(Input, Reverse);
        }

        public IMLData Process(IMLData Input, int EdgeIndex, bool Reverse)
        {
            double[] inputarray = new double[Input.Count];
            for(int i = 0; i < Input.Count; i++)
            {
                inputarray[i] = Input[i];
            }
            return new BasicMLData(Edges[EdgeIndex].Process(inputarray, Reverse));
        }

        public object Clone()
        {
            FlatGRNN result = new FlatGRNN();

            result.EdgeInputSize = EdgeInputSize;
            result.EdgeToVertexFeedIndex = EdgeToVertexFeedIndex;
            result.InputSize = InputSize;
            result.OutputSize = OutputSize;
            result.DestinationWeights = EngineArray.ArrayCopy(DestinationWeights);
            result.OriginWeights = EngineArray.ArrayCopy(OriginWeights);
            result.EdgeNetwork = new FlatNetwork();
            result.VertexNetwork = new FlatNetwork();
            result.VertexLayerOutputTemplate = EngineArray.ArrayCopy(VertexLayerOutputTemplate);
            result.EdgeLayerOutputTemplate = EngineArray.ArrayCopy(EdgeLayerOutputTemplate);
            EdgeNetwork.CloneFlatNetwork(result.EdgeNetwork);
            VertexNetwork.CloneFlatNetwork(result.VertexNetwork);
            result.EdgeNetwork.Weights = EngineArray.ArrayCopy(EdgeNetwork.Weights);
            result.VertexNetwork.Weights = EngineArray.ArrayCopy(VertexNetwork.Weights);

            foreach(var edge in Edges)
            {
                FlatGRNNEdge temp = new FlatGRNNEdge(result);
                temp.DestinationIndex = edge.DestinationIndex;
                temp.OriginIndex = edge.OriginIndex;
                result.Edges.Add(temp);
            }
            foreach(var vertex in Vertices)
            {
                FlatGRNNVertex temp = new FlatGRNNVertex(result);
                result.Vertices.Add(temp);
            }
            

            return result;

        }
        #endregion Public

        #region Encoding
        /// <summary>
        /// Encodes the network weights into an array
        /// </summary>
        /// <param name="array"></param>
        public void EncodeToArray(double[] array)
        {
            if (array.Length != WeightCount)
            {
                throw new Exception("Incorrect array length.");
            }
            EngineArray.ArrayCopy(EdgeNetwork.Weights, 0, array, 0, EdgeNetwork.Weights.Length);
            VertexNetwork.SetInputWeights(OriginWeights);
            EngineArray.ArrayCopy(VertexNetwork.Weights, 0, array, EdgeNetwork.Weights.Length, VertexNetwork.Weights.Length);
            EngineArray.ArrayCopy(DestinationWeights, 0, array, EdgeNetwork.Weights.Length + VertexNetwork.Weights.Length, DestinationWeights.Length);
        }

        /// <summary>
        /// Returns the length of an encoded network array.
        /// </summary>
        /// <returns></returns>
        public int EncodedArrayLength()
        {
            return WeightCount;
        }

        /// <summary>
        /// Decodes an array into network weights.
        /// </summary>
        /// <param name="array"></param>
        public void DecodeFromArray(double[] array)
        {
            if (array.Length != WeightCount)
            {
                throw new Exception("Incorrect array length.");
            }
            EngineArray.ArrayCopy(array, 0, EdgeNetwork.Weights, 0, EdgeNetwork.Weights.Length);
            EngineArray.ArrayCopy(array, EdgeNetwork.Weights.Length, VertexNetwork.Weights, 0, VertexNetwork.Weights.Length);
            OriginWeights = VertexNetwork.GetInputWeights();
            EngineArray.ArrayCopy(array, EdgeNetwork.Weights.Length + VertexNetwork.Weights.Length, DestinationWeights, 0, DestinationWeights.Length);
        }

        #endregion Encoding

        #region Private

        #endregion Private

        #region Topology
        [Serializable]
        public class FlatGRNNVertex
        {
            private FlatGRNN _network;
            public double[] _layerOutputs { get; set; }
            public double[] _previousOutputs { get; set; }
            public double[] _layerSums { get; set; }
            public double[] _previousSums { get; set; }

            public FlatGRNNVertex(FlatGRNN Network)
            {
                _network = Network;
                _layerOutputs = new double[_network.VertexLayerOutputTemplate.Length];
                _previousOutputs = new double[_layerOutputs.Length];
                _layerSums = new double[_network.VertexLayerOutputTemplate.Length];
                _previousSums = new double[_layerSums.Length];


                EngineArray.ArrayCopy(_network.VertexLayerOutputTemplate, _layerOutputs);
                EngineArray.ArrayCopy(_network.VertexLayerOutputTemplate, _previousOutputs);

            }

            public double[] GetHiddenState()
            {
                double[] result = new double[_network.VertexNetwork.LayerFeedCounts[0]];
                EngineArray.ArrayCopy(_layerOutputs, 0, result, 0, result.Length);
                return result;
            }

            public void UpdateVertex(double[] layerOutputs, bool isOrigin)
            {
                EngineArray.ArrayCopy(_layerOutputs, _network.VertexNetwork.LayerOutput);
                if(isOrigin)
                {
                    //EngineArray.ArrayCopy(_network.OriginWeights, 0, _network.VertexNetwork.Weights, _network.VertexNetwork.WeightIndex[_network.VertexNetwork.WeightIndex.Length-2], _network.OriginWeights.Length);
                    _network.VertexNetwork.SetInputWeights(_network.OriginWeights);
                }
                else
                {
                    //EngineArray.ArrayCopy(_network.DestinationWeights, 0, _network.VertexNetwork.Weights, _network.VertexNetwork.WeightIndex[_network.VertexNetwork.WeightIndex.Length - 2], _network.DestinationWeights.Length);
                    _network.VertexNetwork.SetInputWeights(_network.DestinationWeights);
                }
                double[] inputarray = new double[_network.EdgeNetwork.LayerCounts[_network.EdgeToVertexFeedIndex]];
                EngineArray.ArrayCopy(layerOutputs, _network.EdgeNetwork.LayerIndex[_network.EdgeToVertexFeedIndex], inputarray, 0, inputarray.Length);
                double[] outputarray = new double[_network.VertexNetwork.OutputCount];
                _network.VertexNetwork.Compute(inputarray, outputarray);
                EngineArray.ArrayCopy(_layerOutputs, _previousOutputs);
                EngineArray.ArrayCopy(_network.VertexNetwork.LayerOutput, _layerOutputs);
                EngineArray.ArrayCopy(_layerSums, _previousSums);
                EngineArray.ArrayCopy(_network.VertexNetwork.LayerSums, _layerSums);
            }
        }

        [Serializable]
        public class FlatGRNNEdge
        {
            public double[] _layerOutputs { get; set; }
            public double[] _previousOutputs { get; set; }
            public double[] _layerSums { get; set; }
            private FlatGRNN _network { get; set; }
            public int OriginIndex { get; set; }
            public int DestinationIndex { get; set; }
            public FlatGRNNVertex Origin { get { return _network.Vertices[OriginIndex]; } }
            public FlatGRNNVertex Destination { get { return _network.Vertices[DestinationIndex]; } }

            public FlatGRNNEdge(FlatGRNN Network)
            {
                _network = Network;
                _layerOutputs = new double[_network.EdgeLayerOutputTemplate.Length];
                _previousOutputs = new double[_layerOutputs.Length];
                _layerSums = new double[_network.EdgeNetwork.LayerSums.Length];
                EngineArray.ArrayCopy(_network.EdgeLayerOutputTemplate, _layerOutputs);
                EngineArray.ArrayCopy(_network.EdgeLayerOutputTemplate, _previousOutputs);
            }

            public double[] Process(double[] input, bool Reverse)
            {
                EngineArray.ArrayCopy(_layerOutputs, _network.EdgeNetwork.LayerOutput);
                double[] inputarray = new double[_network.EdgeInputSize];
                EngineArray.ArrayCopy(input, 0, inputarray, 0, input.Length);
                if (!Reverse)
                {
                    double[] originArray = Origin.GetHiddenState();
                    EngineArray.ArrayCopy(originArray, 0, inputarray, input.Length, originArray.Length);
                    double[] destinationArray = Destination.GetHiddenState();
                    EngineArray.ArrayCopy(destinationArray, 0, inputarray, input.Length + originArray.Length, destinationArray.Length);
                }
                else
                {
                    double[] originArray = Destination.GetHiddenState();
                    EngineArray.ArrayCopy(originArray, 0, inputarray, input.Length, originArray.Length);
                    double[] destinationArray = Origin.GetHiddenState();
                    EngineArray.ArrayCopy(destinationArray, 0, inputarray, input.Length + originArray.Length, destinationArray.Length);
                }
                double[] output = new double[_network.OutputSize];
                _network.EdgeNetwork.Compute(inputarray, output);
                EngineArray.ArrayCopy(_layerOutputs, _previousOutputs);
                EngineArray.ArrayCopy(_network.EdgeNetwork.LayerOutput, _layerOutputs);
                EngineArray.ArrayCopy(_network.EdgeNetwork.LayerSums, _layerSums);

                Origin.UpdateVertex(_layerOutputs, !Reverse);
                Destination.UpdateVertex(_layerOutputs, Reverse);              

                return output;
            }

        }
        #endregion Topology
    }

    public static class FlatNetworkExtensions
    {
        public static void SetInputWeights(this FlatNetwork Network, double[] Weights)
        {
            int index = Network.WeightIndex[Network.WeightIndex.Length - 2];
            int n = 0;
            int skip = Network.LayerCounts.Last() - Network.InputCount;
            for(int i = 0; i < Network.LayerFeedCounts[Network.LayerFeedCounts.Length - 2]; i++)
            {
                for(int j = 0 ; j < Network.InputCount; j++)
                {
                    Network.Weights[index] = Weights[n];
                    index++;
                    n++;
                }
                index += skip;
            }
        }

        public static double[] GetInputWeights(this FlatNetwork Network)
        {
            double[] result = new double[Network.LayerFeedCounts[Network.LayerFeedCounts.Length - 2] * Network.InputCount];
            int index = Network.WeightIndex[Network.WeightIndex.Length - 2];
            int n = 0;
            int skip = Network.LayerCounts.Last() - Network.InputCount;
            for (int i = 0; i < Network.LayerFeedCounts[Network.LayerFeedCounts.Length - 2]; i++)
            {
                for (int j = 0; j < Network.InputCount; j++)
                {
                    result[n] = Network.Weights[index];
                    index++;
                    n++;
                }
                index += skip;
            }
            return result;
        }
    }
}
