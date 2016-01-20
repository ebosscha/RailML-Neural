using GalaSoft.MvvmLight;
using RailMLNeural.UI.Statistics.View;

namespace RailMLNeural.UI.Statistics.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainStatisticsViewModel : ViewModelBase
    {
        private DelayStatisticsView _delayStatistics;
        public DelayStatisticsView DelayStatistics
        {
            get
            {
                return _delayStatistics;
            }
            set
            {
                _delayStatistics = value;
                RaisePropertyChanged("DelayStatistics");
            }
        }

        private NetworkStatisticsView _networkStatistics;
        public NetworkStatisticsView NetworkStatistics
        {
            get
            {
                return _networkStatistics;
            }
            set
            {
                _networkStatistics = value;
                RaisePropertyChanged("NetworkStatistics");
            }
        }
        /// <summary>
        /// Initializes a new instance of the MainStatisticsViewModel class.
        /// </summary>
        public MainStatisticsViewModel()
        {
            DelayStatistics = new DelayStatisticsView();
            NetworkStatistics = new NetworkStatisticsView();
        }
    }
}