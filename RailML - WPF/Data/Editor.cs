using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF.Data
{
    static class Editor
    {
        public static void AddCrossing(eTrack track1, eTrack track2, double length, double pos1, double pos2)
        {
            string id1 = DataContainer.IDGenerator("Connection");
            string id2 = DataContainer.IDGenerator("Connection");
            string id3 = DataContainer.IDGenerator("Connection");
            string id4 = DataContainer.IDGenerator("Connection");
            
            tConnectionData switchtotrack1 = new tConnectionData(){id=id1, @ref=id2};
            tSwitchConnectionData track1toswitch = new tSwitchConnectionData(){id=id2, @ref=id1};
            tConnectionData switchtotrack2 = new tConnectionData(){id=id3, @ref=id4};
            tSwitchConnectionData track2toswitch = new tSwitchConnectionData(){id=id4, @ref=id3};
            
            eTrack track = new eTrack(){id=DataContainer.IDGenerator("track")};
            track.trackTopology.trackBegin.pos = 0;
            track.trackTopology.trackEnd.pos = (decimal)length;
            track.trackTopology.trackBegin.Item = switchtotrack1;
            track.trackTopology.trackEnd.Item = switchtotrack2;
            DataContainer.model.infrastructure.tracks.Add(track);

            eSwitch switch1 = new eSwitch() { pos = (decimal)pos1, id = DataContainer.IDGenerator("Switch") };
            switch1.connection.Add(track1toswitch);
            track1.trackTopology.connections.Add(switch1);
            eSwitch switch2 = new eSwitch() { pos = (decimal)pos2, id = DataContainer.IDGenerator("Switch") };
            switch1.connection.Add(track2toswitch);
            track2.trackTopology.connections.Add(switch2);
            
        }

        public static void ConnectEnd(eTrack track1, eTrack track2)
        {
            string id1 = DataContainer.IDGenerator("connection");
            string id2 = DataContainer.IDGenerator("connection");

            tConnectionData connection1 = new tConnectionData() { id = id1, @ref = id2 };
            tConnectionData connection2 = new tConnectionData() { id = id2, @ref = id1 };

            track1.trackTopology.trackEnd.Item = connection1;
            track2.trackTopology.trackBegin.Item = connection2;
        }
    }
}
