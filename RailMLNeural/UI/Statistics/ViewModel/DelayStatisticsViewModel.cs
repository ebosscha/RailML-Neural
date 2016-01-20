using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using System.Collections.Generic;

namespace RailMLNeural.UI.Statistics.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DelayStatisticsViewModel : ViewModelBase
    {
        #region Members
        public bool DelayCombinationsDefined
        {
            get
            {
                return DataContainer.DelayCombinations != null;
            }
        }

        public int DelayCombinationCount
        {
            get
            {
                return DataContainer.DelayCombinations.CombinationCount;
            }
        }

        public int TotalDelayCount
        {
            get
            {
                return DataContainer.DelayCombinations.TotalDelayCount;
            }
        }

        public double AverageCountPerCombination
        {
            get
            {
                return DataContainer.DelayCombinations.AverageCountPerCombination;
            }
        }

        public List<int> SecondaryDelayCountHistogram
        {
            get
            {
                return DataContainer.DelayCombinations.DelayCountHistogram;
            }
        }

        public double AverageCombinationsPerDay
        {
            get
            {
                return DataContainer.DelayCombinations.AverageCombinationsPerDay;
            }
        }

        public double AverageDestinationDelay
        {
            get
            {
                return DataContainer.DelayCombinations.AverageDestinationDelay;
            }
        }
        #endregion Members
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public DelayStatisticsViewModel()
        {
            Messenger.Default.Register<DelayCombinationsChangedMessage>(this, (msg) => OnDelaysChanged(msg));
            OnDelaysChanged(new DelayCombinationsChangedMessage());             
        }

        private void OnDelaysChanged(DelayCombinationsChangedMessage msg)
        {
            RaisePropertyChanged("DelayCombinationsDefined");
            RaisePropertyChanged("DelayCombinationCount");
            RaisePropertyChanged("TotalDelayCount");
            RaisePropertyChanged("AverageCountPerCombination");
            RaisePropertyChanged("SecondaryDelayCountHistogram");
            RaisePropertyChanged("AverageCombinationsPerDay");
            RaisePropertyChanged("AverageDestinationDelay");
        }
    }
}