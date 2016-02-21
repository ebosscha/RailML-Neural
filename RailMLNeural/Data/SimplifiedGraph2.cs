using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using RailMLNeural.UI.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RailMLNeural.Data
{
    /// <summary>
    /// Class describing a simplified graph model of the network topology. OCP's are stored in vertices, 
    /// and a matrix of edges describes the connections between the vertices.
    /// </summary>
    [Serializable]
    public class SimplifiedGraph
    {
        #region Parameters
        private List<SimplifiedGraphVertex> _vertices;
        private List<SimplifiedGraphEdge> _edges;
        private Dictionary<string, SimplifiedGraphVertex> _vertexDict;
        private DateTime _currentDate;
        private List<char> UnwantedHeaders = new List<char>() { 'C', 'F', 'G', 'H', 'I', 'J', 'O', 'Q', 'U', 'V', 'X', 'Y' };
        public int RunningThreads
        {
            get
            {
                return Edges.Sum(x => x.ThreadRunning);
            }
        }


        public List<SimplifiedGraphVertex> Vertices
        {
            get
            {
                return _vertices;
            }
        }

        public List<SimplifiedGraphEdge> Edges
        {
            get
            {
                return _edges;
            }
        }

        #endregion Parameters

        #region Public

        /// <summary>
        /// Creates new instance of SimplifiedGraph Object and Creates the Topology from used Links in the Timetable. Does not rely on 
        /// Infrastructure data to create network yet.
        /// </summary>
        public SimplifiedGraph()
        {
            _edges = new List<SimplifiedGraphEdge>();
            _vertices = new List<SimplifiedGraphVertex>();
            _vertexDict = new Dictionary<string, SimplifiedGraphVertex>();
            GenerateTopology();
        }

        private SimplifiedGraph(object state)
        {
            _edges = new List<SimplifiedGraphEdge>();
            _vertices = new List<SimplifiedGraphVertex>();
            _vertexDict = new Dictionary<string, SimplifiedGraphVertex>();
        }

        public void GenerateGraph(DelayCombination DC, bool Iterative)
        {
            if(DC.GetDate() == _currentDate)
            {
                RevertTimetable();
            }
            else
            {
                _currentDate = DC.GetDate();
                ClearTimeTable();
                GenerateTimeTable(_currentDate);
                UpdateVertices();
            }
            foreach(Delay d in DC.primarydelays)
            {
                AddDelay(d, true);
            }
            foreach(Delay d in DC.secondarydelays)
            {
                AddDelay(d, !Iterative);
            }
            //TODO
            //foreach(var edge in Edges.Where(x => x.IsAltered))
            //{
            //    edge.Sort();
            //}
        }

        public SimplifiedGraph Clone()
        {
            SimplifiedGraph result = new SimplifiedGraph(0);
            
            foreach(var vertex in Vertices)
            {
                SimplifiedGraphVertex v = new SimplifiedGraphVertex(vertex.OCP);
                v.Index = vertex.Index;
                v.TrackCount = vertex.TrackCount;
                result.Vertices.Add(v);
                result._vertexDict.Add(v.OCP.id, v);
            }
            foreach(var edge in Edges)
            {
                SimplifiedGraphEdge e = new SimplifiedGraphEdge(result.Vertices[edge.Origin.Index], result.Vertices[edge.Destination.Index], false);
                e.AverageSpeed = edge.AverageSpeed;
                e.Distance = edge.Distance;
                e.Index = edge.Index;
                e.PercentageDoubleTrack = edge.PercentageDoubleTrack;
                e.Route = edge.Route;
                e.SignalCount = edge.SignalCount;
                e.SpeedHomogenity = edge.SpeedHomogenity;
                e.SwitchCount = edge.SwitchCount;
                result.Edges.Add(e);
            }
            return result;
        }

        #endregion Public

        #region Private

        #region Topology
        private void GenerateTopology()
        {
            List<string> NorthernIrelandOcpCodes = new List<string>(){ "228", "238", "241", "242", "260" };
            int n = 0;
            foreach (eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                if (!NorthernIrelandOcpCodes.Contains(ocp.id))
                {
                    _vertices.Add(new SimplifiedGraphVertex(ocp));
                    _vertexDict.Add(ocp.id, Vertices[n]);
                    _vertices[n].Index = n;
                    n++;
                }
            }

            CreateEdges();
        }

        private void CreateEdges()
        {
            foreach (eTrainPart trainPart in DataContainer.model.timetable.trainParts.Where(x => !UnwantedHeaders.Contains(x.trainNumber[0])))
            {
                for (int i = 0; i < trainPart.ocpsTT.Count - 1; i++)
                {
                    AddEdge(trainPart.ocpsTT[i].ocpRef, trainPart.ocpsTT[i + 1].ocpRef);
                }
            }
        }

        private void AddEdge(string OCP1, string OCP2)
        {
            //Check if the edge already exists
            if(Edges.Any(x => (x.Origin.OCP.id == OCP1 && x.Destination.OCP.id == OCP2) || (x.Origin.OCP.id == OCP2 && x.Destination.OCP.id == OCP1)))
            {
                return;
            }
            //check if the OCP's are different
            if(OCP1 == OCP2)
            {
                return;
            }
            //check wether both ocp's have vertices
            if(!Vertices.Any(x => x.OCP.id == OCP1) || !Vertices.Any(x => x.OCP.id == OCP2))
            {
                return;
            }

            SimplifiedGraphEdge edge = new SimplifiedGraphEdge(Vertices.Single(x => x.OCP.id == OCP1),Vertices.Single(x => x.OCP.id == OCP2) );
            _edges.Add(edge);
            edge.Index = _edges.IndexOf(edge);            
        }

        #endregion Topology

        #region BuildTimeTable
        private void GenerateTimeTable(DateTime Date)
        {
            List<eTrainPart> TrainPartList = DataContainer.model.timetable.GetTrainsByDay(Date);
            foreach(var trainpart in TrainPartList.Where(x => !UnwantedHeaders.Contains(x.trainNumber[0])))
            {
                EdgeTrainRepresentation Previous = null;
                for (int i = 0; i < trainpart.ocpsTT.Count - 1; i++)
                {
                    var p = AddToEdge(trainpart.trainNumber, trainpart.ocpsTT[i], trainpart.ocpsTT[i + 1], Previous);
                    if (p != null) { Previous = p; }
                }
            }
            
            //Order Trains by time
            foreach(var edge in Edges)
            {
                edge.Sort();
            }

            //foreach(var vertex in Vertices)
            //{
            //    vertex.Sort();
            //}
        }

        public EdgeTrainRepresentation AddToEdge(string HeaderCode, eOcpTT ocpTT1, eOcpTT ocpTT2, EdgeTrainRepresentation Previous)
        {
            if (ocpTT1.ocpRef == ocpTT2.ocpRef || !_vertexDict.ContainsKey(ocpTT1.ocpRef) || !_vertexDict.ContainsKey(ocpTT2.ocpRef))
            {
                return null;
            }
            SimplifiedGraphVertex v1 = _vertexDict[ocpTT1.ocpRef];
            SimplifiedGraphEdge Edge = v1.Edges.Single(x => x.Destination.OCP.id == ocpTT2.ocpRef || x.Origin.OCP.id == ocpTT2.ocpRef);
            EdgeTrainRepresentation Rep = new EdgeTrainRepresentation(Edge);
            Rep.Previous = Previous;
            Rep.TrainHeaderCode = HeaderCode;
            if (Edge.Origin.OCP.id == ocpTT1.ocpRef) { Rep.Direction = DirectionEnum.Down; }
            else { Rep.Direction = DirectionEnum.Up; }
            Rep.ScheduledDepartureTime = ocpTT1.times[0].departure;
            Rep.PredictedDepartureTime = ocpTT1.times[0].departure;
            Rep.IdealDepartureTime = ocpTT1.times[0].departure;
            Rep.ScheduledArrivalTime = ocpTT2.times[0].arrival;
            Rep.PredictedArrivalTime = ocpTT2.times[0].arrival;
            Rep.IdealArrivalTime = ocpTT2.times[0].arrival;
            Edge.Trains.Add(Rep);
            return Rep;
        }
        #endregion BuildTimeTable

        private void ClearTimeTable()
        {
            foreach(var edge in Edges)
            {
                edge.Trains = new List<EdgeTrainRepresentation>();
            }
            foreach(var vertex in Vertices)
            {
                vertex.Clear();
            }
        }

        public void RevertTimetable()
        {
            foreach(var edge in Edges)
            {
                foreach(var rep in edge.Trains)
                {
                    rep.IdealArrivalTime = rep.ScheduledArrivalTime;
                    rep.IdealDepartureTime = rep.ScheduledDepartureTime;
                    rep.PredictedArrivalTime = rep.ScheduledArrivalTime;
                    rep.PredictedDepartureTime = rep.ScheduledDepartureTime;
                }
            }
            foreach(var vertex in Vertices)
            {
                vertex.IsSubGraph = false;
            }
            //foreach(var edge in Edges.Where(x => x.IsAltered == true))
            //{
            //    edge.Sort();
            //    edge.IsAltered = false;
            //}
        }

        #region AddDelay
        private void AddDelay(Delay D, bool IsPrimary)
        {
            if (D.stopdelays.Count > 0)
            {
                AddDelayEdge(D.traincode, D.origin, D.origindelay, D.stopdelays[0].location, D.stopdelays[0].arrivaldelay, IsPrimary);
                for (int i = 0; i < D.stopdelays.Count-1; i++)
                {
                    AddDelayEdge(D.traincode, D.stopdelays[i].location, D.stopdelays[i].departuredelay, D.stopdelays[i+1].location, D.stopdelays[i+1].arrivaldelay, IsPrimary);
                }
                AddDelayEdge(D.traincode, D.stopdelays.Last().location, D.stopdelays.Last().departuredelay, D.destination, D.destinationdelay, IsPrimary);
            }
            else
            {
                AddDelayEdge(D.traincode, D.origin, D.origindelay, D.destination, D.destinationdelay, IsPrimary);
            }
        }

        private void AddDelayEdge(string HeaderCode, string origin, double departuredelay, string destination, double arrivaldelay, bool IsPrimary)
        {
            if(!Vertices.Any(x => x.OCP.code == origin) || !Vertices.Any(x => x.OCP.code == destination))
            {
                return;
            }
            var vertex = Vertices.Single(x => x.OCP.code == origin);
            var edge = vertex.Edges.Single(x => x.Origin.OCP.code == destination || x.Destination.OCP.code == destination);
            var rep = edge.Trains.First(x => x.TrainHeaderCode == HeaderCode);
            rep.IdealDepartureTime = rep.ScheduledDepartureTime + TimeSpan.FromSeconds(departuredelay);
            rep.IdealArrivalTime = rep.ScheduledArrivalTime + TimeSpan.FromSeconds(arrivaldelay);
            if(IsPrimary)
            {
                rep.PredictedArrivalTime = rep.ScheduledArrivalTime + TimeSpan.FromSeconds(arrivaldelay);
                rep.PredictedDepartureTime = rep.ScheduledDepartureTime + TimeSpan.FromSeconds(departuredelay);
            }
                //edge.IsAltered = true;
            edge.Destination.IsSubGraph = true;
            edge.Origin.IsSubGraph = true;

        }
        #endregion AddDelay

        private void UpdateVertices()
        {
            foreach(var vertex in Vertices)
            {
                vertex.Update();
            }
        }
        #endregion Private
    }

    /// <summary>
    /// Class describing an edge(connection) in the simplified graph object.
    /// Built to contain data about infrastructure as well as traversing trains.
    /// </summary>
    [Serializable]
    public class SimplifiedGraphEdge
    {
        public int Index { get; set; }
        public double PercentageDoubleTrack { get; set; }
        public double Distance { get; set; }
        public double SwitchCount { get; set; }
        public double SignalCount { get; set; }
        public double AverageSpeed { get; set; }
        public double SpeedHomogenity { get; set; }
        public SimplifiedGraphVertex Origin { get; set; }
        public SimplifiedGraphVertex Destination { get; set; }
        public List<EdgeTrainRepresentation> Trains { get; set; }
        public Route Route { get; set; }
        public int ThreadRunning;
        public bool IsSubGraph { get { return Origin.IsSubGraph || Destination.IsSubGraph; } }

        public SimplifiedGraphEdge(SimplifiedGraphVertex origin, SimplifiedGraphVertex destination)
        {
            Trains = new List<EdgeTrainRepresentation>();
            Origin = origin;
            Destination = destination;
            Origin.Edges.Add(this);
            Destination.Edges.Add(this);
            ThreadPool.QueueUserWorkItem(SetRoute);
        }

        public SimplifiedGraphEdge(SimplifiedGraphVertex origin, SimplifiedGraphVertex destination, bool setRoute)
        {
            Trains = new List<EdgeTrainRepresentation>();
            Origin = origin;
            Destination = destination;
            Origin.Edges.Add(this);
            Destination.Edges.Add(this);
            if(setRoute)
            {
                ThreadPool.QueueUserWorkItem(SetRoute);
            }
        }

        public void Sort()
        {
            Trains = new List<EdgeTrainRepresentation>(Trains.OrderBy(x => x.ScheduledDepartureTime));
        }

        private void SetRoute(object state)
        {
            Interlocked.Increment(ref ThreadRunning);
            Route = DataContainer.PathContainer.GetRoute(Origin.OCP, Destination.OCP);
            if (Route != null)
            {
                PercentageDoubleTrack = Route.PercentageDoubleTrack();
                SwitchCount = Route.SwitchCount();
                Distance = (double)Route.distance;
                if(Route.distance > 100)
                {
                    int i = 0;
                }
            }
            Logger.AddEntry("Created Edge between " + Origin.OCP.name + " and " + Destination.OCP.name + ".");
            Interlocked.Decrement(ref ThreadRunning);
        }
    }

    /// <summary>
    /// Class describing a Vertex(Node) in the SimplifiedGraph Object
    /// Contains data about OCP itself and passing traffic.
    /// </summary>
    [Serializable]
    public class SimplifiedGraphVertex
    {
        public int Index { get; set; }
        public eOcp OCP { get; set; }
        public int TrackCount { get; set; }
        public List<SimplifiedGraphEdge> Edges { get; set; }
        public IEnumerable<VertexTrainRepresentation> Trains { get { return _trainDict.Values; } }
        private Dictionary<string, VertexTrainRepresentation> _trainDict;
        public bool IsSubGraph { get; set; }

        public bool IsStation
        {
            get
            { return OCP.type == "station"; }
        }

        public SimplifiedGraphVertex(eOcp ocp)
        {
            Edges = new List<SimplifiedGraphEdge>();
            _trainDict = new Dictionary<string, VertexTrainRepresentation>();
            OCP = ocp;
            TrackCount = OCP.StationTrackCount();
            IsSubGraph = false;
        }

        public void Clear()
        {
            _trainDict = new Dictionary<string, VertexTrainRepresentation>();
            IsSubGraph = false;
        }

        public void Update()
        {
            _trainDict = new Dictionary<string, VertexTrainRepresentation>();
            foreach(var edge in Edges)
            {
                foreach(var rep in edge.Trains)
                {
                    if ((edge.Origin.OCP.id == this.OCP.id && rep.Direction == DirectionEnum.Down) ||
                        (edge.Destination.OCP.id == this.OCP.id && rep.Direction == DirectionEnum.Up))
                    {
                        VertexTrainRepresentation Vrep = new VertexTrainRepresentation(this) { TrainHeaderCode = rep.TrainHeaderCode };
                        if (_trainDict.ContainsKey(rep.TrainHeaderCode))
                        {
                            Vrep = _trainDict[rep.TrainHeaderCode];
                        }
                        else { _trainDict.Add(rep.TrainHeaderCode,Vrep); }
                        Vrep.ScheduledDepartureTime = rep.ScheduledDepartureTime;
                        Vrep.IdealDepartureTime = rep.IdealDepartureTime;
                        Vrep.PredictedDepartureTime = rep.PredictedDepartureTime;
                    }
                    else
                    {
                        VertexTrainRepresentation Vrep = new VertexTrainRepresentation(this) { TrainHeaderCode = rep.TrainHeaderCode };
                        if (_trainDict.ContainsKey(rep.TrainHeaderCode))
                        {
                            Vrep = _trainDict[rep.TrainHeaderCode];
                        }
                        else { _trainDict.Add(rep.TrainHeaderCode, Vrep); }
                        Vrep.ScheduledArrivalTime = rep.ScheduledArrivalTime;
                        Vrep.IdealArrivalTime = rep.IdealArrivalTime;
                        Vrep.PredictedArrivalTime = rep.PredictedArrivalTime;
                    }
                }
            }
        }
        
    }

    [Serializable]
    public class VertexTrainRepresentation
    {
        public string TrainHeaderCode { get; set; }
        public DateTime ScheduledArrivalTime { get; set; }
        public DateTime PredictedArrivalTime { get; set; }
        public DateTime IdealArrivalTime { get; set; }
        public DateTime ScheduledDepartureTime { get; set; }
        public DateTime PredictedDepartureTime { get; set; }
        public DateTime IdealDepartureTime { get; set; }
        public SimplifiedGraphVertex Vertex { get; private set; }

        public VertexTrainRepresentation(SimplifiedGraphVertex Owner)
        {
            Vertex = Owner;
        }
    }

    [Serializable]
    public class EdgeTrainRepresentation
    {
        public string TrainHeaderCode { get; set; }
        public DateTime ScheduledArrivalTime { get; set; }
        public DateTime PredictedArrivalTime { get; set; }
        public DateTime IdealArrivalTime { get; set; }
        public DateTime ScheduledDepartureTime { get; set; }
        public DateTime PredictedDepartureTime { get; set; }
        public DateTime IdealDepartureTime { get; set; }
        public DirectionEnum Direction { get; set; }
        public SimplifiedGraphEdge Edge { get; private set; }
        public EdgeTrainRepresentation Previous { get; set; }


        public EdgeTrainRepresentation(SimplifiedGraphEdge Owner)
        {
            Edge = Owner;
        }

    }

    public enum DirectionEnum
    {
        Up,
        Down
    }

    public static class Extentions2
    {
        public static int StationTrackCount(this eOcp OCP)
        {
            return DataContainer.model.infrastructure.tracks
                .SelectMany(x => x.trackTopology.crossSections)
                .Where(x => x.ocpRef == OCP.id)
                .Count();
        }

        public static int SwitchCount(this Route route)
        {
            int count = 0;
            foreach(RoutePart part in route.route)
            {
                count += part.track.trackTopology.connections
                        .Where(x => x.pos > Math.Min(part.begin, part.end) && x.pos < Math.Max(part.begin, part.end)).Count();
            }
            return count;
        }
    }
}
