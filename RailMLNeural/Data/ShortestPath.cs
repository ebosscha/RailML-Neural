using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailMLNeural.Data
{
    class PathContainer
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
                dict[originid].Add(destinationid, r);
                return r;
            }
        }
    }

    class Route
    {
        private List<Vertex> vertices { get; set; }

        private infrastructure inf { get; set; }
        public eOcp origin { get; set; }
        public eOcp destination { get; set; }
        public List<RoutePart> route { get; set; }
        public decimal distance { get; set; }
        public Route(string originid, string destinationid)
        {
            vertices = new List<Vertex>();
            route = new List<RoutePart>();
            inf = DataContainer.model.infrastructure;
            origin = inf.operationControlPoints.Single(x => x.id == originid);
            destination = inf.operationControlPoints.Single(x => x.id == destinationid);

            CalculateRoute();
        }


        // TODO: Alter algorithm to calculate route to all OCP's at once. Faster than individual shortest path calculation.
        private void CalculateRoute()
        {

            // Set up vertices
            for (int i = 0; i < inf.tracks.Count; i++)
            {
                Vertex v = new Vertex { track = inf.tracks[i] };

                if (v.track.trackTopology.crossSections.Any(x => x.ocpRef == origin.id))
                {
                    v.pos = v.track.trackTopology.crossSections.Single(e => e.ocpRef == origin.id).pos;
                    v.dist = 0;
                }
                else { v.dist = (decimal)99999999999999999; }
                vertices.Add(v);

            }

            while (vertices.Count > 0)
            {
                Vertex v = vertices.Find(x => x.dist == vertices.Min(e => e.dist));
                if (HandleVertex(v))
                {
                    break;
                }
                vertices.RemoveAt(vertices.IndexOf(v));
            }
        }

        private bool HandleVertex(Vertex v)
        {
            decimal lowerbound = -9999999999999999;
            decimal upperbound = 9999999999999999;
            switch (v.track.mainDir)
            {
                case tExtendedDirection.up:
                    upperbound = v.pos;
                    break;
                case tExtendedDirection.down:
                    lowerbound = v.pos;
                    break;
                default:
                    break;
            }
            foreach (tCrossSection c in v.track.trackTopology.crossSections)
            {
                if (c.pos > lowerbound && c.pos < upperbound)
                {
                    if (c.ocpRef == destination.id)
                    {
                        distance = v.dist + Math.Abs(v.pos - c.pos);
                        route = v.route;
                        route.Add(new RoutePart() { track = v.track, begin = v.pos, end = c.pos });
                        return true;
                    }
                }
            }

            foreach (eSwitch sw in v.track.trackTopology.connections)
            {
                if (sw.pos > lowerbound && sw.pos < upperbound)
                {
                    foreach (var c in sw.connection)
                    {
                        Vertex vert = vertices.Single(e => e.ContainsConnection(c.@ref).Successful);
                        decimal dist = v.dist + Math.Abs(v.pos - sw.pos);
                        if (dist < vert.dist)
                        {
                            vert.prev = v;
                            vert.prevpos = sw.pos;
                            vert.dist = dist;
                            vert.pos = vert.ContainsConnection(c.@ref).pos;
                            vert.route = v.route;
                            vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = sw.pos });
                        }
                    }
                }
            }
            if (v.track.mainDir != tExtendedDirection.down)
            {
                try
                {
                    string cref = ((tConnectionData)v.track.trackTopology.trackBegin.Item).@ref;
                    Vertex vert = vertices.Single(e => e.ContainsConnection(cref).Successful);
                    decimal dist = v.dist + Math.Abs(v.pos - v.track.trackTopology.trackBegin.pos);
                    if (dist < vert.dist)
                    {
                        vert.prev = v;
                        vert.prevpos = v.track.trackTopology.trackBegin.pos;
                        vert.dist = dist;
                        vert.pos = vert.ContainsConnection(cref).pos;
                        vert.route = v.route;
                        vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = v.track.trackTopology.trackBegin.pos });
                    }
                }
                catch { }
            }
            if (v.track.mainDir != tExtendedDirection.up)
            {
                try
                {
                    string cref = ((tConnectionData)v.track.trackTopology.trackEnd.Item).@ref;
                    Vertex vert = vertices.Single(e => e.ContainsConnection(cref).Successful);
                    decimal dist = v.dist + Math.Abs(v.pos - v.track.trackTopology.trackEnd.pos);
                    if (dist < vert.dist)
                    {
                        vert.prev = v;
                        vert.prevpos = v.track.trackTopology.trackEnd.pos;
                        vert.dist = dist;
                        vert.pos = vert.ContainsConnection(cref).pos;
                        vert.route = v.route;
                        vert.route.Add(new RoutePart { track = vert.track, begin = v.pos, end = v.track.trackTopology.trackEnd.pos });
                    }
                }
                catch { }
            }

            return false;
        }

    }

    class Vertex : IEquatable<Vertex>
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

        public QueryResult ContainsConnection(string id)
        {
            try
            {
                if (((tConnectionData)track.trackTopology.trackBegin.Item).id == id)
                {
                    return new QueryResult() { Successful = true, pos = track.trackTopology.trackBegin.pos };
                }
            }
            catch { }
            try
            {
                if (((tConnectionData)track.trackTopology.trackBegin.Item).id == id)
                {
                    return new QueryResult() { Successful = true, pos = track.trackTopology.trackEnd.pos };
                }
            }
            catch { }
            foreach (eSwitch sw in track.trackTopology.connections)
            {
                if (sw.connection.Any(e => e.id == id)) { return new QueryResult() { Successful = true, pos = sw.pos }; }
            }
            return new QueryResult() { Successful = false };
        }
    }

    class QueryResult
    {
        public bool Successful;
        public decimal pos;
    }

    class RoutePart
    {
        public eTrack track { get; set; }
        public decimal begin { get; set; }
        public decimal end { get; set; }
    }

}
