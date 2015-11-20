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
                    _model.infrastructure.trackGroups.line.Add(line);
                    functionalLocation loc = new functionalLocation();
                    loc.sectorID = line.id;
                    loc.name = line.description;

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
                if (row.RowState != DataRowState.Deleted && !(bool)row["removed"])// && (string)row["ElementUsage"] != "C" && (string)row["ElementUsage"] != "F" && (string)row["ElementUsage"] != "A")
                {
                    eTrack track = new eTrack()
                    {
                        id = (row["Serial Number"] as string) ?? null,
                        description = (row["Description"] as string) ?? null
                    };
                    track.trackTopology.trackBegin.pos = (decimal)(((row["StartMiles"] as double?) ?? 0) + ((row["StartYards"] as double?) ?? 0) / 1760);
                    track.trackTopology.trackEnd.pos = (decimal)(((row["EndMiles"] as double?) ?? 0) + ((row["EndYards"] as double?) ?? 0) / 1760);
                    if ((string)row["Type"] == "UR") { track.mainDir = tExtendedDirection.up; }
                    else if ((string)row["Type"] == "DR") { track.mainDir = tExtendedDirection.down; }
                    else { track.mainDir = tExtendedDirection.none; }

                    if (row["Road"] is DBNull) { track.type = "connectingTrack"; }
                    else if ((string)row["Road"] == "MAIN") { track.type = "mainTrack"; }
                    else if ((string)row["Road"] == "BAY") { track.type = "stationTrack"; }
                    else { track.type = "sidingTrack"; }
                    if ((string)row["ElementUsage"] != "C" && (string)row["ElementUsage"] != "A")
                    {
                        track.trackDescr = new List<string>();
                        track.trackDescr.Add("Closed/Abandoned");
                    }


                    if (row["ParentTrackElement"] != System.DBNull.Value)
                    {
                        eLine line = _model.infrastructure.trackGroups.line.Find(x => x.id == (string)row["Sector"]);
                        line.trackRef.Add(new tTrackRefInGroup() { @ref = track.id });
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
                    tempswitch.pos = (decimal)(((row["Miles"] as double?) ?? 0) + ((row["Yards"] as double?) ?? 0) / 1760);
                    tempswitch.description = row["Description"] as string;
                    tempswitch.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch.geoCoord.coord.Add((double)row["INGNorthing"]);
                    tempswitch = FindConnections(tempswitch);
                }

                else if ((string)row["Type"] == "Fixed Diamond") { FixedDiamond(row); }
                else if ((string)row["Type"] == "Switch Diamond") { SwitchDiamond(row); }
                else if ((string)row["Type"] == "Single Slip") { SingleSlip(row); }
                else if ((string)row["Type"] == "Double Slips") { SingleSlip(row); }

            }
        }

        /// <summary>
        /// Method to find connections for an existing switch. Does so based on geocoord of tracknodes.
        /// </summary>
        /// <param name="tempswitch"></param>
        /// <returns></returns>

        private static eSwitch FindConnections(eSwitch tempswitch)
        {
            Point switchlocation = new Point(tempswitch.geoCoord.coord[0], tempswitch.geoCoord.coord[1]);
            eTrack hostline = null;
            eTrack track2 = null;
            bool trackend = false;

            while (track2 == null)
            {
                eTrack besttrack = null;
                double bestdist = 999999999999;

                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    double dist = GetDistance(switchlocation, new Point(track.trackTopology.trackBegin.geoCoord.coord[0], track.trackTopology.trackBegin.geoCoord.coord[1]));
                    if (dist < bestdist) { bestdist = dist; besttrack = track; trackend = false; }
                    dist = GetDistance(switchlocation, new Point(track.trackTopology.trackEnd.geoCoord.coord[0], track.trackTopology.trackEnd.geoCoord.coord[1]));
                    if (dist < bestdist) { bestdist = dist; besttrack = track; trackend = true; }
                }
                track2 = besttrack;
            }
            while (hostline == null)
            {
                eTrack besttrack = null;
                double bestdist = 99999999999;

                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    if (track == track2) { continue; }
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
            if (trackend == true) { track2.trackTopology.trackEnd.Item = conn1; }
            else { track2.trackTopology.trackBegin.Item = conn1; }
            tempswitch.connection.Add(conn2);
            hostline.trackTopology.connections.Add(tempswitch);
            string[] orientation = GetOrientation(hostline, track2, tempswitch);
            tempswitch.connection[0].orientation = orientation[0];
            tempswitch.connection[0].course = orientation[1];

            return tempswitch;
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
            return Math.Sqrt((a - p).LengthSquared);
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
                eCrossing tempcrossing = crossing;
                tempcrossing.id = tempcrossing.id + "-" + (count+1).ToString();
                tempcrossing.pos = (decimal)hosttuple.Item2[count];
                track.trackTopology.connections.Add(tempcrossing);    
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
                crossing.description = row["Description"] as string;
                crossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
                crossing.pos = GetPos(crossing.geoCoord.coord, DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine1"]));

                //Append crossing to second track
                crossing = new eCrossing();
                crossing.id = row["SerialNo"] as string + "-2";
                crossing.type = row["Type"] as string;
                crossing.description = row["Description"] as string;
                crossing.geoCoord.coord.Add((double)row["INGEasting"]); crossing.geoCoord.coord.Add((double)row["INGNorthing"]);
                crossing.pos = GetPos(crossing.geoCoord.coord, DataContainer.model.infrastructure.tracks.Single(x => x.id == (string)row["HostLine2"]));


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
            tempswitch1.description = row["Description"] as string;
            tempswitch1.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch1.geoCoord.coord.Add((double)row["INGNorthing"]);
            tempswitch1.pos = GetPos(tempswitch1.geoCoord.coord, track2);

            eSwitch tempswitch2 = new eSwitch();
            tempswitch2.id = row["SerialNo"] as string + "-2";
            tempswitch2.type = row["Type"] as string;
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
                tempswitch3.description = row["Description"] as string;
                tempswitch3.geoCoord.coord.Add((double)row["INGEasting"]); tempswitch3.geoCoord.coord.Add((double)row["INGNorthing"]);
                tempswitch3.pos = GetPos(tempswitch3.geoCoord.coord, track1);

                eSwitch tempswitch4 = new eSwitch();
                tempswitch4.id = row["SerialNo"] as string + "-4";
                tempswitch4.type = row["Type"] as string;
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

                eTrackNode besttrack = null;
                double bestdist = 99999999999999999;
                double x = (row["INGEasting"] as double?) ?? 0;
                double y = (row["INGNorthing"] as double?) ?? 0;
                foreach (eTrack track in DataContainer.model.infrastructure.tracks)
                {
                    double x2 = track.trackTopology.trackBegin.geoCoord.coord[0];
                    double y2 = track.trackTopology.trackBegin.geoCoord.coord[1];
                    if (Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2)) < bestdist)
                    {
                        bestdist = Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));
                        besttrack = track.trackTopology.trackBegin;
                    }
                    x2 = track.trackTopology.trackEnd.geoCoord.coord[0];
                    y2 = track.trackTopology.trackEnd.geoCoord.coord[1];
                    if (Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2)) < bestdist)
                    {
                        bestdist = Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));
                        besttrack = track.trackTopology.trackEnd;
                    }
                }
                besttrack.Item = buffer;
            }

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
                foreach (eTrack track in FindHostLines(crossing.geoCoord.coord[0], crossing.geoCoord.coord[1], (row["NumberOfTracks"] as int?) ?? 0).Item1)
                {
                    track.trackElements.levelCrossings.Add(crossing);
                }
            }
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
            StreamReader coordstream = new StreamReader("C:/Users/Edwin/OneDrive/Afstuderen/Irish Grid Conversion/Irish Grid WTK.txt");
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
                if (DataContainer.model.infrastructure.operationControlPoints.Any(x => x.name == ocp.name))
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

                        tCrossSection crosssection = new tCrossSection();
                        crosssection.ocpRef = ocp.id;
                        crosssection.name = ocp.name;
                        Data.DataContainer.IDGenerator(crosssection);
                        crosssection.pos = track.trackTopology.trackBegin.pos + (decimal)(pos * 0.000621371);
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

            return track.trackTopology.trackBegin.pos + (decimal)(pos * 0.000621371);
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
        private static void ConnectTracks()
        {
            foreach (eTrack track in _model.infrastructure.tracks)
            {
                if(track.trackTopology.trackBegin.Item == null)
                {
                    Point tracknodelocation = new Point(track.trackTopology.trackBegin.geoCoord.coord[0], track.trackTopology.trackBegin.geoCoord.coord[1]);
                    double lowestdist = double.PositiveInfinity;
                    eTrackNode bestnode = null;
                    foreach(eTrack othertrack in _model.infrastructure.tracks.Where(x => x != track 
                        && ( x.trackTopology.trackBegin.Item == null || x.trackTopology.trackEnd == null)))
                    {
                        if(othertrack.trackTopology.trackBegin.Item == null)
                        {
                            Point p = new Point(othertrack.trackTopology.trackBegin.geoCoord.coord[0], othertrack.trackTopology.trackBegin.geoCoord.coord[1]);
                            double dist = GetDistance(tracknodelocation, p);
                            if (dist < lowestdist) 
                            { 
                                lowestdist = dist;
                                bestnode = othertrack.trackTopology.trackBegin;
                            }
                        }
                        if (othertrack.trackTopology.trackEnd.Item == null)
                        {
                            Point p = new Point(othertrack.trackTopology.trackEnd.geoCoord.coord[0], othertrack.trackTopology.trackEnd.geoCoord.coord[1]);
                            double dist = GetDistance(tracknodelocation, p);
                            if (dist < lowestdist)
                            {
                                lowestdist = dist;
                                bestnode = othertrack.trackTopology.trackEnd;
                            }
                        }
                    }

                    tConnectionData c1 = new tConnectionData();
                    tConnectionData c2 = new tConnectionData();
                    DataContainer.IDGenerator(c1);
                    DataContainer.IDGenerator(c2);
                    c1.@ref = c2.id;
                    c2.@ref = c1.id;
                    track.trackTopology.trackBegin.Item = c1;
                    bestnode.Item = c2;
                }
                if(track.trackTopology.trackEnd.Item == null)
                {
                    Point tracknodelocation = new Point(track.trackTopology.trackEnd.geoCoord.coord[0], track.trackTopology.trackEnd.geoCoord.coord[1]);
                    double lowestdist = double.PositiveInfinity;
                    eTrackNode bestnode = null;
                    foreach (eTrack othertrack in _model.infrastructure.tracks.Where(x => x != track
                        && (x.trackTopology.trackBegin.Item == null || x.trackTopology.trackEnd == null)))
                    {
                        if (othertrack.trackTopology.trackBegin.Item == null)
                        {
                            Point p = new Point(othertrack.trackTopology.trackBegin.geoCoord.coord[0], othertrack.trackTopology.trackBegin.geoCoord.coord[1]);
                            double dist = GetDistance(tracknodelocation, p);
                            if (dist < lowestdist)
                            {
                                lowestdist = dist;
                                bestnode = othertrack.trackTopology.trackBegin;
                            }
                        }
                        if (othertrack.trackTopology.trackEnd.Item == null)
                        {
                            Point p = new Point(othertrack.trackTopology.trackEnd.geoCoord.coord[0], othertrack.trackTopology.trackEnd.geoCoord.coord[1]);
                            double dist = GetDistance(tracknodelocation, p);
                            if (dist < lowestdist)
                            {
                                lowestdist = dist;
                                bestnode = othertrack.trackTopology.trackEnd;
                            }
                        }
                    }

                    tConnectionData c1 = new tConnectionData();
                    tConnectionData c2 = new tConnectionData();
                    DataContainer.IDGenerator(c1);
                    DataContainer.IDGenerator(c2);
                    c1.@ref = c2.id;
                    c2.@ref = c1.id;
                    track.trackTopology.trackEnd.Item = c1;
                    bestnode.Item = c2;
                }
            }
        }

    }


}
