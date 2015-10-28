using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using ProtoBuf;
using System.Windows;

namespace RailML___WPF.Data
{
    // TODO: Implement Protobuf method, It's seemingly faster than binary serialization. 
    static class SaveLoad
    {
        static BackgroundWorker worker;
        public static void SaveToFile(object sender, DoWorkEventArgs e )
        {
            worker = sender as BackgroundWorker;
            string filename = e.Argument as string;
            SaveLoadData data = new SaveLoadData();
            data.railml = XML.ToXElement<railml>(Data.DataContainer.model).ToString();
            data.NN = DataContainer.NeuralNetwork;
            MyStream stream = new MyStream(filename, FileMode.Create, FileAccess.Write);
            stream.ProgressChanged += new ProgressChangedEventHandler(Save_ProgressChanged);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static void ProtoSaveToFile(object sender, DoWorkEventArgs e)
        {
            try
            {
                worker = sender as BackgroundWorker;
                string filename = e.Argument as string;
                SaveLoadData data = new SaveLoadData();
                data.railml = XML.ToXElement<railml>(Data.DataContainer.model).ToString();
                data.NN = DataContainer.NeuralNetwork;
                MyStream stream = new MyStream(filename, FileMode.Create, FileAccess.Write);
                stream.ProgressChanged += new ProgressChangedEventHandler(Save_ProgressChanged);
                Serializer.Serialize(stream, data);
                stream.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex + "      Inner Exception: " + ex.InnerException);
                worker.ReportProgress(1, ex + "      Inner Exception: " + ex.InnerException);
            }

        }

        private static void Save_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            worker.ReportProgress(0, (long)e.UserState);
        }

        public static void LoadFile(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            string filename = e.Argument as string;
            MyStream stream = new MyStream(filename, FileMode.Open, FileAccess.Read);
            stream.ProgressChanged += new ProgressChangedEventHandler(Load_ProgressChanged);
            IFormatter formatter = new BinaryFormatter();
            SaveLoadData data = (SaveLoadData)formatter.Deserialize(stream);
            XElement elem = XElement.Parse(data.railml);
            DataContainer.model = XML.FromXElement<railml>(elem);
            DataContainer.NeuralNetwork = data.NN;
            stream.Close();
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
            DataContainer.NeuralNetwork = data.NN;
            stream.Close();
        }
        

        private static void Load_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MyStream stream = sender as MyStream;
            long percentage = ((long)e.UserState)*100 / stream.Length;
            worker.ReportProgress(0,percentage);
        }

        
    }

    [Serializable]
    [ProtoContract]
    class SaveLoadData
    {
        [ProtoMember(1)]
        public string railml { get; set; }
        [ProtoMember(2)]
        public NeuralNetwork NN { get; set; }
    }

    class MyStream : FileStream
    {
        public long bytesWritten = 0;
        public long bytesRead = 0;
        public long MBcounter = 0;
        public MyStream(string filename, FileMode mode, FileAccess access) : base(filename, mode, access) 
        {
            
        }
        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            this.bytesWritten += count;
            if(this.bytesWritten > 1000000)
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
            if(this.bytesWritten > 1000000)
            {
                bytesWritten = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter));
            }
        }
        public override int Read(byte[] array, int offset, int count)
        {
            this.bytesRead += count;
            if(this.bytesRead > 1000000)
            {
                bytesRead = 0;
                MBcounter++;
                ProgressChanged(this, new ProgressChangedEventArgs(0, MBcounter*1000000));
            }
            return base.Read(array, offset, count);
        }
        public override int ReadByte()
        {
            this.bytesRead++;
            if(this.bytesRead > 1000000)
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
