using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Data
{
    /// <summary>
    /// Class describing a simplified graph model of the network topology. OCP's are stored in vertices, 
    /// and a matrix of edges describes the connections between the vertices.
    /// </summary>
    class SimplifiedGraph
    {
        #region Parameters
        private List<SimplifiedGraphVertex> _vertices;
        private List<SimplifiedGraphEdge> _edges;
        private Dictionary<string, int> _vertexindices;
        private DateTime _currentDate;

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
            _vertexindices = new Dictionary<string, int>();
            _edges = new List<SimplifiedGraphEdge>();
            _vertices = new List<SimplifiedGraphVertex>();
            GenerateTopology();
        }

        public void GenerateGraph(DelayCombination DC)
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
            }
            foreach(Delay d in DC.primarydelays)
            {
                AddDelay(d, true);
            }
            foreach(Delay d in DC.secondarydelays)
            {
                AddDelay(d, false);
            }
            UpdateVertices();
        }

        #endregion Public

        #region Private

        #region Topology
        private void GenerateTopology()
        {
            int n = 0;
            foreach (eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                _vertices.Add(new SimplifiedGraphVertex() { OCP = ocp });
                _vertexindices.Add(ocp.code, n);
                n++;
            }

            CreateEdges();
        }

        private void CreateEdges()
        {
            foreach (eTrainPart trainPart in DataContainer.model.timetable.trainParts)
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

            SimplifiedGraphEdge edge = new SimplifiedGraphEdge();
            edge.Origin = Vertices.Single(x => x.OCP.id == OCP1);
            edge.Destination = Vertices.Single(x => x.OCP.id == OCP2);
            _edges.Add(edge);
            edge.Origin.Edges.Add(edge);
            edge.Destination.Edges.Add(edge);
        }

        #endregion Topology

        #region BuildTimeTable
        private void GenerateTimeTable(DateTime Date)
        {
            List<eTrainPart> TrainPartList = DataContainer.model.timetable.GetTrainsByDay(Date);
            foreach(var trainpart in TrainPartList)
            {
                for (int i = 0; i < trainpart.ocpsTT.Count - 1; i++)
                {
                    AddToEdge(trainpart.trainNumber, trainpart.ocpsTT[i], trainpart.ocpsTT[i + 1]);
                }
            }
        }

        public void AddToEdge(string HeaderCode, eOcpTT ocpTT1, eOcpTT ocpTT2)
        {
            if(ocpTT1.ocpRef == ocpTT2.ocpRef)
            {
                return;
            }
            SimplifiedGraphVertex v1 = Vertices.Single(x => x.OCP.id == ocpTT1.ocpRef);
            SimplifiedGraphEdge Edge = v1.Edges.Single(x => x.Destination.OCP.id == ocpTT2.ocpRef || x.Origin.OCP.id == ocpTT2.ocpRef);
            EdgeTrainRepresentation Rep = new EdgeTrainRepresentation();
            Rep.TrainHeaderCode = HeaderCode;
            if (Edge.Origin.OCP.id == ocpTT1.ocpRef) { Rep.Direction = DirectionEnum.Down; }
            else { Rep.Direction = DirectionEnum.Up; }
            Rep.ScheduledDepartureTime = ocpTT1.times[0].departure;
            Rep.PredictedDepartureTime = ocpTT1.times[0].departure;
            Rep.IdealDepartureTime = ocpTT1.times[0].departure;
            Rep.ScheduledArrivalTime = ocpTT2.times[0].arrival;
            Rep.PredictedArrivalTime = ocpTT2.times[0].arrival;
            Rep.IdealDepartureTime = ocpTT2.times[0].arrival;
            Edge.Trains.Add(Rep);
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
                vertex.Trains = new List<VertexTrainRepresentation>();
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
    public class SimplifiedGraphEdge
    {
        public double PercentageDoubleTrack { get; set; }
        public double Distance { get; set; }
        public double SwitchCount { get; set; }
        public double SignalCount { get; set; }
        public double AverageSpeed { get; set; }
        public double SpeedHomogenity { get; set; }
        public SimplifiedGraphVertex Origin { get; set; }
        public SimplifiedGraphVertex Destination { get; set; }
        public List<EdgeTrainRepresentation> Trains { get; set; }

        public SimplifiedGraphEdge()
        {
            Trains = new List<EdgeTrainRepresentation>();
        }
    }

    /// <summary>
    /// Class describing a Vertex(Node) in the SimplifiedGraph Object
    /// Contains data about OCP itself and passing traffic.
    /// </summary>
    public class SimplifiedGraphVertex
    {
        public eOcp OCP { get; set; }
        public int TrackCount { get; set; }
        public List<SimplifiedGraphEdge> Edges { get; set; }
        public List<VertexTrainRepresentation> Trains { get; set; }

        public bool IsStation
        {
            get
            { return OCP.type == "station"; }
        }

        public SimplifiedGraphVertex()
        {
            Edges = new List<SimplifiedGraphEdge>();
            Trains = new List<VertexTrainRepresentation>();
        }

        public void Update()
        {
            Trains = new List<VertexTrainRepresentation>();
            foreach(var edge in Edges)
            {
                foreach(var rep in edge.Trains.Where(x => x.Direction == DirectionEnum.Down))
                {
                    VertexTrainRepresentation Vrep = new VertexTrainRepresentation() {TrainHeaderCode = rep.TrainHeaderCode};
                    if(Trains.Any(x => x.TrainHeaderCode == rep.TrainHeaderCode))
                    {
                        Vrep = Trains.First(x => x.TrainHeaderCode == rep.TrainHeaderCode);
                    }
                    else { Trains.Add(Vrep);}
                    Vrep.ScheduledDepartureTime = rep.ScheduledDepartureTime;
                    Vrep.IdealDepartureTime = rep.IdealDepartureTime;
                    Vrep.PredictedDepartureTime = rep.PredictedDepartureTime;
                }
                foreach(var rep in edge.Trains.Where(x => x.Direction == DirectionEnum.Up))
                {
                    VertexTrainRepresentation Vrep = new VertexTrainRepresentation() { TrainHeaderCode = rep.TrainHeaderCode };
                    Trains.Add(Vrep);
                    Vrep.ScheduledDepartureTime = rep.ScheduledDepartureTime;
                    Vrep.IdealDepartureTime = rep.IdealDepartureTime;
                    Vrep.PredictedDepartureTime = rep.PredictedDepartureTime;
                }
            }
        }
        
    }

    public class VertexTrainRepresentation
    {
        public string TrainHeaderCode { get; set; }
        public DateTime ScheduledArrivalTime { get; set; }
        public DateTime PredictedArrivalTime { get; set; }
        public DateTime IdealArrivalTime { get; set; }
        public DateTime ScheduledDepartureTime { get; set; }
        public DateTime PredictedDepartureTime { get; set; }
        public DateTime IdealDepartureTime { get; set; }
    }

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
    }

    public enum DirectionEnum
    {
        Up,
        Down
    }
}
