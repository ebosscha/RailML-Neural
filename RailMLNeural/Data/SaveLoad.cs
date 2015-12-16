using Encog.Neural.Networks;
using Encog.Neural.NeuralData;
using ProtoBuf;
using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Xml.Linq;

namespace RailMLNeural.Data
{
    static class SaveLoad
    {
        public static BackgroundWorker worker;
        
        public static void ProtoSaveToFile(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            string filename = e.Argument as string;
            SaveLoadData data = new SaveLoadData();
            data.railml = XML.ToXElement<railml>(Data.DataContainer.model).ToString();
            data.NN = SerializeNetwork();
            data.settings = DataContainer.Settings;
            data.metadata = DataContainer.MetaData;
            data.metadata.LastEditTime = DateTime.Now;
            data.DelayCombinations = DataContainer.DelayCombinations;
            data.HeaderRoutes = DataContainer.HeaderRoutes;
            MyStream stream = new MyStream(filename, FileMode.Create, FileAccess.Write);
            stream.ProgressChanged += new ProgressChangedEventHandler(Save_ProgressChanged);
            Serializer.Serialize(stream, data);
            stream.Close();
            data.Dispose();
            data = null;
            GC.Collect();

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
            DataContainer.MetaData = data.metadata;
            
            stream.Close();
            data.Dispose();
            data = null;
            GC.Collect();
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
            string output = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, DataContainer.NeuralNetworks);
                output = Convert.ToBase64String(stream.ToArray());
            }
            return output;
            
        }

        private static List<NeuralNetwork> DeserializeNetwork(string input)
        {
            IFormatter formatter = new BinaryFormatter();
            //byte[] b = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] b = Convert.FromBase64String(input);
            List<NeuralNetwork> output = new List<NeuralNetwork>();
            using (MemoryStream stream = new MemoryStream(b, 0, b.Length))
            {
                stream.Write(b, 0, b.Length);
                stream.Position = 0;
                output = formatter.Deserialize(stream) as List<NeuralNetwork>;
                stream.Close();
            }
            return output;
        }


    }

    [Serializable]
    [ProtoContract]
    class SaveLoadData : IDisposable
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
        [ProtoMember(8)]
        public MetaData metadata { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                railml = null;
                NN = null;
                network = null;
                trainingset = null;
                settings = null;
                DelayCombinations = null;
                HeaderRoutes = null;
                metadata = null;
            }
        }

    }

    [Serializable]
    public class SaveLoadDataContainer
    {
        
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
