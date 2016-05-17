using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RailMLNeural.Data;
using System.Collections.Generic;
using System.Windows.Input;

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

        public double AverageTotalPrimaryDelays
        {
            get
            {
                return DataContainer.DelayCombinations.AverageTotalPrimaryDelays;
            }
        }

        public double AverageTotalFirstOrderDelays
        {
            get
            {
                return DataContainer.DelayCombinations.AverageTotalFirstOrderDelays;
            }
        }

        public double AverageTotalSecondOrderDelays
        {
            get
            {
                return DataContainer.DelayCombinations.AverageTotalSecondOrderDelays;
            }
        }

        public double IsDelayedPercentage
        {
            get
            {
                return DataContainer.DelayCombinations.AverageIsDelayedPercentage;
            }
        }

        public double IsPrimaryDelayPercentage
        {
            get
            {
                return DataContainer.DelayCombinations.AverageIsPrimaryDelayedPercentage;
            }
        }

        public double HasKnockOnDelayPercentage
        {
            get
            {
                return DataContainer.DelayCombinations.HasKnockOnDelayedPercentage;
            }
        }
        #endregion Members
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public DelayStatisticsViewModel()
        {
            //Messenger.Default.Register<DelayCombinationsChangedMessage>(this, (msg) => OnDelaysChanged(msg));
            //OnDelaysChanged(new DelayCombinationsChangedMessage());   
            RefreshCommand = new RelayCommand(OnDelaysChanged);
        }

        private void OnDelaysChanged()
        {
            RaisePropertyChanged("DelayCombinationsDefined");
            RaisePropertyChanged("DelayCombinationCount");
            RaisePropertyChanged("TotalDelayCount");
            RaisePropertyChanged("AverageCountPerCombination");
            RaisePropertyChanged("SecondaryDelayCountHistogram");
            RaisePropertyChanged("AverageCombinationsPerDay");
            RaisePropertyChanged("AverageDestinationDelay");
            RaisePropertyChanged("AverageTotalPrimaryDelays");
            RaisePropertyChanged("AverageTotalFirstOrderDelays");
            RaisePropertyChanged("AverageTotalSecondOrderDelays");
            RaisePropertyChanged("IsDelayedPercentage");
            RaisePropertyChanged("IsPrimaryDelayPercentage");
            RaisePropertyChanged("HasKnockOnDelayPercentage");
        }

        public ICommand RefreshCommand { get; private set; }
    }

}