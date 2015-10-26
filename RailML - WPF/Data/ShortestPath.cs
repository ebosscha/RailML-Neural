using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.Data
{
    static class ShortestPath
    {
        public  static Route FindRoute(dynamic origin, dynamic destination)
        {
            eTrack starttrack;
            Route route = new Route();
            if(origin.GetType == typeof(eTrack))
            {
                starttrack = origin;
            }

            


            return route;
        }

        private static List<dynamic> GetConnections(eTrack track, double pos)
        {
            List<dynamic> list = new List<dynamic>();
            if (track.trackTopology.trackBegin.Item != null && track.trackTopology.trackBegin.Item is tConnectionData  )
            {
                list.Add(DataContainer.GetItem(((tConnectionData)track.trackTopology.trackBegin.Item).@ref).FindParent(typeof(eTrack)));
            }
            if (track.trackTopology.trackBegin.Item != null && track.trackTopology.trackEnd.Item is tConnectionData)
            {
                list.Add(DataContainer.GetItem(((tConnectionData)track.trackTopology.trackEnd.Item).@ref).FindParent(typeof(eTrack)));
            }
            foreach(eSwitch sw in track.trackTopology.connections)
            {
                foreach(tSwitchConnectionData conn in sw.connection)
                {
                    
                }
            }

                return list;

        }


    
        
    }

    public class Route
    {
        public List<eTrack> tracks {get; set;}
        public double distance {get;set;}


        public Route()
        {
            tracks = new List<eTrack>();
        }
    }
}
