using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.Data
{
    static class ShortestPath
    {
    }

    class PathContainer
    {
        private Dictionary<string, Dictionary<string, Route>> dict;
        public PathContainer()
        {
            Recalculate();
        }

        public void Recalculate()
        {
            dict = new Dictionary<string, Dictionary<string, Route>>();
            foreach (eOcp ocp in DataContainer.model.infrastructure.operationControlPoints)
            {
                Dictionary<string, Route> tempdict = new Dictionary<string, Route>();
                foreach (eOcp other in DataContainer.model.infrastructure.operationControlPoints.Where(e => e.id != ocp.id))
                {
                    tempdict.Add(other.id, new Route(ocp, other));
                }
                dict.Add(ocp.id, tempdict);
            }
        }

        public Route GetRoute(eOcp origin, eOcp destination)
        {
            return GetRoute(origin.id, destination.id);
        }

        public Route GetRoute(string originid, string destinationid)
        {
            return dict[originid][destinationid];
        }
    }

    class Route
    {
        private List<eTrack> unvisited;
        private List<double> distances;

        private infrastructure inf;
        public eOcp origin;
        public eOcp destination;
        public List<RoutePart> route;
        public Route(eOcp o, eOcp d)
        {
            origin = o; destination = d;
            inf = DataContainer.model.infrastructure;
            CalculateRoute();
        }

        private void CalculateRoute()
        {
            unvisited = inf.tracks;
            distances = Enumerable.Repeat(9999999999999999.0, unvisited.Count).ToList();

        }
    }

    class RoutePart
    {
        public eTrack track;
        public decimal begin;
        public decimal end;
    }

}
