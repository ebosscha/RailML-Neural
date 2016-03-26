using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using RailMLNeural.Data;
using RailMLNeural.Neural;
using RailMLNeural.Neural.PreProcessing;
using RailMLNeural.RailML;
using RailMLNeural.UI.Dialog.View;
using RailMLNeural.UI.Model;
using RailMLNeural.UI.Neural.Views;
using RailMLNeural.UI.RailML.ViewModel;
using RailMLNeural.UI.RailML.Views;
using RailMLNeural.UI.Statistics.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RailMLNeural.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        private string _statustext;

        private UserControl _currentViewModel;
        private MainRailMLView _mainRailMLView = new MainRailMLView();
        private MainNeuralView _mainNeuralView = new MainNeuralView();
        private MainStatisticsView _statisticsView = new MainStatisticsView();

        public UserControl CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if(_currentViewModel == value)
                { return; }
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public string StatusText
        {
            get { return _statustext; }
            set
            {
                if(_statustext == value)
                { return; }
                _statustext = value;
                RaisePropertyChanged("StatusText");
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                });
            InitializeFileMenuCommands();
            InitializeProjectMenuCommands();
            InitializeDataMenuCommands();
            InitializeNeuralNetworkCommands();
            InitializeViewCommands();
            CurrentViewModel = _mainRailMLView;
        }

        #region FileMenuCommands
        public ICommand NewProjectCommand { get; private set; }
        public ICommand ProjectLoadCommand { get; private set; }
        public ICommand ProjectSaveCommand { get; private set; }
        public ICommand RailMLLoadCommand { get; private set; }
        public ICommand RailMLSaveCommand { get; private set; }
        public ICommand InfrastructureLoadCommand { get; private set; }
        public ICommand RollingstockLoadCommand { get; private set; }
        public ICommand TimetableLoadCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        

        private void InitializeFileMenuCommands()
        {
            NewProjectCommand = new RelayCommand(ExecuteNewProject);
            ProjectLoadCommand = new RelayCommand(ExecuteLoadProject);
            ProjectSaveCommand = new RelayCommand(ExecuteSaveProject);
            RailMLSaveCommand = new RelayCommand(ExecuteSaveRailML);
            RailMLLoadCommand = new RelayCommand(ExecuteLoadRailML);
            InfrastructureLoadCommand = new RelayCommand(ExecuteLoadInfrastructure);
            RollingstockLoadCommand = new RelayCommand(ExecuteLoadRollingstock);
            TimetableLoadCommand = new RelayCommand(ExecuteLoadTimetable);
            TestCommand = new RelayCommand(ExecuteTest);
            ExitCommand = new RelayCommand(ExecuteExit);
        }

        private void ExecuteNewProject()
        {
            DataContainer.model = new railml();
            DataContainer.NeuralConfigurations = new List<INeuralConfiguration>();
            DataContainer.PathContainer = new PathContainer();
            DataContainer.DelayCombinations = new DelayCombinationCollection();
            DataContainer.HeaderRoutes = new Dictionary<string, Dictionary<DateTime, string>>();
        }

        #region LoadButton
        private void ExecuteLoadProject()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "Document"; // Default file name
            dlg.Filter = "RailML NN Documents (.railNN)|*.railNN"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                using (BackgroundWorker worker = new BackgroundWorker())
                {
                    worker.WorkerReportsProgress = true;
                    //SaveLoad.ProtoLoadFile(worker, new DoWorkEventArgs(filename));
                    worker.ProgressChanged += new ProgressChangedEventHandler(LoadButton_ProgressChanged);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadButton_Completed);
                    worker.DoWork += new DoWorkEventHandler(SaveLoad.ProtoLoadFile);
                    worker.RunWorkerAsync(filename);
                    Application.Current.MainWindow.IsHitTestVisible = false;
                    Mouse.OverrideCursor = Cursors.AppStarting;
                }
            }
        }

        private void LoadButton_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusText = "Percentage Loaded : " + e.UserState.ToString();
        }

        private void LoadButton_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.MainWindow.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusText = null;
            SaveLoad.worker = null;
            DataContainer.PathContainer = new PathContainer();
        }
        #endregion LoadButton


        #region SaveButton
        private void ExecuteSaveProject()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.AddExtension = true;
            dlg.DefaultExt = ".railNN";
            dlg.Filter = "RailML Documents (.railNN.)|*.railNN"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                
                string filename = dlg.FileName;
                using (BackgroundWorker worker = new BackgroundWorker())
                {
                    worker.WorkerReportsProgress = true;
                    //SaveLoad.ProtoSaveToFile(worker, new DoWorkEventArgs(filename));
                    worker.ProgressChanged += new ProgressChangedEventHandler(SaveButton_ProgressChanged);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SaveButton_Completed);
                    worker.DoWork += new DoWorkEventHandler(SaveLoad.ProtoSaveToFile);
                    worker.RunWorkerAsync(filename);
                    Application.Current.MainWindow.IsHitTestVisible = false;
                    Mouse.OverrideCursor = Cursors.AppStarting;
                }
                
            }
        }

        private void SaveButton_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                StatusText = "Bytes Saved : " + e.UserState.ToString();
            }
            else { MessageBox.Show(e.UserState.ToString()); }
        }

        private void SaveButton_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.MainWindow.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusText = null;
            SaveLoad.worker = null;
        }
        #endregion SaveButton


        private void ExecuteLoadRailML()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "Document"; // Default file name
            dlg.Filter = "RailML Documents (.xml, .railml.)|*.xml;*.railml"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                DataContainer.model = XML.ImportFile(filename);
                DataContainer.PathContainer = new PathContainer();
            }
        }

        private void ExecuteSaveRailML()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.AddExtension = true;
            dlg.DefaultExt = ".xml";
            dlg.Filter = "RailML Documents (.xml, .railml.)|*.xml;*.railml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                XML.WriteToFile(filename);
            }
        }

        private void ExecuteLoadInfrastructure()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "Document"; // Default file name
            dlg.Filter = "RailML Documents (.xml, .railml.)|*.xml;*.railml"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                DataContainer.model.infrastructure = XML.ImportFile(filename).infrastructure;
                DataContainer.model = DataContainer.model;
                DataContainer.PathContainer = new PathContainer();
            }
        }

        private void ExecuteLoadRollingstock()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "Document"; // Default file name
            dlg.Filter = "RailML Documents (.xml, .railml.)|*.xml;*.railml"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                DataContainer.model.rollingstock = XML.ImportFile(filename).rollingstock;
                DataContainer.model = DataContainer.model;
            }
        }

        private void ExecuteLoadTimetable()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "Document"; // Default file name
            dlg.Filter = "RailML Documents (.xml, .railml.)|*.xml;*.railml"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                DataContainer.model.timetable = XML.ImportFile(filename).timetable;
                DataContainer.model = DataContainer.model;
            }
        }

        private void ExecuteTest()
        {
            DataContainer.model = new railml();
            eTrack track1 = new eTrack { trackTopology = new eTrackTopology { trackBegin = new eTrackBegin { pos = 0, geoCoord = new tGeoCoord() }, trackEnd = new eTrackEnd { pos = 600, geoCoord = new tGeoCoord() } } };
            DataContainer.IDGenerator(track1);
            track1.trackTopology.trackBegin.geoCoord.coord.Add(0); track1.trackTopology.trackBegin.geoCoord.coord.Add(0);
            track1.trackTopology.trackEnd.geoCoord.coord.Add(600); track1.trackTopology.trackEnd.geoCoord.coord.Add(0);
            eSwitch sw1 = new eSwitch() { id = "sw1" };
            sw1.geoCoord.coord.AddRange(new double[] { 300, 0 });
            track1.trackTopology.connections.Add(sw1);

            DataContainer.model.infrastructure.tracks.Add(track1);

            eTrack track2 = new eTrack { trackTopology = new eTrackTopology { trackBegin = new eTrackBegin { pos = 0, geoCoord = new tGeoCoord() }, trackEnd = new eTrackEnd { pos = 600, geoCoord = new tGeoCoord() } } };
            DataContainer.IDGenerator(track2);
            track2.trackTopology.trackBegin.geoCoord.coord.Add(200); track2.trackTopology.trackBegin.geoCoord.coord.Add(200);
            track2.trackTopology.trackEnd.geoCoord.coord.Add(600); track2.trackTopology.trackEnd.geoCoord.coord.Add(600);

            eTrack track3 = new eTrack { trackTopology = new eTrackTopology { trackBegin = new eTrackBegin { pos = 0, geoCoord = new tGeoCoord() }, trackEnd = new eTrackEnd { pos = 600, geoCoord = new tGeoCoord() } } };
            DataContainer.IDGenerator(track3);
            track3.trackTopology.trackBegin.geoCoord.coord.Add(-200); track3.trackTopology.trackBegin.geoCoord.coord.Add(-200);
            track3.trackTopology.trackEnd.geoCoord.coord.Add(-600); track3.trackTopology.trackEnd.geoCoord.coord.Add(-600);

            eTrack track4 = new eTrack { trackTopology = new eTrackTopology { trackBegin = new eTrackBegin { pos = 0, geoCoord = new tGeoCoord() }, trackEnd = new eTrackEnd { pos = 600, geoCoord = new tGeoCoord() } } };
            DataContainer.IDGenerator(track3);
            track4.trackTopology.trackBegin.geoCoord.coord.Add(600); track4.trackTopology.trackBegin.geoCoord.coord.Add(200);
            track4.trackTopology.trackEnd.geoCoord.coord.Add(200); track4.trackTopology.trackEnd.geoCoord.coord.Add(600);

            DataContainer.model.infrastructure.tracks.Add(track2);
            DataContainer.model.infrastructure.tracks.Add(track3);
            DataContainer.model.infrastructure.tracks.Add(track4);

            //Data.Editor.AddCrossing(track1, track2, 70, 300, 350);

            eOcp ocp1 = new eOcp { id = "34", name = "test1", geoCoord = new tGeoCoord() };
            ocp1.geoCoord.coord.Add(0); ocp1.geoCoord.coord.Add(0);
            DataContainer.model.infrastructure.operationControlPoints.Add(ocp1);

            eOcp ocp2 = new eOcp { id = "443", name = "test2", geoCoord = new tGeoCoord() };
            ocp2.geoCoord.coord.Add(600); ocp2.geoCoord.coord.Add(0);
            DataContainer.model.infrastructure.operationControlPoints.Add(ocp2);

            eOcp ocp3 = new eOcp { id = "344", name = "test3", geoCoord = new tGeoCoord() };
            ocp3.geoCoord.coord.Add(600); ocp3.geoCoord.coord.Add(20);
            DataContainer.model.infrastructure.operationControlPoints.Add(ocp3);

            eOcp ocp4 = new eOcp { id = "343", name = "test3", geoCoord = new tGeoCoord() };
            ocp4.geoCoord.coord.Add(0); ocp4.geoCoord.coord.Add(20);
            DataContainer.model.infrastructure.operationControlPoints.Add(ocp4);

            //DataContainer.PrepareData();

            DataContainer.model = DataContainer.model;

            //dynamic result = DataContainer.model.infrastructure.tracks[2].trackTopology.trackBegin.FindParent(typeof(eTrack));
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        #endregion FileMenuCommands

        #region ProjectMenuCommands
        public ICommand ProjectOptionsCommand { get; private set; }

        private void InitializeProjectMenuCommands()
        {
            ProjectOptionsCommand = new RelayCommand(ExecuteProjectOptions);
        }

        private void ExecuteProjectOptions()
        {
            var settingsdialog = new Dialog.View.ProjectOptionsView();
            settingsdialog.ShowDialog();

        }
        #endregion ProjectMenuCommands

        #region DataMenuCommands
        public ICommand ExcelTracksCommand { get; private set; }
        public ICommand ExcelSwitchesCommand { get; private set; }
        public ICommand ExcelBridgesCommand { get; private set; }
        public ICommand ExcelBufferStopsCommand { get; private set; }
        public ICommand ExcelTunnelsCommand { get; private set; }
        public ICommand ExcelLevelCrossingsCommand { get; private set; }
        public ICommand ExcelSignalsCommand { get; private set; }
        public ICommand ExcelSpeedRestrictionsCommand { get; private set; }
        public ICommand APIOCPCommand { get; private set; }
        public ICommand ExcelOCPCommand { get; private set; }
        public ICommand ExcelRollingstockCommand { get; private set; }
        public ICommand TimetableCSVCommand { get; private set; }
        public ICommand ConnectTracksCommand { get; private set; }
        public ICommand DebugDataCommand { get; private set; }

        private void InitializeDataMenuCommands()
        {
            ExcelTracksCommand = new RelayCommand(ExecuteExcelTracks);
            ExcelSwitchesCommand = new RelayCommand(ExecuteExcelSwitches);
            ExcelBridgesCommand = new RelayCommand(ExecuteExcelBridges);
            ExcelBufferStopsCommand = new RelayCommand(ExecuteExcelBufferStops);
            ExcelTunnelsCommand = new RelayCommand(ExecuteExcelTunnels);
            ExcelSignalsCommand = new RelayCommand(ExecuteExcelSignals);
            ExcelSpeedRestrictionsCommand = new RelayCommand(ExecuteExcelSpeedRestrictions);
            ExcelLevelCrossingsCommand = new RelayCommand(ExecuteExcelLevelCrossings);
            APIOCPCommand = new RelayCommand(ExecuteAPIOCP);
            ExcelOCPCommand = new RelayCommand(ExecuteExcelOCP);
            TimetableCSVCommand = new RelayCommand(ExecuteTimetableCSV);
            DebugDataCommand = new RelayCommand(ExecuteDebugData);
            ConnectTracksCommand = new RelayCommand(() => ImportInfrastructure.ConnectTracks());
            ExcelRollingstockCommand = new RelayCommand(ExecuteExcelRollingstock);
        }

        private void ExecuteExcelTracks()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;
                Data.ImportInfrastructure.ImportFromExcel(filename);
            }
        }

        private void ExecuteExcelSwitches()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddSwitchesFromExcel(filename);
            }
        }

        private void ExecuteExcelBridges()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddBridgesFromExcel(filename);
            }
        }

        private void ExecuteExcelBufferStops()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddBufferStopsFromExcel(filename);
            }
        }

        private void ExecuteExcelTunnels()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddTunnelsFromExcel(filename);
            }
        }

        private void ExecuteExcelSignals()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddSignalsFromExcel(filename);
            }
        }

        private void ExecuteExcelSpeedRestrictions()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddSpeedRestrictionsFromExcel(filename);
            }
        }

        private void ExecuteExcelLevelCrossings()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddLevelCrossingsFromExcel(filename);
            }
        }

        private void ExecuteAPIOCP()
        {
            Data.ImportInfrastructure.OCPfromAPI();
        }

        private void ExecuteExcelOCP()
        {
            Data.ImportInfrastructure.OCPfromExcel();
        }

        private void ExecuteExcelRollingstock()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportTimetable.ImportTrainTypesExcel(filename);
            }
        }

        private void ExecuteTimetableCSV()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text Documents (.txt)|*.txt"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                //ImportTimetable.TimetableFromCsv(worker, new DoWorkEventArgs(filename));
                worker.DoWork += new DoWorkEventHandler(ImportTimetable.TimetableFromCsv);
                worker.ProgressChanged += new ProgressChangedEventHandler(ImportTimetable_Progress);
                worker.RunWorkerAsync(filename);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportTimetable_Finished);

                Application.Current.MainWindow.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void ImportTimetable_Progress(object sender, ProgressChangedEventArgs e)
        {
            string[] state = e.UserState as string[];
            StatusText = "Current Date: " + state[0] + "      Lines Processed: " + state[1];
        }

        private void ImportTimetable_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show("Number of unhandled lines: " + e.Result.ToString());
                Application.Current.MainWindow.IsHitTestVisible = true;
                Mouse.OverrideCursor = null;
                StatusText = null;
            }
            else
            {
                string message = e.Error.Message;
                if (e.Error.InnerException != null)
                { message += "    Inner Exception : " + e.Error.InnerException.Message; }
                MessageBox.Show(message + " Error Line: " + e.Error.TargetSite.ToString());
            }
        }

        private void ExecuteDebugData()
        {
            DebugWindow dw = new DebugWindow();
            dw.Show();
        }
        
        
        #endregion DataMenuCommands

        #region NeuralNetworkCommands
        public ICommand ImportDelaysCommand { get; private set; }
        public ICommand ImportHeaderHistoryCommand { get; private set; }

        private void InitializeNeuralNetworkCommands()
        {
            ImportDelaysCommand = new RelayCommand(ExecuteImportDelays);
            ImportHeaderHistoryCommand = new RelayCommand(ExecuteImportHeaderHistory);
        }

        private void ExecuteImportDelays()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text Documents (.txt)|*.txt"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                //NeuralNetwork.PreProcessing.Import.ImportDelayCombinations(worker, new DoWorkEventArgs(filename));
                worker.DoWork += new DoWorkEventHandler(Import.ImportDelayCombinations);
                worker.ProgressChanged += new ProgressChangedEventHandler(ImportDelays_Progress);
                worker.RunWorkerAsync(filename);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportDelays_Finished);

                Application.Current.MainWindow.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void ImportDelays_Progress(object sender, ProgressChangedEventArgs e)
        {
            string[] state = e.UserState as string[];
            StatusText = "Current Date: " + state[0] + "      Lines Processed: " + state[1];
        }

        private void ImportDelays_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.MainWindow.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusText = null;
        }

        private void ExecuteImportHeaderHistory()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text Documents (.txt)|*.txt"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                string filename = dlg.FileName;

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                //NeuralNetwork.PreProcessing.Import.ImportHeaderHistory(worker, new DoWorkEventArgs(filename));
                worker.DoWork += new DoWorkEventHandler(Import.ImportHeaderHistory);
                worker.ProgressChanged += new ProgressChangedEventHandler(ImportHeaderHistory_Progress);
                worker.RunWorkerAsync(filename);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportHeaderHistory_Finished);

                Application.Current.MainWindow.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void ImportHeaderHistory_Progress(object sender, ProgressChangedEventArgs e)
        {
            StatusText = e.UserState as string;
        }

        private void ImportHeaderHistory_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.MainWindow.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusText = null;        
        }

        #endregion NeuralNetworkCommands

        #region ViewCommands
        public ICommand RailMLViewCommand { get; private set; }
        public ICommand NeuralViewCommand { get; private set; }
        public ICommand StatisticsViewCommand { get; private set; }
        public ICommand LogViewCommand { get; private set; }

        private void InitializeViewCommands()
        {
            RailMLViewCommand = new RelayCommand(ExecuteRailMLView);
            NeuralViewCommand = new RelayCommand(ExecuteNeuralView);
            StatisticsViewCommand = new RelayCommand(ExecuteStatisticsView);
            LogViewCommand = new RelayCommand(ExecuteLogView);

        }

        private void ExecuteRailMLView()
        {
            CurrentViewModel = _mainRailMLView;
        }

        private void ExecuteNeuralView()
        {
            CurrentViewModel = _mainNeuralView;
        }

        private void ExecuteStatisticsView()
        {
            CurrentViewModel = _statisticsView;
        }

        private void ExecuteLogView()
        {
            Thread LogThread = new Thread(() =>
                {
                    if (!Logger.Logger.IsActive)
                    {
                        Logger.Logger.View.Show();
                        Logger.Logger.View.Closed += (sender2, e2) =>
                            Logger.Logger.View.Dispatcher.InvokeShutdown();
                        System.Windows.Threading.Dispatcher.Run();
                    }
                }
            );
            LogThread.SetApartmentState(ApartmentState.STA);
            LogThread.Start();
        }

        #endregion ViewCommands

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}