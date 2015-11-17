using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace RailMLNeural.Data
{
    [ProtoContract]
    class Settings
    {   
        [ProtoMember(1)]
        private DateTime _dataStartDate;
        [ProtoMember(2)]
        private DateTime _dataEndDate;

        public DateTime DataStartDate
        {
            get { return _dataStartDate; }
            set { _dataStartDate = value; }
        }
        
        public DateTime DataEndDate
        {
            get { return _dataEndDate; }
            set { _dataEndDate = value; }
        }

    }
}
