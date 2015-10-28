using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RailML___WPF.RailMLViewer.Views;
using RailML___WPF.Data;
using RailML___WPF.NeuralNetwork.Views;
using System.Threading;
using System.ComponentModel;

namespace RailML___WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel viewmodel = new MainWindowViewModel();
            this.DataContext = viewmodel;
        }

        #region Filemenu
        #region LoadButton
        public void LoadButton_Click(object sender,EventArgs e)
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
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                //SaveLoad.ProtoLoadFile(worker, new DoWorkEventArgs(filename));
                worker.ProgressChanged += new ProgressChangedEventHandler(LoadButton_ProgressChanged);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadButton_Completed);
                worker.DoWork += new DoWorkEventHandler(SaveLoad.ProtoLoadFile);
                worker.RunWorkerAsync(filename);
                this.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;


                
            }
        }

        private void LoadButton_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusbarText.Text = "Percentage Loaded : " + e.UserState.ToString();
        }

        private void LoadButton_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusbarText.Text = null;
            this.MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }

        #endregion LoadButton

        #region SaveButton
        public void SaveButton_Click(object sender, EventArgs e)
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
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                //SaveLoad.ProtoSaveToFile(worker, new DoWorkEventArgs(filename));
                worker.ProgressChanged += new ProgressChangedEventHandler(SaveButton_ProgressChanged);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SaveButton_Completed);
                worker.DoWork += new DoWorkEventHandler(SaveLoad.ProtoSaveToFile);
                worker.RunWorkerAsync(filename);
                this.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void SaveButton_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                StatusbarText.Text = "Bytes Saved : " + e.UserState.ToString();
            }
            else { MessageBox.Show(e.UserState.ToString()); }
        }

        private void SaveButton_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusbarText.Text = null;
        }

        #endregion SaveButton

        public void LoadRailMLButton_Click(object sender, EventArgs e)
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

                this.MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
            }
        }

        public void LoadInfrastructure_Click(object sender, EventArgs e)
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

                this.MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
            }
        }

        public void LoadRollingStock_Click(object sender, EventArgs e)
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

                this.MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
            }
        }

        public void LoadTimetable_Click(object sender, EventArgs e)
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

                this.MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
            }
        }

        public void SaveRailMLButton_Click(object sender, EventArgs e)
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

        public void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void NewButton_Click(object sender, EventArgs e)
        {
            DataContainer.model = new railml();
        }

        public void TestButton_Click(object sender, EventArgs e)
        {
            DataContainer.model = new railml();
            eTrack track1 = new eTrack{trackTopology = new eTrackTopology{trackBegin=new eTrackBegin{pos=0, geoCoord=new tGeoCoord()}, trackEnd = new eTrackEnd{pos=600, geoCoord=new tGeoCoord()}}};
            DataContainer.IDGenerator(track1);
            track1.trackTopology.trackBegin.geoCoord.coord.Add(0); track1.trackTopology.trackBegin.geoCoord.coord.Add(0);
            track1.trackTopology.trackEnd.geoCoord.coord.Add(600); track1.trackTopology.trackEnd.geoCoord.coord.Add(0);
            eSwitch sw1 = new eSwitch() { id = "sw1" };
            sw1.geoCoord.coord.AddRange(new double[] { 300, 0 });
            track1.trackTopology.connections.Add(sw1);
            
            DataContainer.model.infrastructure.tracks.Add(track1);

            eTrack track2 = new eTrack{trackTopology = new eTrackTopology{trackBegin=new eTrackBegin{pos=0, geoCoord=new tGeoCoord()}, trackEnd = new eTrackEnd{pos=600, geoCoord=new tGeoCoord()}}};
            DataContainer.IDGenerator(track2);
            track2.trackTopology.trackBegin.geoCoord.coord.Add(6000); track2.trackTopology.trackBegin.geoCoord.coord.Add(20);
            track2.trackTopology.trackEnd.geoCoord.coord.Add(8000); track2.trackTopology.trackEnd.geoCoord.coord.Add(20);
            
            
            DataContainer.model.infrastructure.tracks.Add(track2);

            Data.Editor.AddCrossing(track1, track2, 70, 300, 350);

            eOcp ocp1 = new eOcp { id = "34", name= "test1", geoCoord = new tGeoCoord() };
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

            DataContainer.PrepareData();

            dynamic result = DataContainer.model.infrastructure.tracks[2].trackTopology.trackBegin.FindParent(typeof(eTrack));

            this.MainViewContentControl.Content = new BaseRailMLView();

        }
        #endregion

        #region View
        public void RailMLView_Click(object sender, EventArgs e)
        {
            this.MainViewContentControl.Content = new BaseRailMLView();
        }

        public void NeuralNetworkView_Click(object sender, EventArgs e)
        {
            this.MainViewContentControl.Content = new BaseNeuralNetworkView();
        }
        #endregion

        #region Data Imports

        #region Infrastructure Imports
        public void ImportExcel_Click(object sender, EventArgs e)
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
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();

        }

        public void ExcelSwitches_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddSwitchesFromExcel(filename);
            }
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();

        }

        public void ExcelBridges_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddBridgesFromExcel(filename);
            }
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }

        public void ExcelBufferStops_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddBufferStopsFromExcel(filename);
            }
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }

        public void ExcelTunnels_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddTunnelsFromExcel(filename);
            }
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }

        public void ExcelLevelCrossings_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Excel Documents (.xlsx)|*.xlsx"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                Data.ImportInfrastructure.AddLevelCrossingsFromExcel(filename);
            }
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }

        public void OCPAPI_Click(object sender, EventArgs e)
        {
            Data.ImportInfrastructure.OCPfromAPI();
            MainViewContentControl.Content = new RailMLViewer.Views.BaseRailMLView();
        }
        #endregion

        #region Timetable Imports
        public void ImportTimetableCSV_Click(object sender, EventArgs e)
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
                ImportTimetable.TimetableFromCsv(worker, new DoWorkEventArgs(filename));
                worker.DoWork += new DoWorkEventHandler(ImportTimetable.TimetableFromCsv);
                worker.ProgressChanged += new ProgressChangedEventHandler(ImportTimetable_Progress);
                worker.RunWorkerAsync(filename);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportTimetable_Finished);

                this.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void ImportTimetable_Progress(object sender, ProgressChangedEventArgs e)
        {
            string[] state = e.UserState as string[];
            StatusbarText.Text = "Current Date: " + state[0] + "      Lines Processed: " + state[1];
        }

        private void ImportTimetable_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusbarText.Text = null;
            MainViewContentControl.Content = new BaseRailMLView();
        }
        

        #endregion

        #endregion

        #region Neural Network
        public void ImportDelays_Click(object sender, EventArgs e)
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
                worker.DoWork += new DoWorkEventHandler(NeuralNetwork.PreProcessing.Import.ImportDelayCombinations);
                worker.ProgressChanged += new ProgressChangedEventHandler(ImportDelays_Progress);
                worker.RunWorkerAsync(filename);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportDelays_Finished);

                //NeuralNetwork.PreProcessing.Import.ImportDelayCombinations(filename);

                this.IsHitTestVisible = false;
                Mouse.OverrideCursor = Cursors.AppStarting;
            }
        }

        private void ImportDelays_Progress(object sender, ProgressChangedEventArgs e)
        {
            string[] state = e.UserState as string[];
            StatusbarText.Text = "Current Date: " + state[0] + "      Lines Processed: " + state[1];
        }

        private void ImportDelays_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsHitTestVisible = true;
            Mouse.OverrideCursor = null;
            StatusbarText.Text = null;
            MainViewContentControl.Content = new NeuralNetwork.Views.BaseNeuralNetworkView();
        }

        public void SetTimetableFile_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text Documents (.txt)|*.txt"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                DataContainer.NeuralNetwork.timetablefile = dlg.FileName;
            }
        }

        public void SetReportsFile_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text Documents (.txt)|*.txt"; // Filter files by extension

            // Show load file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process load file dialog box results
            if (result == true)
            {
                // Load document
                DataContainer.NeuralNetwork.reportsfile = dlg.FileName;
            }
        }
        #endregion
    }
}
