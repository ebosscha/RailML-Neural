using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RailMLNeural.Data;
using RailMLNeural.RailML;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace RailMLNeural.UI.Dialog.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DebugViewModel : ViewModelBase
    {
        public List<string> TrackIDs { get; set; }
        public string SelectedTrack {get; set;}
        /// <summary>
        /// Initializes a new instance of the DebugViewModel class.
        /// </summary>
        public DebugViewModel()
        {
            InitializeCommands();
            Start();
        }

        public void Start()
        {
            Data.DebugData.StartDebug();
            SetTrackList();
        }

        private void SetTrackList()
        {
            TrackIDs = new List<string>();
            foreach(eTrack track in DataContainer.model.infrastructure.tracks)
            {
                TrackIDs.Add(track.id);
            }
        }

        #region Commands
        public ICommand DebugNextCommand { get; private set; }
        public ICommand DebugPreviousCommand { get; private set; }
        public ICommand ExitDebugCommand { get; private set; }
        public ICommand DebugTrackCommand { get; private set; }

        private void InitializeCommands()
        {
            DebugNextCommand = new RelayCommand(() => Data.DebugData.DebugNext(), DebugData.HasNext);
            DebugPreviousCommand = new RelayCommand(() => Data.DebugData.DebugPrevious(), DebugData.HasPrevious);
            ExitDebugCommand = new RelayCommand<Window>((param) => ExecuteExit(param));
            DebugTrackCommand = new RelayCommand(ExecuteDebugTrack);
        }

        private void ExecuteExit(Window window)
        {
            window.Close();
        }

        private void ExecuteDebugTrack()
        {
            DebugData.DebugTrack(SelectedTrack);
        }
        #endregion Commands
    }
}