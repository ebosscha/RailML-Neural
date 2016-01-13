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
        private SimplifiedGraphEdge[][] _edges;
        private Dictionary<string, int> _vertexindices;
        private DateTime _currentDate;

        #endregion Parameters

        #region Public

        /// <summary>
        /// Creates new instance of SimplifiedGraph Object and Creates the Topology from used Links in the Timetable. Does not rely on 
        /// Infrastructure data to create network yet.
        /// </summary>
        public SimplifiedGraph()
        {
            _vertexindices = new Dictionary<string, int>();
            CreateSimplifiedGraphTopology();
        }

        public void RebuildTimeTable(DelayCombination dc)
        {
            if (dc.GetDate() != _currentDate)
            {
                ClearTimeTable();
                RebuildTimeTable(dc.GetDate());
                _currentDate = dc.GetDate();
            }
            else
            {
                RevertTimetable();
            }
            AddDelayCombination(dc);
        }

       

        /// <summary>
        /// Clears the Timetable Element of the simplifiedGraph
        /// </summary>
        public void ClearTimeTable()
        {
            foreach(SimplifiedGraphVertex x in _vertices)
            {
                x.ClearTimeTable();
            }
            for(int i = 0; i < _edges.Length; i++)
            {
                for(int j = 0; j < _edges[i].Length; j++)
                {
                    if(_edges[i][j] != null)
                    {
                        _edges[i][j].ClearTimeTable();
                    }
                }
            }
        }
        #endregion Public

        #region Private
        /// <summary>
        /// Creates Topology based on every OCP
        /// </summary>
        private void CreateSimplifiedGraphTopology()
        {
            _vertices = new List<SimplifiedGraphVertex>();
            int n = 0;
            foreach(eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                _vertices.Add(new SimplifiedGraphVertex() { OCP = ocp });
                _vertexindices.Add(ocp.code, n);
                n++;
            }
            _edges = new SimplifiedGraphEdge[_vertices.Count][];
            for (int i = 0; i < _edges.Length; i++ )
            {
                _edges[i] = Enumerable.Repeat<SimplifiedGraphEdge>(null, _vertices.Count).ToArray();
            }

            CreateLinks();
        }

        /// <summary>
        /// Creates links based on every train description in the RailML Timetable part. 
        /// Used to represent all connections between OCPs 
        /// </summary>
        private void CreateLinks()
        {
            foreach(eTrainPart trainPart in DataContainer.model.timetable.trainParts)
            {
                for(int i = 0; i < trainPart.ocpsTT.Count - 1; i++)
                {
                    AddLink(trainPart.ocpsTT[i].ocpRef, trainPart.ocpsTT[i+1].ocpRef);
                }
            }
        }

        /// <summary>
        /// Adds a link between two OCP vertices. Currently creates a matrix mirrored diagonally.
        /// So every edge represents both the Up and Down road in case of a double track.
        /// </summary>
        /// <param name="originID"></param>
        /// <param name="destinationID"></param>
        private void AddLink(string originID, string destinationID)
        {
            int i = _vertices.IndexOf(_vertices.First(x => x.OCP.id == originID));
            int j = _vertices.IndexOf(_vertices.First(x => x.OCP.id == destinationID));
            if(_edges[i][j] == null || _edges[j][i] == null)
            {
                _edges[i][j] = new SimplifiedGraphEdge()
                {
                    Origin = _vertices[i],
                    Destination = _vertices[j]
                };
                _edges[j][i] = _edges[i][j];
            }
        }

        #region BuildTimeTable
        public void RebuildTimeTable(DateTime Date)
        {
            var TrainPartList = DataContainer.model.timetable.GetTrainsByDay(Date);
            foreach(eTrainPart TrainPart in TrainPartList)
            {
                for(int i = 0; i < TrainPart.ocpsTT.Count - 1; i++)
                {
                    AddToVertex(TrainPart.ocpsTT[i], TrainPart.trainNumber);
                    AddToEdge(TrainPart.ocpsTT[i], TrainPart.ocpsTT[i + 1], TrainPart.trainNumber);
                }
                AddToVertex(TrainPart.ocpsTT.Last(), TrainPart.trainNumber);
            }
        }

        public void AddToVertex(eOcpTT ocpTT, string HeaderCode)
        {
            var vertex1 = _vertices.Single(x => x.OCP.id == ocpTT.ocpRef);
            TrainVertexRepresentation rep = new TrainVertexRepresentation()
            {
                TrainHeaderCode = HeaderCode,
                ScheduledArrivalTime = ocpTT.times[0].arrival,
                ScheduledDepartureTime = ocpTT.times[0].departure,
                IdealArrivalTime = ocpTT.times[0].arrival,
                IdealDepartureTime = ocpTT.times[0].departure
            };
            vertex1.PassingTrains.Add(rep);
        }

        public void AddToEdge(eOcpTT ocpTT1, eOcpTT ocpTT2, string HeaderCode)
        {
            int i = _vertices.FindIndex(x => x.OCP.id == ocpTT1.ocpRef);
            int j = _vertices.FindIndex(x => x.OCP.id == ocpTT2.ocpRef);
            TrainEdgeRepresentation rep = new TrainEdgeRepresentation()
            {
                TrainHeaderCode = HeaderCode,
                ScheduledDepartureTime = ocpTT1.times[0].departure,
                ScheduledArrivalTime = ocpTT2.times[0].arrival,
                IdealDepartureTime = ocpTT1.times[0].departure,
                IdealArrivalTime = ocpTT2.times[0].arrival
            };
            if(_edges[i][j].Origin.OCP.id == ocpTT1.ocpRef)
            {
                rep.Direction = DirectionEnum.Down;
            }
            else 
            { 
                rep.Direction = DirectionEnum.Up; 
            }
            _edges[i][j].TraversingTrains.Add(rep);
        }


        /// <summary>
        /// Revert Timetable to original daily timetable.
        /// </summary>
        private void RevertTimetable()
        {
            foreach(var v in _vertices)
            {
                foreach(var t in v.PassingTrains.Where(x => x.IdealArrivalTime != x.ScheduledArrivalTime || x.IdealDepartureTime != x.ScheduledDepartureTime))
                {
                    t.IdealArrivalTime = new DateTime(t.ScheduledArrivalTime.Ticks);
                    t.IdealDepartureTime = new DateTime(t.ScheduledDepartureTime.Ticks);
                    t.PredictedArrivalTime = DateTime.MinValue;
                    t.PredictedDepartureTime = DateTime.MinValue;
                }
            }
            foreach(var y in _edges)
            { 
                foreach(var z in y)
                {
                    foreach (var t in z.TraversingTrains.Where(x => x.IdealArrivalTime != x.ScheduledArrivalTime || x.IdealDepartureTime != x.ScheduledDepartureTime))
                    {
                        t.IdealArrivalTime = new DateTime(t.ScheduledArrivalTime.Ticks);
                        t.IdealDepartureTime = new DateTime(t.ScheduledDepartureTime.Ticks);
                        t.PredictedArrivalTime = DateTime.MinValue;
                        t.PredictedDepartureTime = DateTime.MinValue;
                    }
                }
            }
        }

        private void AddDelayCombination(DelayCombination dc)
        {
            foreach(Delay d in dc.primarydelays)
            {
                AddDelay(d, true);
            }
            foreach(Delay d in dc.secondarydelays)
            {
                AddDelay(d, false);
            }
        }

        private void AddDelay(Delay delay, bool IsPrimary)
        {
            OverrideVertex(delay.traincode, delay.origin, IsPrimary, default(DateTime), delay.ActualDeparture);
            for(int i = 0; i < delay.stopdelays.Count; i++)
            {
                StopDelay d1 = delay.stopdelays[i];
                OverrideVertex(delay.traincode, d1.location, IsPrimary, d1.ActualArrival, d1.ActualDeparture);
            }
            OverrideVertex(delay.traincode, delay.destination, IsPrimary, delay.ActualArrival, default(DateTime));
            if(delay.stopdelays.Count > 0)
            {
                OverrideEdge(delay.traincode, delay.origin, delay.stopdelays[0].location, IsPrimary, delay.ActualDeparture, delay.stopdelays[0].ActualArrival);
                for(int i = 0; i < delay.stopdelays.Count - 1; i++)
                {
                    StopDelay d1 = delay.stopdelays[i];
                    StopDelay d2 = delay.stopdelays[i+1];
                    OverrideEdge(delay.traincode, d1.location, d2.location, IsPrimary, d1.ActualDeparture, d2.ActualArrival);
                }
                OverrideEdge(delay.traincode, delay.stopdelays.Last().location, delay.destination, IsPrimary, delay.stopdelays.Last().ActualDeparture, delay.ScheduledArrival);
            }
            else
            {
                OverrideEdge(delay.traincode, delay.origin, delay.destination, IsPrimary, delay.ActualDeparture, delay.ActualArrival);
            }
        }

        private void OverrideVertex(string HeaderCode, string StationSemaName, bool IsPrimary, DateTime ActualArrival, DateTime ActualDeparture)
        {
            int index = _vertices.FindIndex(x => x.OCP.code == StationSemaName);
            SimplifiedGraphVertex vertex = _vertices[index];
            TrainVertexRepresentation rep = vertex.PassingTrains.Single(x => x.TrainHeaderCode == HeaderCode);
            rep.IdealArrivalTime = ActualArrival;
            rep.IdealDepartureTime = ActualDeparture;
            if(IsPrimary)
            {
                rep.PredictedArrivalTime = ActualArrival;
                rep.PredictedDepartureTime = ActualArrival;
            }
            else
            {
                rep.PredictedArrivalTime = DateTime.MinValue;
                rep.PredictedDepartureTime = DateTime.MinValue;
            }
        }

        private void OverrideEdge(string HeaderCode, string Origin, string Destination, bool IsPrimary, DateTime ActualDeparture, DateTime ActualArrival)
        {
            SimplifiedGraphEdge edge = _edges[_vertexindices[Origin]][_vertexindices[Destination]];
            TrainEdgeRepresentation rep = edge.TraversingTrains.Single(x => x.TrainHeaderCode == HeaderCode);
            rep.IdealArrivalTime = ActualArrival;
            rep.IdealDepartureTime = ActualDeparture;
            if(IsPrimary)
            {
                rep.PredictedArrivalTime = ActualArrival;
                rep.PredictedDepartureTime = ActualDeparture;
            }
            else
            {
                rep.PredictedArrivalTime = DateTime.MinValue;
                rep.PredictedDepartureTime = DateTime.MaxValue;
            }
        }

        #endregion BuildtimeTable
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
        public List<TrainEdgeRepresentation> TraversingTrains { get; set; }

        public SimplifiedGraphEdge()
        {
            TraversingTrains = new List<TrainEdgeRepresentation>();
        }

        public void ClearTimeTable()
        {
            TraversingTrains = new List<TrainEdgeRepresentation>();
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
        public List<TrainVertexRepresentation> PassingTrains { get; set; }

        public bool IsStation { get
            { return OCP.type == "station"; }
        }

        public SimplifiedGraphVertex()
        {
            PassingTrains = new List<TrainVertexRepresentation>();
        }
        public void ClearTimeTable()
        {
            PassingTrains = new List<TrainVertexRepresentation>();
        }
    }

    public class TrainEdgeRepresentation
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

    public class TrainVertexRepresentation
    {
        public string TrainHeaderCode { get; set; }
        public DateTime ScheduledArrivalTime { get; set; }
        public DateTime PredictedArrivalTime { get; set; }
        public DateTime IdealArrivalTime { get; set; }
        public DateTime ScheduledDepartureTime { get; set; }
        public DateTime PredictedDepartureTime { get; set; }
        public DateTime IdealDepartureTime { get; set; }
    }
}
