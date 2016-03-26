using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RailMLNeural.Data;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RailMLNeural.UI.Statistics.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class EdgeStatisticsViewModel : ViewModelBase
    {
        private ObservableCollection<StatisticsEdge> _edges;
        
        public ObservableCollection<StatisticsEdge> Edges
        {
            get { return _edges; }
            set { _edges = value;
            RaisePropertyChanged("Edges");
            }
        }
        /// <summary>
        /// Initializes a new instance of the EdgeStatisticsViewModel class.
        /// </summary>
        public EdgeStatisticsViewModel()
        {
            InitializeCommands();
            Edges = new ObservableCollection<StatisticsEdge>();
        }

        private void Refresh()
        {
            Edges.Clear();
            SimplifiedGraph Graph = new SimplifiedGraph();
            for(int i = 0; i < Graph.Edges.Count; i++)
            {
                Edges.Add(new StatisticsEdge());
                Edges[i].Origin = Graph.Edges[i].Origin.OCP.name;
                Edges[i].Destination = Graph.Edges[i].Destination.OCP.name;
                Edges[i].AverageMaxSpeedDown = Graph.Edges[i].AverageSpeedDown;
                Edges[i].AverageMaxSpeedUp = Graph.Edges[i].AverageSpeedUp;
            }
            for(int i = 0; i < (DataContainer.Settings.DataEndDate - DataContainer.Settings.DataStartDate).TotalDays; i++)
            {
                DateTime Date = DataContainer.Settings.DataStartDate.AddDays(i);
                DelayCombination DC = new DelayCombination();
                foreach(var dc in DataContainer.DelayCombinations.dict[Date] == null ? new List<DelayCombination>() : DataContainer.DelayCombinations.dict[Date])
                {
                    DC.primarydelays.AddRange(dc.primarydelays);
                    DC.secondarydelays.AddRange(dc.secondarydelays);
                }
                Graph.GenerateGraph(DC, true);
                for(int j = 0; j < Graph.Edges.Count; j++)
                {
                    foreach(var rep in Graph.Edges[j].Trains)
                    {
                        Edges[j].AddTrain(rep);
                    }
                }
            }
            RaisePropertyChanged("Edges");
        }


        #region Commands
        public ICommand RefreshCommand { get; private set; }

        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(Refresh);
        }

        #endregion Commands
    }

    public class StatisticsEdge
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int PassingTrainCount { get; set; }
        public double TotalDelayHours { get; set; }
        public double TotalPrimaryDelayHours { get; set; }
        public double TotalSecondaryDelayHours { get; set; }
        public double AverageMaxSpeedDown { get; set; }
        public double AverageMaxSpeedUp { get; set; }

        public void AddTrain(EdgeTrainRepresentation rep)
        {
            PassingTrainCount++;
            if(rep.ScheduledArrivalTime.TimeOfDay != rep.IdealArrivalTime.TimeOfDay)
            {
                double diff = (rep.IdealArrivalTime - rep.ScheduledArrivalTime).TotalHours;
                TotalDelayHours += diff;
                if(rep.PredictedArrivalTime.TimeOfDay != rep.IdealArrivalTime.TimeOfDay)
                {
                    TotalSecondaryDelayHours += diff;
                }
                else
                {
                    TotalPrimaryDelayHours += diff;
                }
            }
        }

        public double AverageDelaySeconds 
        {
            get { return TotalDelayHours * 3600 / PassingTrainCount; }
        }

        public double AveragePrimaryDelaySeconds
        {
            get { return TotalPrimaryDelayHours * 3600 / PassingTrainCount; }
        }

        public double AverageSecondaryDelaySeconds
        {
            get { return TotalSecondaryDelayHours * 3600 / PassingTrainCount; }
        }
    }
}