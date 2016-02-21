using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace RailMLNeural.Data
{
    [Serializable]
    [ProtoContract]
    public class Settings
    {   
        [ProtoMember(1)]
        private DateTime _dataStartDate;
        [ProtoMember(2)]
        private DateTime _dataEndDate;
        [ProtoMember(3)]
        private bool _useDateFilter;

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

        public bool UseDateFilter
        {
            get { return _useDateFilter; }
            set { _useDateFilter = value; }
        }

        public Settings()
        {
            DataStartDate = DateTime.Now;
            DataEndDate = DateTime.Now;
        }
    }
}
