using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Data
{
    [ProtoContract]
    class MetaData
    {
        [ProtoMember(1)]
        public bool DelayCombinationsSpecified
        {
            get
            {
                return (DataContainer.DelayCombinations != null &&
                    DataContainer.DelayCombinations.dict.Values.Count > 0);
            }
        }
        [ProtoMember(2)]
        public bool TimeTableSpecified 
        { get
            {
                return (DataContainer.model != null && 
                    DataContainer.model.timetable.trainGroups.Count > 0);
            }
        }
        [ProtoMember(3)]
        public bool HeaderHistorySpecified
        {
            get
            {
                return (DataContainer.HeaderRoutes != null &&
                    DataContainer.HeaderRoutes.Count > 0);
            }
        }
        
        [ProtoMember(4)]
        public DateTime LastEditTime { get; set; }
    
    }
}
