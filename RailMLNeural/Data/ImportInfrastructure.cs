using Excel;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using RailMLNeural.RailML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace RailMLNeural.Data
{
    public static class ImportInfrastructure
    {
        private static DataSet _dataset;
        public static DataTable _maintable;
        public static railml _model;
        private static Hashtable _coordtable;
        public static bool VisualFeedback = false;

        public static void ImportFromExcel(string filepath)
        {
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }
            _model = new railml();
            _coordtable = CreateCoordTable();

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;

            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables[0];
            _maintable = GetParentLines(_maintable);
            GetTracks(_maintable);

            DataContainer.model = _model;

        }

        private static DataTable GetParentLines(DataTable table)
        {
            Hashtable parentlist = new Hashtable();
            DataView view = new DataView(table);
            DataTable distinctvalues = view.ToTable(true, "ParentTrackElement");
            while (distinctvalues.Rows.Count > 0)
            {
                DataRow row = distinctvalues.Rows[0];
                if (row["ParentTrackElement"] != System.DBNull.Value)
                {
                    DataRow datarow = table.Select("EquipmentID = " + (double)row["ParentTrackElement"])[0];
                    eLine line = new eLine();
                    line.id = (string)datarow["Sector"];
                    line.description = (string)datarow["Description"];
                    if (!_model.infrastructure.trackGroups.line.Any(x => x.id == line.id))
                    {
                        _model.infrastructure.trackGroups.line.Add(line);
                    }
                    functionalLocation loc = new functionalLocation();
                    loc.sectorID = line.id;
                    loc.name = line.description;
                    if(!_model.infrastructure.trackGroups.functionalLocations.Any(x => x.sectorID == line.id))
                    {
                        _model.infrastructure.trackGroups.functionalLocations.Add(loc);
                    }

                    datarow.Delete();
                }
                row.Delete();
            }
            return table;
        }

        private static void GetTracks(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState != DataRowState.Deleted && !(bool)row["removed"] && (string)row["ElementUsage"] != "C" && (string)row["ElementUsage"] != "A")
                {
                    eTrack track = new eTrack()
                    {
                        id = (row["Serial Number"] as string) ?? null,
                        description = (row["Description"] as string) ?? null
                    };
                    if(track.id == null)
                    {
                        DataContainer.IDGenerator(track);
                    }
                    track.trackTopology.trackBegin.pos = (decimal)(((row["StartMiles"] as double?) ?? 0) + ((row["StartYards"] as double?) ?? 0) / 1760);
                    track.trackTopology.trackEnd.pos = (decimal)(((row["EndMiles"] as double?) ?? 0) + ((row["EndYards"] as double?) ?? 0) / 1760);
                    if ((string)row["Type"] == "UR" || (string)row["Type"] == "US") { track.mainDir = tExtendedDirection.up; }
                    else if ((string)row["Type"] == "DR" ||(string)row["Type"] == "DS")  { track.mainDir = tExtendedDirection.down; }
                    else { track.mainDir = tExtendedDirection.none; }

                    if (row["Road"] is DBNull) { track.type = "connectingTrack"; }
                    else if ((string)row["Road"] == "MAIN") { track.type = "mainTrack"; }
                    else if ((string)row["Road"] == "BAY") { track.type = "stationTrack"; }
                    else { track.type = "sidingTrack"; }
                    track.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                    if ((string)row["ElementUsage"] == "C" || (string)row["ElementUsage"] == "A")
                    {
                        track.trackDescr = new List<string>();
                        track.trackDescr.Add("Closed/Abandoned");
                    }


                    if (row["ParentTrackElement"] != System.DBNull.Value)
                    {
                        eLine line = _model.infrastructure.trackGroups.line.Find(x => x.id == (string)row["Sector"]);
                        line.trackRef.Add(new tTrackRefInGroup() { @ref = track.id });
                    }

                    if (row["Sector"] != System.DBNull.Value)
                    {
                        tTrackRef tref = new tTrackRef();
                        tref.@ref = track.id;
                        if (track.mainDir == tExtendedDirection.up) { tref.dir = tStrictDirection.up; }
                        if (track.mainDir == tExtendedDirection.down) { tref.dir = tStrictDirection.down; }
                        if (_model.infrastructure.trackGroups.functionalLocations.Any(x => x.sectorID == row["Sector"]))
                        {
                            _model.infrastructure.trackGroups.functionalLocations.First(x => x.sectorID == row["Sector"]).trackrefs.Add(tref);
                        }
                        else
                        {
                            functionalLocation loc = new functionalLocation();
                            loc.sectorID = row["Sector"] as string;
                            loc.trackrefs.Add(tref);
                            _model.infrastructure.trackGroups.functionalLocations.Add(loc);
                        }
                    }

                    track = GetTrackCoords(track, row["EquipmentID"].ToString());

                    _model.infrastructure.tracks.Add(track);

                }
            }
        }

        private static eTrack GetTrackCoords(eTrack track, string id)
        {
            int c = 0;
            List<double[]> list = _coordtable[id] as List<double[]>;
            foreach (double[] coord in list)
            {
                if (c == 0) { track.trackTopology.trackBegin.geoCoord.coord.AddRange(coord); c++; continue; }
                if (c == list.Count - 1) { track.trackTopology.trackEnd.geoCoord.coord.AddRange(coord); c++; continue; }
                else
                {
                    tPlacedElement map = new tPlacedElement();
                    map.geoCoord.coord.AddRange(coord);
                    track.trackElements.geoMappings.Add(map);

                }
                c++;
            }

            return track;
        }

        private static Hashtable CreateCoordTable()
        {
            Hashtable table = new Hashtable();
            string[] lines = System.IO.File.ReadAllLines("C:/Users/Edwin/OneDrive/Afstuderen/IrishRail Data/TrackGeometry.txt");
            foreach (string line in lines)
            {
                List<double[]> coordlist = new List<double[]>();
                string[] splitline = line.Split(new char[] { '\t' }, 2);
                string equipmentid = splitline[0].Trim();
                string coordstring = splitline[1].Trim().Replace("LINESTRING (", String.Empty).Replace(")", String.Empty);
                foreach (string coordset in coordstring.Split(','))
                {
                    string[] coordsetstrings = coordset.Trim().Split(' ');
                    double[] point = new double[] {
                        double.Parse(coordsetstrings[0].Trim(),CultureInfo.InvariantCulture),
                        double.Parse(coordsetstrings[1].Trim(),CultureInfo.InvariantCulture)};
                    coordlist.Add(point);
                }

                table.Add(equipmentid, coordlist);

            }


            return table;
        }

        public static void AddSwitchesFromExcel(string filepath)
        {
            //MessageBoxResult yesnobox = MessageBox.Show("Use Visual Feedback?", null, MessageBoxButton.YesNo);
            //if (yesnobox == MessageBoxResult.Yes)
            //{
            //    VisualFeedback = true;
            //}
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;
            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables[0];

            foreach (DataRow row in _maintable.Rows)
            {
                if ((string)row["Type"] == "Turnout" || ((string)row["Type"]) == "Crossover")
                {
                    eSwitch tempswitch = new eSwitch();
                    tempswitch.id = row["SerialNo"] as string;
                    tempswitch.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                    if(tempswitch.id == string.Empty || tempswitch.id == null)
                    {
                        DataContainer.IDGenerator(tempswitch);
                    }
                    tempswitch.description = row["Description"] as string;
                    tempswitch.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch.geoCoord.coord.Add((double)row["INGNorthing"]);
                    eTrack hostline = FindConnections(ref tempswitch);
                    tempswitch.pos = GetPos(tempswitch.geoCoord.coord, hostline);
                }

                else if ((string)row["Type"] == "Fixed Diamond") { FixedDiamond(row); }
                else if ((string)row["Type"] == "Switch Diamond") { SwitchDiamond(row); }
                else if ((string)row["Type"] == "Single Slip") { SingleSlip(row); }
                else if ((string)row["Type"] == "Double Slips") { SingleSlip(row); }


            }
            VisualFeedback = false;
            DataContainer.model = DataContainer.model;
        }

        /// <summary>
        /// Method to find connections for an existing switch. Does so based on geocoord of tracknodes.
        /// </summary>
        /// <param name="tempswitch"></param>
        /// <returns></returns>

        private static eTrack FindConnections(ref eSwitch tempswitch)
        {
            Point switchlocation = new Point(tempswitch.geoCoord.coord[0], tempswitch.geoCoord.coord[1]);
            eTrack hostline = null;
            eTrack track2 = null;
            bool trackend = false;

            while (track2 == null)
            {
                eTrack besttrack = null;
                double bestdist = double.PositiveInfinity;

                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    if (track.trackTopology.trackBegin.Item == null)
                    {
                        double dist = GetDistance(switchlocation, new Point(track.trackTopology.trackBegin.geoCoord.coord[0], track.trackTopology.trackBegin.geoCoord.coord[1]));
                        if (dist < bestdist) { bestdist = dist; besttrack = track; trackend = false; }
                    }
                    if (track.trackTopology.trackEnd.Item == null)
                    {
                        double dist = GetDistance(switchlocation, new Point(track.trackTopology.trackEnd.geoCoord.coord[0], track.trackTopology.trackEnd.geoCoord.coord[1]));
                        if (dist < bestdist) { bestdist = dist; besttrack = track; trackend = true; }
                    }
                }
                if(bestdist > 100)
                {
                    int i = 0;
                }

                track2 = besttrack;
            }
            while (hostline == null)
            {
                eTrack besttrack = null;
                double bestdist = double.PositiveInfinity;

                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    if (track.id == track2.id) { continue; }
                    List<Point> pointlist = NodeList(track);
                    for (int i = 0; i < pointlist.Count - 1; i++)
                    {
                        double dist = GetDistance(pointlist[i], pointlist[i + 1], switchlocation);
                        if (dist < bestdist) { bestdist = dist; besttrack = track; }
                    }

                }
                hostline = besttrack;
            }

            tConnectionData conn1 = new tConnectionData();
            DataContainer.IDGenerator(conn1);
            tSwitchConnectionData conn2 = new tSwitchConnectionData();
            DataContainer.IDGenerator(conn2);
            conn1.@ref = conn2.id; conn2.@ref = conn1.id;
            if (trackend) { track2.trackTopology.trackEnd.Item = conn1; }
            else { track2.trackTopology.trackBegin.Item = conn1; }
            tempswitch.connection.Add(conn2);
            hostline.trackTopology.connections.Add(tempswitch);
            string[] orientation = GetOrientation(hostline, track2, tempswitch);
            tempswitch.connection[0].orientation = orientation[0];
            tempswitch.connection[0].course = orientation[1];

            return hostline;
        }

        /// <summary>
        /// Function to get the minimum distance between a point and a line between point start and point end. 
        /// Used to define the minimum distance to a given track
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static double GetDistance(Point start, Point end, Point p)
        {
            var length = (start - end).LengthSquared;
            if (length == 0.0)
            {
                return Math.Sqrt((start - p).LengthSquared);
            }

            var t = (p - start) * (end - start) / length;
            if (t < 0.0) { return Math.Sqrt((start - p).LengthSquared); }
            else if (t > 1.0) { return Math.Sqrt((end - p).LengthSquared); }

            var projection = start + t * (end - start);
            return Math.Sqrt((projection - p).LengthSquared);

        }

        /// <summary>
        /// Method to return the distance between two points. Used to define distance between tracknodes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static double GetDistance(Point a, Point p)
        {
            double X = (a.X - p.X)*(a.X - p.X);
            double Y = (a.Y - p.Y)*(a.Y - p.Y);
            return Math.Sqrt(X + Y);
        }

        /// <summary>
        /// Method to create a fixed diamond crossing and append it to two hostlines.
        /// Assumes Tracks are not broken by fixeddiamond unit. 
        /// Might need manual checking.
        /// </summary>
        /// <param name="row"></param>
        private static void FixedDiamond(DataRow row)
        {
            eCrossing crossing = new eCrossing();
            crossing.id = row["SerialNo"] as string;
            crossing.pos = (decimal)(((row["Miles"] as double?) ?? 0) + ((row["Yards"] as double?) ?? 0) / 1760);
            crossing.type = row["Type"] as string;
            crossing.description = row["Description"] as string;
            crossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
            var hosttuple = FindHostLines(crossing.geoCoord.coord[0], crossing.geoCoord.coord[1], 2);
            int count = 0;
            foreach (eTrack track in hosttuple.Item1)
            {
                eCrossing tempcrossing = new eCrossing();
                tempcrossing.id = crossing.id + "-" + (count + 1).ToString();
                tempcrossing.pos = (decimal)hosttuple.Item2[count];
                tempcrossing.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                tempcrossing.type = row["Type"] as string;
                tempcrossing.description = row["Description"] as string;
                tempcrossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
                track.trackTopology.connections.Add(tempcrossing);
                count++;
            }
        }

        private static void SwitchDiamond(DataRow row)
        {
            if(row["HostLine1"] != DBNull.Value && row["HostLine2"] != DBNull.Value)
            {
                // Append crossing to first track
                eCrossing crossing = new eCrossing();
                crossing.id = row["SerialNo"] as string + "-1";
                crossing.type = row["Type"] as string;
                crossing.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                crossing.description = row["Description"] as string;
                crossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
                crossing.pos = GetPos(crossing.geoCoord.coord, DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine1"]));
                eTrack track1 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine1"]);
                track1.trackTopology.connections.Add(crossing);


                //Append crossing to second track
                crossing = new eCrossing();
                crossing.id = row["SerialNo"] as string + "-2";
                crossing.type = row["Type"] as string;
                crossing.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                crossing.description = row["Description"] as string;
                crossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
                crossing.pos = GetPos(crossing.geoCoord.coord, DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine2"]));
                eTrack track2 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine2"]);
                track2.trackTopology.connections.Add(crossing);


            }

        }

        private static void SingleSlip(DataRow row)
        {
            eTrack track1 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine1"]);
            eTrack track2 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine2"]);
            eTrack track3 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine3"]);
            eTrack track4 = DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine4"]);
            if(track1.id != track3.id)
            {
                ConnectTrackNodes(track1, track3);
            }
            if(track2.id != track4.id)
            {
                ConnectTrackNodes(track2, track4);
            }
            eSwitch tempswitch1 = new eSwitch();
            tempswitch1.id = row["SerialNo"] as string + "-1";
            tempswitch1.type = row["Type"] as string;
            tempswitch1.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
            tempswitch1.description = row["Description"] as string;
            tempswitch1.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch1.geoCoord.coord.Add((double)row["INGNorthing"]);
            tempswitch1.pos = GetPos(tempswitch1.geoCoord.coord, track2);

            eSwitch tempswitch2 = new eSwitch();
            tempswitch2.id = row["SerialNo"] as string + "-2";
            tempswitch2.type = row["Type"] as string;
            tempswitch2.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
            tempswitch2.description = row["Description"] as string;
            tempswitch2.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch2.geoCoord.coord.Add((double)row["INGNorthing"]);
            tempswitch2.pos = GetPos(tempswitch2.geoCoord.coord, track3);

            tSwitchConnectionData ts1conn = new tSwitchConnectionData();
            DataContainer.IDGenerator(ts1conn);
            
            tSwitchConnectionData ts2conn = new tSwitchConnectionData();
            DataContainer.IDGenerator(ts2conn);

            ts1conn.@ref = ts2conn.id;
            ts2conn.@ref = ts1conn.id;
            tempswitch1.connection.Add(ts1conn);
            tempswitch2.connection.Add(ts2conn);
            track2.trackTopology.connections.Add(tempswitch1);
            track3.trackTopology.connections.Add(tempswitch2);

            if((string)row["Type"] == "Double Slips")
            {
                eSwitch tempswitch3 = new eSwitch();
                tempswitch3.id = row["SerialNo"] as string + "-3";
                tempswitch3.type = row["Type"] as string;
                tempswitch3.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                tempswitch3.description = row["Description"] as string;
                tempswitch3.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch3.geoCoord.coord.Add((double)row["INGNorthing"]);
                tempswitch3.pos = GetPos(tempswitch3.geoCoord.coord, track1);

                eSwitch tempswitch4 = new eSwitch();
                tempswitch4.id = row["SerialNo"] as string + "-4";
                tempswitch4.type = row["Type"] as string;
                tempswitch4.code = ((double)row["EquipmentID"]).ToString() ?? string.Empty;
                tempswitch4.description = row["Description"] as string;
                tempswitch4.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch4.geoCoord.coord.Add((double)row["INGNorthing"]);
                tempswitch4.pos = GetPos(tempswitch4.geoCoord.coord, track4);

                tSwitchConnectionData ts3conn = new tSwitchConnectionData();
                DataContainer.IDGenerator(ts3conn);

                tSwitchConnectionData ts4conn = new tSwitchConnectionData();
                DataContainer.IDGenerator(ts4conn);

                ts3conn.@ref = ts4conn.id;
                ts4conn.@ref = ts3conn.id;
                tempswitch3.connection.Add(ts3conn);
                tempswitch4.connection.Add(ts4conn);
                track1.trackTopology.connections.Add(tempswitch3);
                track1.trackTopology.connections.Add(tempswitch4);
            }

        }

        private static void ConnectTrackNodes(eTrack track1, eTrack track2)
        {
            eTrackNode[] track1nodes = new eTrackNode[] { track1.trackTopology.trackBegin, track1.trackTopology.trackEnd };
            eTrackNode[] track2nodes = new eTrackNode[] { track2.trackTopology.trackBegin, track2.trackTopology.trackEnd };
            double bestdist = double.PositiveInfinity;
            eTrackNode besttrack1node = null;
            eTrackNode besttrack2node = null;
            foreach (eTrackNode track1node in track1nodes)
            {
                Point a = new Point(track1node.geoCoord.coord[0], track1node.geoCoord.coord[1]);
                foreach (eTrackNode track2node in track2nodes)
                {
                    Point b = new Point(track2node.geoCoord.coord[0], track2node.geoCoord.coord[1]);
                    if(GetDistance(a,b) < bestdist)
                    {
                        bestdist = GetDistance(a, b);
                        besttrack1node = track1node;
                        besttrack2node = track2node;
                    }
                }
            }
            tConnectionData c1 = new tConnectionData();
            tConnectionData c2 = new tConnectionData();
            DataContainer.IDGenerator(c1);
            DataContainer.IDGenerator(c2);
            c1.@ref = c2.id;
            c2.@ref = c1.id;
            besttrack1node.Item = c1;
            besttrack2node.Item = c2;
        }

        public static void AddBridgesFromExcel(string filepath)
        {

        }

        public static void AddTunnelsFromExcel(string filepath)
        {

        }

        public static void AddSignalsFromExcel(string filepath)
        {
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;

            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables["Signals"];
            foreach(DataRow row in _maintable.Rows)
            {
                tSignal signal = new tSignal();
                signal.id = row["SerialNo"] as string;
                signal.description = row["Description"] as string;
                double x = (row["INGEasting"] as double?) ?? 0;
                double y = (row["INGNorthing"] as double?) ?? 0;
                signal.geoCoord.coord.Add(x);
                signal.geoCoord.coord.Add(y);

                var tuple = FindHostLines(signal.geoCoord.coord[0], signal.geoCoord.coord[1], 1);
                signal.pos = (decimal)tuple.Item2[0];
                tuple.Item1[0].ocsElements.signals.Add(signal);
                string direction = (row["Direction"] as string) ?? String.Empty;
                if(direction == "UP")
                {
                    signal.dir = tLaxDirection.up;
                }
                else if(direction == "DOWN")
                {
                    signal.dir = tLaxDirection.down;
                }
                else
                {
                    signal.dir = tLaxDirection.unknown;
                }
            }
            
        }

        public static void AddSpeedRestrictionsFromExcel(string filepath)
        {
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;

            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables[0];
            decimal linespeedup = 0;
            decimal linespeeddown = 0;
            foreach(DataRow row in _maintable.Rows)
            {
                if(row["Between"] != DBNull.Value && (string)row["Between"] == "Line speed")
                {
                    if((string)row["Trains"] == "Down" || (string)row["Trains"] == "Both")
                    {
                        linespeeddown = (decimal)((row["Kmph"] as double?) ?? 0);
                    }
                    if ((string)row["Trains"] == "Up" || (string)row["Trains"] == "Both")
                    {
                        linespeedup = (decimal)((row["Kmph"] as double?) ?? 0);
                    }
                }
                decimal startpos = (decimal)((row["From"] as double?) ?? 0);
                decimal endpos = (decimal)((row["To"] as double?) ?? 0);
                decimal vMax = (decimal)((row["Kmph"] as double?) ?? 0);
                string loc = (row["Functional Location"]) as string;

                Nullable<tStrictDirection> dir = null;
                if((string)row["Trains"] == "Down")
                {
                    dir = tStrictDirection.down;
                }
                if ((string)row["Trains"] == "Up")
                {
                    dir = tStrictDirection.up;
                }
                List<eTrack> tracks = new List<eTrack>();
                foreach(var tref in DataContainer.model.infrastructure.trackGroups.functionalLocations.Single(x => x.sectorID == loc).trackrefs
                    .Where(x => row["EquipmentID"] == DBNull.Value || (string)row["EquipmentID"] == x.@ref ))
                {
                    eTrack track = DataContainer.GetItem(tref.@ref);
                    if((startpos > track.trackTopology.trackEnd.pos || endpos < track.trackTopology.trackBegin.pos))
                    {
                        continue;
                    }
                    decimal trackstartpos = Math.Max(startpos, track.trackTopology.trackBegin.pos);
                    decimal trackendpos = Math.Min(endpos, track.trackTopology.trackEnd.pos);
                    if(track.trackElements.speedChanges.Any(x => x.pos == trackstartpos && x.dir == dir))
                    {
                        track.trackElements.speedChanges.RemoveAt(track.trackElements.speedChanges.FindIndex(x => x.pos == trackstartpos && x.dir == dir));
                    }
                    tSpeedChange begin = new tSpeedChange();
                    DataContainer.IDGenerator(begin);
                    tSpeedChange end = new tSpeedChange();
                    DataContainer.IDGenerator(end);
                    begin.pos = trackstartpos;
                    end.pos = trackendpos;
                    if (dir.HasValue)
                    {
                        begin.dir = dir.Value;
                        end.dir = dir.Value;
                    }
                    begin.vMax = vMax;
                    end.vMax = linespeeddown; //TODO! : Better implementation of returning to proper linespeed
                    track.trackElements.speedChanges.Add(begin);
                    track.trackElements.speedChanges.Add(end);
                    //TODO: Implement train type distinction
                }
            }

            foreach(var track in DataContainer.model.infrastructure.tracks)
            {
                track.trackElements.speedChanges.OrderBy(x => x.pos);
            }
        }

        /// <summary>
        /// Adds bufferstops from Excel file. Appends bufferstop to the nearest tracknode based on location.
        /// </summary>
        /// <param name="filepath"></param>
        public static void AddBufferStopsFromExcel(string filepath)
        {
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;

            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables[0];

            foreach (DataRow row in _maintable.Rows)
            {
                tBufferStop buffer = new tBufferStop();
                buffer.id = row["SerialNo"] as string;
                buffer.description = row["Description"] as string;

                eTrackNode bestnode = null;
                double bestdist = double.PositiveInfinity;
                double x = (row["INGEasting"] as double?) ?? 0;
                double y = (row["INGNorthing"] as double?) ?? 0;
                Point location = new Point(x,y);
                foreach(eTrackNode node in DataContainer.model.infrastructure.tracks.SelectMany(e => e.GetTrackNodes())
                    .Where(e => e.Item == null))
                {
                    Point nodelocation = new Point(node.geoCoord.coord[0], node.geoCoord.coord[1]);
                    double dist = GetDistance(location, nodelocation);
                    if(dist < bestdist)
                    {
                        bestdist = dist;
                        bestnode = node;
                    }

                }

                if(bestdist < 10)
                {
                    bestnode.Item = buffer;
                }
            }
            DataContainer.model = DataContainer.model;

        }

        /// <summary>
        /// Adds level crossings from excel file. Currently relies on Findhostline method and needs improving to contain more hostlines.
        /// Most Level crossings exist on multiple tracks. Need to create multiple levelcrossings to avoid ID related issues.
        /// </summary>
        /// <param name="filepath"></param>
        public static void AddLevelCrossingsFromExcel(string filepath)
        {
            FileStream stream;
        Loop:
            try { stream = File.Open(filepath, FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;

            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables[0];

            foreach (DataRow row in _maintable.Rows)
            {
                tLevelCrossing crossing = new tLevelCrossing();
                crossing.id = row["SerialNo"] as string;
                crossing.description = row["Description"] as string;
                crossing.pos = (decimal)(((row["Miles"] as double?) ?? 0) + ((row["Yards"] as double?) ?? 0) / 1760);
                double[] coord = { (row["INGEasting"] as double?) ?? 0, (row["INGNorthing"] as double?) ?? 0 };
                crossing.geoCoord.coord.AddRange(coord);
                int count = 1;
                foreach (eTrack track in FindHostLines(crossing.geoCoord.coord[0], crossing.geoCoord.coord[1], (row["NumberOfTracks"] as int?) ?? 0).Item1)
                {
                    tLevelCrossing tempcrossing = crossing;
                    tempcrossing.id = crossing.id + "-" + count.ToString();
                    track.trackElements.levelCrossings.Add(tempcrossing);
                    count++;
                }
            }
            DataContainer.model = DataContainer.model;
        }

        /// <summary>
        /// Imports OCP's from XML recieved from the API. Should be avoided as API does not return all older stations.
        /// </summary>
        public static void OCPfromAPI()
        {
            DataContainer.model.infrastructure.operationControlPoints.Clear();
            CoordinateSystemFactory c = new CoordinateSystemFactory();
            StreamReader stream = new StreamReader("C:/Users/Edwin/OneDrive/Afstuderen/Irish Grid Conversion/Irish Grid WTK.txt");
            string wtk = stream.ReadLine();
            ICoordinateSystem target = c.CreateFromWkt(wtk);
            //ICoordinateSystem target = c.CreateFromWkt("PROJCS[\"TM65 / Irish Grid\", GEOGCS[\"TM65\", DATUM[\"TM65\", SPHEROID[\"Airy Modified 1849\",6377340.189,299.3249646, AUTHORITY[\"EPSG\",\"7002\"]],AUTHORITY[\"EPSG\",\"6299\"]], PRIMEM[\"Greenwich\",0,  AUTHORITY[\"EPSG\",\"8901\"]], UNIT[\"degree\",0.01745329251994328, AUTHORITY[\"EPSG\",\"9122\"]],  AUTHORITY[\"EPSG\",\"4299\"]], PROJECTION[\"Transverse_Mercator\"], PARAMETER[\"latitude_of_origin\",53.5], PARAMETER[\"central_meridian\",-8], PARAMETER[\"scale_factor\",1.000035],PARAMETER[\"false_easting\",200000], PARAMETER[\"false_northing\",250000], UNIT[\"metre\",1,  AUTHORITY[\"EPSG\",\"9001\"]], AUTHORITY[\"EPSG\",\"29902\"],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH]]");
            //ICoordinateSystem source = c.CreateFromWkt("GEOGCS[\"GCS_WGS_1984\", DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]], PRIMEM[\"Greenwich\",0], UNIT[\"Degree\",0.0174532925199433]]");
            ICoordinateSystem source = c.CreateFromWkt(stream.ReadLine());

            CoordinateTransformationFactory trf = new CoordinateTransformationFactory();
            ICoordinateTransformation tr = trf.CreateFromCoordinateSystems(source, target);
            //XDocument stationdoc = XDocument.Load("http://api.irishrail.ie/realtime/realtime.asmx/getAllStationsXML");
            XDocument stationdoc = XDocument.Load("C:/Users/Edwin/OneDrive/Afstuderen/IrishRail Data/getAllStationsXML.xml");
            XNamespace ns = stationdoc.Root.GetDefaultNamespace();
            foreach (XElement station in stationdoc.Root.Elements(ns + "objStation"))
            {
                eOcp tempocp = new eOcp();
                tempocp.id = station.Element(ns + "StationId").Value.Trim();
                tempocp.name = station.Element(ns + "StationDesc").Value.Trim();
                tempocp.code = station.Element(ns + "StationCode").Value.Trim();
                tempocp.propOperational.operationalType = "station";
                double[] coord = new double[] { double.Parse(station.Element(ns + "StationLongitude").Value, CultureInfo.InvariantCulture), double.Parse(station.Element(ns + "StationLatitude").Value, CultureInfo.InvariantCulture) };
                double[] irishgridcoord = tr.MathTransform.Transform(coord);
                tempocp.geoCoord.coord.Add(irishgridcoord[0]); tempocp.geoCoord.coord.Add(irishgridcoord[1]);

                AddOCPToTracks(tempocp);
                Data.DataContainer.model.infrastructure.operationControlPoints.Add(tempocp);
            }
            DataContainer.model = DataContainer.model;
        }


        /// <summary>
        /// Imports all OCP's from a set Excel file with all OCP references
        /// Relies on Coordinate transformation to transform to Irish National Grid.
        /// </summary>
        public static void OCPfromExcel()
        {
            FileStream stream;
        Loop:
            try { stream = File.Open("C:/Users/Edwin/OneDrive/Afstuderen/IrishRail Data/ReferenceData.xlsx", FileMode.Open, FileAccess.Read); }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Error. Please close the excel document. Retry?", "Error", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) { goto Loop; }
                else { return; }
            }
            CoordinateSystemFactory c = new CoordinateSystemFactory();
            StreamReader coordstream = new StreamReader("C:/Users/Edwin/OneDrive/Afstuderen/Irish Grid Conversion/Irish Grid WKT.txt");
            string wtk = coordstream.ReadLine();
            ICoordinateSystem target = c.CreateFromWkt(wtk);

            IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelreader.IsFirstRowAsColumnNames = true;
            ICoordinateSystem source = c.CreateFromWkt(coordstream.ReadLine());

            CoordinateTransformationFactory trf = new CoordinateTransformationFactory();
            ICoordinateTransformation tr = trf.CreateFromCoordinateSystems(source, target);


            _dataset = excelreader.AsDataSet();
            _maintable = _dataset.Tables["tbl_rtStations"];

            foreach (DataRow row in _maintable.Rows)
            {
                eOcp ocp = new eOcp();
                ocp.id = (row["StationID"] as string ?? string.Empty).Trim();
                ocp.name = (row["Station_Desc"] as string ?? string.Empty).Trim();
                ocp.code = (row["SEMA_Name"] as string ?? string.Empty).Trim();
                if (DataContainer.model.infrastructure.operationControlPoints.Any(x => x.id == ocp.id))
                {
                    continue;
                }
                if (row["Latitude"] != DBNull.Value && row["Longitude"] != DBNull.Value)
                {
                    double[] coord = new double[] { (row["Longitude"] as double?) ?? 0, (row["Latitude"] as double?) ?? 0 };
                    double[] irishgridcoord = tr.MathTransform.Transform(coord);
                    ocp.geoCoord.coord.Add(irishgridcoord[0]);
                    ocp.geoCoord.coord.Add(irishgridcoord[1]);
                    AddOCPToTracks(ocp);
                }
                if (((row["suburban"] as int?) ?? 0) != 0 && ((row["mainline_station"] as int?) ?? 0) != 0 && ((row["DART_STATION"] as int?) ?? 0) != 0)
                {
                    ocp.propOperational.operationalType = "station";
                }

                DataContainer.model.infrastructure.operationControlPoints.Add(ocp);
            }
            DataContainer.model = DataContainer.model;
        }

        /// <summary>
        /// Adds OCP to every track within the margin range. Margin currently set to 100. 
        /// Currently just added to maintracks and stationtracks.
        /// </summary>
        /// <param name="ocp"></param>
        private static void AddOCPToTracks(eOcp ocp)
        {
            double margin = 100;
            Point location = new Point(ocp.geoCoord.coord[0], ocp.geoCoord.coord[1]);
            foreach (eTrack track in DataContainer.model.infrastructure.tracks)
            {
                if (track.type == "mainTrack" || track.type == "stationTrack")
                {
                    double bestdist = 9999999999999;
                    List<Point> nodelist = NodeList(track);
                    Point a = new Point();
                    Point b = new Point();
                    int index = 0;
                    for (int i = 0; i < nodelist.Count - 1; i++)
                    {
                        double dist = GetDistance(nodelist[i], nodelist[i + 1], location);
                        if (dist < bestdist)
                        {
                            bestdist = dist;
                            a = nodelist[i]; b = nodelist[i + 1];
                            index = i;
                        }
                    }
                    if (bestdist < margin)
                    {
                        tCrossSection crosssection = new tCrossSection();
                        crosssection.ocpRef = ocp.id;
                        crosssection.name = ocp.name;
                        Data.DataContainer.IDGenerator(crosssection);
                        crosssection.pos = GetPos(new List<double>(){location.X, location.Y}, track);
                        track.trackTopology.crossSections.Add(crosssection);
                    }
                }
            }
        }

        /// <summary>
        /// Method to find the host track for an element such as a switch, bufferstop or something else.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static Tuple<List<eTrack>,List<double>> FindHostLines(double x, double y, int n)
        {
            List<eTrack> tracks = new List<eTrack>();
            List<double> positions = new List<double>();
            Point location = new Point(x, y);
            int c = 0;
            while (c < n)
            {
                eTrack besttrack = null;
                double bestdist = 99999999999;
                double pos = 0;
                int index = 0;

                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    foreach (eTrack alreadyinlist in tracks)
                    {
                        if (track == alreadyinlist) { continue; }
                    }
                    List<Point> nodelist = NodeList(track);
                    for(int i=0; i < nodelist.Count - 1; i++)
                    {
                        double dist = GetDistance(nodelist[i], nodelist[i + 1], location);
                        if(dist < bestdist)
                        {
                            besttrack = track;
                            bestdist = dist;
                            index = i;
                        }
                    }
                }

                List<Point> besttracknodelist = NodeList(besttrack);
                pos = 0;
                for(int i=0; i < index; i++)
                {
                    pos += GetDistance(besttracknodelist[i], besttracknodelist[i + 1]);
                }
                Point a = besttracknodelist[index];
                Point b = besttracknodelist[index + 1];
                    
                if ((a - b).LengthSquared != 0.0)
                {
                    var t = (location - a) * (b - a) / (a - b).LengthSquared;
                    pos += t * GetDistance(a, b);
                }

                tracks.Add(besttrack);
                positions.Add((double)besttrack.trackTopology.trackBegin.pos + (pos * 0.000621371));
                c++;
                    
            }
            return new Tuple<List<eTrack>, List<double>>(tracks, positions);
        }

        /// <summary>
        /// Method to get the position of a feature along a track.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        private static decimal GetPos(List<double> coord, eTrack track)
        {
            Point location = new Point(coord[0], coord[1]);
            List<Point> nodelist = NodeList(track);

            double bestdist = 9999999999999;
            Point a = new Point();
            Point b = new Point();
            int index = 0;
            for (int i = 0; i < nodelist.Count - 1; i++)
            {
                double dist = GetDistance(nodelist[i], nodelist[i + 1], location);
                if (dist < bestdist)
                {
                    bestdist = dist;
                    a = nodelist[i]; b = nodelist[i + 1];
                    index = i;
                }
            }
                   
            double pos = 0;
            for (int i = 0; i < index - 1; i++)
            {
                pos += GetDistance(nodelist[i], nodelist[i + 1]);
            }
            if ((a - b).LengthSquared != 0.0)
            {
                var t = (location - a) * (b - a) / (a - b).LengthSquared;
                pos += t * GetDistance(a, b);
            }

            pos = (double)track.trackTopology.trackBegin.pos + (pos * 0.000621371);
            if((decimal)pos >= track.trackTopology.trackEnd.pos)
            {
                pos = (double)track.trackTopology.trackEnd.pos - 0.001; 
            }

            if((decimal)pos <= track.trackTopology.trackBegin.pos)
            {
                pos = (double)track.trackTopology.trackBegin.pos + 0.001;
            }
            return (decimal)pos;
        }

        /// <summary>
        /// Generates a nodelist for a track. Used for defining closes neighbouring tracks.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        private static List<Point> NodeList(eTrack track)
        {
            List<Point> list = new List<Point>();
            list.Add(new Point(track.trackTopology.trackBegin.geoCoord.coord[0], track.trackTopology.trackBegin.geoCoord.coord[1]));
            foreach (tPlacedElement e in track.trackElements.geoMappings)
            {
                list.Add(new Point(e.geoCoord.coord[0], e.geoCoord.coord[1]));
            }
            list.Add(new Point(track.trackTopology.trackEnd.geoCoord.coord[0], track.trackTopology.trackEnd.geoCoord.coord[1]));


            return list;

        }

        /// <summary>
        /// returns orientation and coarse of a switch intersection. Used for defining the trackcontinuecourse.
        /// </summary>
        /// <param name="maintrack"></param>
        /// <param name="sidetrack"></param>
        /// <param name="sw"></param>
        /// <returns></returns>

        private static string[] GetOrientation(eTrack maintrack, eTrack sidetrack, eSwitch sw)
        {
            string[] result = new string[2];
            bool trackend = false;
            double angle;
            Point switchlocation = new Point(sw.geoCoord.coord[0], sw.geoCoord.coord[1]);
            if (DataContainer.GetItem(sw.connection[0].@ref).FindParent(typeof(eTrackBegin)) != null)
            {
                trackend = true;
            }
            double bestdist = 9999999999999999999;
            List<Point> maintracknodelist = NodeList(maintrack);
            Point[] maintrackpoints = new Point[2];
            for (int i = 0; i < maintracknodelist.Count - 1; i++)
            {
                double dist = GetDistance(maintracknodelist[i], maintracknodelist[i + 1], switchlocation);
                if (dist < bestdist)
                {
                    maintrackpoints[0] = maintracknodelist[i];
                    maintrackpoints[1] = maintracknodelist[i + 1];
                }
            }
            List<Point> sidetracknodelist = NodeList(sidetrack);
            Point[] sidetrackpoints = new Point[2];

            if (trackend)
            {
                sidetrackpoints[0] = sidetracknodelist[sidetracknodelist.Count];
                sidetrackpoints[1] = sidetracknodelist[sidetracknodelist.Count - 1];
            }
            else
            {
                sidetrackpoints[0] = sidetracknodelist[0];
                sidetrackpoints[1] = sidetracknodelist[1];
            }

            angle = Math.Atan2((maintrackpoints[1] - maintrackpoints[0]).X, (maintrackpoints[1] - maintrackpoints[0]).Y) - Math.Atan2((sidetrackpoints[1] - sidetrackpoints[0]).X, (sidetrackpoints[1] - sidetrackpoints[0]).Y);
            if (angle > Math.PI / 4 || angle < -Math.PI / 4) { result[0] = "incoming"; }
            else { result[0] = "outgoing"; }
            if (angle < 0) { result[1] = "right"; }
            else { result[1] = "left"; }

            return result;
        }

        /// <summary>
        /// Connects all unassigned trackend connections to another unassigned trackend collection
        /// Only do after adding bufferstops and all switch types. 
        /// Might need heavy data checking for correctness.
        /// 
        /// Works by iterating all tracks with an empty Item variable in the trackbegin or trackend tracknodes. 
        /// Traces minimum distance to other tracks, forms connection with those. 
        /// </summary>
        public static void ConnectTracks()
        {
            foreach (eTrack track in DataContainer.model.infrastructure.tracks)
            {
                foreach (eTrackNode node in track.GetTrackNodes().Where(x => x.Item == null))
                {
                    double bestdist = double.PositiveInfinity;
                    eTrackNode bestnode = null;
                    Point location = new Point(node.geoCoord.coord[0], node.geoCoord.coord[1]);
                    foreach (eTrackNode othernode in DataContainer.model.infrastructure.tracks.Where(x => x.id != track.id)
                        .SelectMany(x => x.GetTrackNodes())
                        .Where(x => x.Item == null))
                    {
                        Point otherlocation = new Point(othernode.geoCoord.coord[0], othernode.geoCoord.coord[1]);
                        double dist = GetDistance(location, otherlocation);
                        if (dist < bestdist)
                        {
                            bestdist = dist;
                            bestnode = othernode;
                        }
                    }

                    if (bestdist < 2)
                    {

                        tConnectionData c1 = new tConnectionData();
                        tConnectionData c2 = new tConnectionData();
                        DataContainer.IDGenerator(c1);
                        DataContainer.IDGenerator(c2);
                        c1.@ref = c2.id;
                        c2.@ref = c1.id;
                        node.Item = c1;
                        bestnode.Item = c2;
                    }
                }
            }

            DataContainer.model = DataContainer.model;
        }

    }

    public static class Extentions
    {
        public static List<eTrackNode> GetTrackNodes(this eTrack track)
        {
            List<eTrackNode> result = new List<eTrackNode>();
            result.Add(track.trackTopology.trackBegin);
            result.Add(track.trackTopology.trackEnd);
            return result;
        }
    }


}
