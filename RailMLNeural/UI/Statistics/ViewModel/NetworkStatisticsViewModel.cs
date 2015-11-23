using GalaSoft.MvvmLight;
using RailMLNeural.Data;
using RailMLNeural.RailML;
using System;
using System.Linq;
using System.Windows;

namespace RailMLNeural.UI.Statistics.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NetworkStatisticsViewModel : ViewModelBase
    {
        #region Parameters
        private decimal _totalTrackLength { get; set; }
        private decimal _mainSingleTrackLength { get; set; }
        private decimal _mainDoubleTrackLength { get; set; }
        private decimal _otherTrackLength { get; set; }
        private decimal _stationCount { get; set; }
        private decimal _switchCount { get; set; }
        public string TotalTrackLength
        {
            get
            {
                return _totalTrackLength.ToString();
            }
        }

        public string MainSingleTrackLength
        {
            get
            {
                return _mainSingleTrackLength.ToString(); 
            }
        }

        public string MainDoubleTrackLength
        {
            get
            {
                return _mainDoubleTrackLength.ToString();
            }
        }

        public string OtherTrackLength
        {
            get
            {
                return _otherTrackLength.ToString();
            }
        }

        public string StationCount
        {
            get
            {
                return _stationCount.ToString();
            }
        }

        public string SwitchCount
        {
            get
            {
                return _switchCount.ToString();
            }
        }
        #endregion Parameters

        #region Public
        /// <summary>
        /// Initializes a new instance of the NetworkStatisticsViewModel class.
        /// </summary>
        public NetworkStatisticsViewModel()
        {
            Initialize();
        }
        #endregion Public

        public void Loaded()
        {
            Initialize();
        }

        #region Private
        private void Initialize()
        {
            _totalTrackLength = 0;
            _mainDoubleTrackLength = 0;
            _mainSingleTrackLength = 0;
            _otherTrackLength = 0;
            _switchCount = 0;
            _stationCount = 0;
            if(DataContainer.model != null)
            {
                foreach(eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    _totalTrackLength += track.trackTopology.trackEnd.pos - track.trackTopology.trackBegin.pos;
                    if(track.type == "mainTrack")
                    {
                        if(track.mainDir == tExtendedDirection.down || track.mainDir == tExtendedDirection.up)
                        {
                            _mainDoubleTrackLength += (track.trackTopology.trackEnd.pos - track.trackTopology.trackBegin.pos)/2;
                        }
                        else
                        {
                            _mainSingleTrackLength += track.trackTopology.trackEnd.pos - track.trackTopology.trackBegin.pos;
                        }
                    }
                    else
                    {
                        _otherTrackLength += track.trackTopology.trackEnd.pos - track.trackTopology.trackBegin.pos;
                    }
                    foreach(eSwitch sw in track.trackTopology.connections)
                    {
                        _switchCount++;
                    }
                }
                foreach(eOcp ocp in DataContainer.model.infrastructure.operationControlPoints.Where(x => x.geoCoord.coord.Count == 2))
                {
                    _stationCount++;
                }
            }
            RaisePropertyChanged("TotalTrackLength");
            RaisePropertyChanged("MainSingleTrackLength");
            RaisePropertyChanged("MainDoubleTrackLength");
            RaisePropertyChanged("OtherTrackLength");
            RaisePropertyChanged("StationCount");
            RaisePropertyChanged("SwitchCount");

        }
        #endregion Private
    }
}