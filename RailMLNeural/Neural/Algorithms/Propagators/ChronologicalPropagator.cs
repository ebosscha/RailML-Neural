using RailMLNeural.Data;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Algorithms.Propagators
{
    class ChronologicalPropagator
    {
        #region Parameters
        private DateTime _currentDateTime;
        private List<eTrainPart> _trainParts;
        private List<Tuple<DateTime, eOcpTT>> _ocpTTs;
        private int _currentIndex;
        private List<string> primarytrainheaders;

        public bool HasNext
        {
            get
            {
                //TODO!!
                return true;
            }
        }
        #endregion Parameters

        #region Public
        public ChronologicalPropagator()
        {

        }

        public void NewCycle(DelayCombination dc)
        {
            SetDateTime(dc);
            _trainParts = DataContainer.model.timetable.GetTrainsByDay(_currentDateTime.Date);
            _ocpTTs = new List<Tuple<DateTime, eOcpTT>>();
            primarytrainheaders = new List<string>();
            foreach(Delay d in dc.primarydelays)
            {
                primarytrainheaders.Add(d.traincode);
            }
            foreach (eTrainPart trainPart in _trainParts)
            {
                foreach(eOcpTT ocpTT in trainPart.ocpsTT)
                {
                    _ocpTTs.Add(new Tuple<DateTime, eOcpTT>(ocpTT.times[0].arrival, ocpTT));
                    _ocpTTs.Add(new Tuple<DateTime, eOcpTT>(ocpTT.times[0].departure, ocpTT));
                }
            }
            _ocpTTs.OrderBy(x => x.Item1);
            _currentIndex = _ocpTTs.IndexOf(_ocpTTs.FirstOrDefault(x => x.Item1 >= _currentDateTime && !primarytrainheaders.Contains(x.Item2.GetParent().trainNumber)));
        }

        public object MoveNext()
        {
            if(HasNext)
            {
                //TODO! 
            }

            throw new Exception("Propagator can't move to next object");
        }
        #endregion Public

        #region Private
        private void SetDateTime(DelayCombination dc)
        {
            _currentDateTime = dc.GetDate();
            DateTime _time = DateTime.MaxValue;
            foreach(Delay d in dc.primarydelays)
            {
                if (d.ScheduledDeparture < _time) { _time = d.ScheduledDeparture; }
            }
            _currentDateTime = _currentDateTime + _time.TimeOfDay;
        }
        #endregion Private
    }
}
