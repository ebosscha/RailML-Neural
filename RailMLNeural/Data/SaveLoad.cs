using Encog.Neural.Networks;
using Encog.Neural.NeuralData;
using ProtoBuf;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Xml.Linq;

namespace RailMLNeural.Data
{
    static class SaveLoad
    {
        static BackgroundWorker worker;
        
        public static void ProtoSaveToFile(object sender, DoWorkEventArgs e)
        {
            try
            {
                worker = sender as BackgroundWorker;
                string filename = e.Argument as string;
                SaveLoadData data = new SaveLoadData();
                data.railml = XML.ToXElement<railml>(Data.DataContainer.model).ToString();
                data.NN = SerializeNetwork();
                data.settings = DataContainer.Settings;
                data.DelayCombinations = DataContainer.DelayCombinations;
                data.HeaderRoutes = DataContainer.HeaderRoutes;
                MyStream stream = new MyStream(filename, FileMode.Create, FileAccess.Write);
                stream.ProgressChanged += new ProgressChangedEventHandler(Save_ProgressChanged);
                Serializer.Serialize(stream, data);
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + "      Inner Exception: " + ex.InnerException);
                worker.ReportProgress(1, ex + "      Inner Exception: " + ex.InnerException);
            }

        }

        private static void Save_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            worker.ReportProgress(0, (long)e.UserState);
        }

        public static void ProtoLoadFile(object sender, DoWorkEventArgs e)
        {

            worker = sender as BackgroundWorker;
            string filename = e.Argument as string;
            MyStream stream = new MyStream(filename, FileMode.Open, FileAccess.Read);
            stream.ProgressChanged += new ProgressChangedEventHandler(Load_ProgressChanged);
            SaveLoadData data = Serializer.Deserialize<SaveLoadData>(stream);
            XElement elem = XElement.Parse(data.railml);
            DataContainer.model = XML.FromXElement<railml>(elem);
            DataContainer.NeuralNetworks = DeserializeNetwork(data.NN);
            DataContainer.Settings = data.settings;
            DataContainer.HeaderRoutes = data.HeaderRoutes;
            DataContainer.DelayCombinations = data.DelayCombinations;
            
            stream.Close();
        }


        private static void Load_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MyStream stream = sender as MyStream;
            long percentage = ((long)e.UserState) * 100 / stream.Length;
            worker.ReportProgress(0, percentage);
        }

        private static string SerializeNetwork()
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, DataContainer.NeuralNetworks);
            byte[] b = stream.ToArray();
            return System.Text.Encoding.UTF8.GetString(b);
        }

        private static List<NeuralNetwork> DeserializeNetwork(string input)
        {
            IFormatter formatter = new BinaryFormatter();
            byte[] b = System.Text.Encoding.UTF8.GetBytes(input);
            MemoryStream stream = new MemoryStream(b);
            return formatter.Deserialize(stream) as List<NeuralNetwork>;
        }


    }

    [Serializable]
    [ProtoContract]
    class SaveLoadData
    {
        [ProtoMember(1)]
        public string railml { get; set; }
        [ProtoMember(2)]
        public string NN { get; set; }
        [ProtoMember(3)]
        public string network { get; set; }
        [ProtoMember(4)]
        public string trainingset { get; set; }
        [ProtoMember(5)]
        public Settings settings { get; set; }
        [ProtoMember(6)]
        public DelayCombinationCollection DelayCombinations { get; set; }
        [ProtoMember(7)]
        public Dictionary<string, Dictionary<DateTime, string>> HeaderRoutes { get; set; }

    }

    class MyStream : FileStream
    {
        public long bytesWritten = 0;
        public long bytesRead = 0;
        public long MBcounter = 0;
        public MyStream(string filename, FileMode mode, FileAccess access)
            : base(filename, mode, access)
        {

        }
        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            this.bytesWritten += count;
            if (this.bytesWritten > 1000000)
            {
                bytesWritten = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter));
            }
        }
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            this.bytesWritten++;
            if (this.bytesWritten > 1000000)
            {
                bytesWritten = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter));
            }
        }
        public override int Read(byte[] array, int offset, int count)
        {
            this.bytesRead += count;
            if (this.bytesRead > 1000000)
            {
                bytesRead = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter * 1000000));
            }
            return base.Read(array, offset, count);
        }
        public override int ReadByte()
        {
            this.bytesRead++;
            if (this.bytesRead > 1000000)
            {
                bytesRead = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter * 1000000));
            }
            return base.ReadByte();
        }

        public event ProgressChangedEventHandler ProgressChanged;

    }
}
