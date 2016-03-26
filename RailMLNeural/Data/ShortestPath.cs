using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailMLNeural.Data
{
    [Serializable]
    public class PathContainer
    {
        private Dictionary<string, Dictionary<string, Route>> dict;
        public PathContainer()
        {
            Clear();
        }

        public void Clear()
        {
            dict = new Dictionary<string, Dictionary<string, Route>>();
            foreach (eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                Dictionary<string, Route> tempdict = new Dictionary<string, Route>();
                dict.Add(ocp.id, tempdict);

            }
        }

        public Route GetRoute(eOcp origin, eOcp destination)
        {
            return GetRoute(origin.id, destination.id);
        }

        public Route GetRoute(string originid, string destinationid)
        {
            if (dict[originid].ContainsKey(destinationid))
            {
                return dict[originid][destinationid];
            }
            else
            {
                Route r = new Route(originid, destinationid);
                lock (dict)
                {
                    dict[originid].Add(destinationid, r);
                }
                return r;
            }
        }
    }

    [Serializable]
    public class Route
    {
        private Dictionary<string, Vertex> vertices { get; set; }

        private infrastructure inf { get; set; }
        public eOcp origin { get; set; }
        public eOcp destination { get; set; }
        public List<RoutePart> route { get; set; }
        public decimal distance { get; set; }
        public Route(string originid, string destinationid)
        {
            vertices = new Dictionary<string, Vertex>();
            route = new List<RoutePart>();
            inf = DataContainer.model.infrastructure;
            origin = inf.operationControlPoints.Single(x => x.id == originid);
            destination = inf.operationControlPoints.Single(x => x.id == destinationid);
            if (origin.geoCoord.coord.Count == 2 && destination.geoCoord.coord.Count == 2)
            {

                CalculateRoute();
            }
        }


        private void CalculateRoute()
        {

            // Set up vertices
            int n = 0;
            for (int i = 0; i < inf.tracks.Count; i++)
            {
                Vertex v = new Vertex { track = inf.tracks[i] };

                if (v.track.trackTopology.crossSections.Any(x => x.ocpRef == origin.id))
                {
                    v.pos = v.track.trackTopology.crossSections.Single(e => e.ocpRef == origin.id).pos;
                    v.dist = 0;
                    n++;
                }
                else { v.dist = (decimal)99999999999999999; }
                vertices.Add(v.track.id, v);
            }
            if(n == 0)
            {
                return;
            }

            while (vertices.Count > 0)
            {
                Vertex v = vertices.Values.Aggregate((agg, item) => item.dist < agg.dist ? item : agg);
                if (HandleVertex(ref v))
                {
                    break;
                }
                vertices.Remove(v.track.id);
            }
        }

        private bool HandleVertex(ref Vertex v)
        {
            decimal lowerbound = -9999999999999999;
            decimal upperbound = 9999999999999999;
            //TODO:: Implement driving direction in ShortestPath
            //switch (v.track.mainDir)
            //{
            //    case tExtendedDirection.up:
            //        upperbound = v.pos;
            //        break;
            //    case tExtendedDirection.down:
            //        lowerbound = v.pos;
            //        break;
            //    default:
            //        break;
            //}
            foreach (tCrossSection c in v.track.trackTopology.crossSections)
            {
                if (c.pos > lowerbound && c.pos < upperbound)
                {
                    if (c.ocpRef == destination.id)
                    {
                        distance = v.dist + Math.Abs(v.pos - c.pos);
                        if(distance > 20)
                        {
                            int i = 0;
                        }
                        route = v.route;
                        route.Add(new RoutePart() { track = v.track, begin = v.pos, end = c.pos });
                        return true;
                    }
                }
            }

            foreach (eSwitch sw in v.track.trackTopology.connections.Where(x => x is eSwitch))
            {
                if (sw.pos > lowerbound && sw.pos < upperbound)
                {
                    foreach (var c in sw.connection)
                    {
                        
                        string trackid = c.refConnection.FindParent(typeof(eTrack)).id;
                        if (vertices.ContainsKey(trackid))
                        {
                            Vertex vert = vertices[trackid];
                            decimal dist = v.dist + Math.Abs(v.pos - sw.pos);
                            if (dist < vert.dist)
                            {
                                vert.prev = v;
                                vert.prevpos = sw.pos;
                                vert.dist = dist;
                                vert.pos = c.refConnection.GetParent().pos;
                                vert.route = v.route;
                                vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = sw.pos });
                            }
                        }
                       
                    }
                }
            }
            //if (v.track.mainDir != tExtendedDirection.down)
            //{
                if(v.track.trackTopology.trackBegin.Item is tConnectionData)
                { 
                    tConnectionData c = ((tConnectionData)v.track.trackTopology.trackBegin.Item);
                    string trackid = c.refConnection.FindParent(typeof(eTrack)).id;
                    if (vertices.ContainsKey(trackid))
                    {
                        Vertex vert = vertices[trackid];
                        decimal dist = v.dist + Math.Abs(v.pos - v.track.trackTopology.trackBegin.pos);
                        if (dist < vert.dist)
                        {
                            vert.prev = v;
                            vert.prevpos = v.track.trackTopology.trackBegin.pos;
                            vert.dist = dist;
                            vert.pos = c.refConnection.GetParent().pos;
                            vert.route = v.route;
                            vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = v.track.trackTopology.trackBegin.pos });
                        }
                    }
                }
                
                
            //}
            //if (v.track.mainDir != tExtendedDirection.up)
            //{
                if (v.track.trackTopology.trackEnd.Item is tConnectionData)
                {
                    tConnectionData c = ((tConnectionData)v.track.trackTopology.trackEnd.Item);
                    string trackid = c.refConnection.FindParent(typeof(eTrack)).id;
                    if (vertices.ContainsKey(trackid))
                    {
                        Vertex vert = vertices[trackid];
                        decimal dist = v.dist + Math.Abs(v.pos - v.track.trackTopology.trackEnd.pos);
                        if (dist < vert.dist)
                        {
                            vert.prev = v;
                            vert.prevpos = v.track.trackTopology.trackEnd.pos;
                            vert.dist = dist;
                            vert.pos = c.refConnection.GetParent().pos;
                            vert.route = v.route;
                            vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = v.track.trackTopology.trackEnd.pos });
                        }
                    }
                }
               
            //}

            return false;
        }

        public double PercentageDoubleTrack()
        {
            if(route.Count == 0)
            {
                return 0;
            }
            decimal totallength = 0;
            decimal doublelength = 0;
            foreach (var p in route)
            {
                totallength += Math.Abs(p.begin - p.end);
                if (p.track.mainDir == RailML.tExtendedDirection.up || p.track.mainDir == RailML.tExtendedDirection.down)
                {
                    doublelength += Math.Abs(p.begin - p.end);
                }
            }
            if (totallength > 0)
            {
                return (double)(doublelength / totallength);
            }
            return 0;
        }

        public double AverageSpeed(bool down)
        {
            if (route.Count == 0)
            {
                return 0;
            }
            decimal totaldist = 0;
            decimal speedsum = 0;
            foreach(var part in route)
            {
                tStrictDirection dir = tStrictDirection.down;
                if(part.begin > part.end && !down || part.begin < part.end && down)
                {
                    dir = tStrictDirection.up;
                }
                decimal pos = Math.Min(part.begin, part.end);
                decimal speed = part.track.trackElements.speedChanges
                    .Where(x => x.pos <= pos && (x.dirSpecified == false || x.dir == dir)).Any() ? 
                    part.track.trackElements.speedChanges
                    .Where(x => x.pos <= pos && (x.dirSpecified == false || x.dir == dir))
                    .Last().vMax : 0;
                foreach(var rest in part.track.trackElements.speedChanges
                    .Where(x => (x.dirSpecified == false || x.dir == dir) && x.pos > Math.Min(part.begin, part.end) && x.pos < Math.Max(part.begin, part.end)))
                {
                    speedsum += rest.vMax * (rest.pos - pos);
                    totaldist += (rest.pos - pos);
                    speed = rest.vMax;
                }
                speedsum += speed * (Math.Max(part.begin, part.end) - pos);
                totaldist += (Math.Max(part.begin, part.end) - pos);
                
            }
            if(totaldist > 0)
            {
                return (double)(speedsum / totaldist);
            }
            
            return 0;
        }

    }

    public class Vertex : IEquatable<Vertex>
    {
        public eTrack track { get; set; }
        public decimal dist { get; set; }
        public Vertex prev { get; set; }
        public decimal prevpos { get; set; }
        public decimal pos { get; set; }
        public List<RoutePart> route { get; set; }

        public Vertex()
        {
            route = new List<RoutePart>();
        }

        public bool Equals(Vertex other)
        {
            var othervertex = other as Vertex;
            return track.id == othervertex.track.id;
        }

        public override int GetHashCode()
        {
            return (int)dist;
        }
    }

    [Serializable]
    public class RoutePart
    {
        public eTrack track { get; set; }
        public decimal begin { get; set; }
        public decimal end { get; set; }
    }

}
