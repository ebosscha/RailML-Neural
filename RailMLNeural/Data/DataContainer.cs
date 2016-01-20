using RailMLNeural.Neural;
using RailMLNeural.RailML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace RailMLNeural.Data
{
    [Serializable]
    static class DataContainer
    {
        private static railml _model;
        private static List<INeuralConfiguration> _neuralConfigurations;
        private static Hashtable _idlist = new Hashtable();
        private static PathContainer _pathcontainer;
        private static Settings _settings = new Settings();
        private static MetaData _metadata = new MetaData();
        public static DelayCombinationCollection DelayCombinations { get; set; }
        public static Dictionary<string, Dictionary<DateTime, string>> HeaderRoutes { get; set; }
        static public event EventHandler ModelChanged;

        public static railml model
        {
            get { return _model; }
            set { _model = value;
                PrepareData(); 
                OnModelChanged("model"); }
        }

        public static Settings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public static List<INeuralConfiguration> NeuralConfigurations
        {
            get
            {
                if (_neuralConfigurations == null) { _neuralConfigurations = new List<INeuralConfiguration>(); }
                return _neuralConfigurations;
            }
            set { _neuralConfigurations = value; }
        }

        public static MetaData MetaData
        {
            get { return _metadata; }
            set { _metadata = value; }
        }

        public static Hashtable IDList { get { return _idlist; } }

        public static PathContainer PathContainer
        {
            get
            {
                if (_pathcontainer == null)
                { _pathcontainer = new PathContainer(); }
                return _pathcontainer;
            }
            set { _pathcontainer = value; }
        }

        public static void OnModelChanged(string name)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                EventHandler handler = ModelChanged;
                if (handler != null)
                {
                    handler(model, new PropertyChangedEventArgs(name));
                }
            }));

        }

        public static void PrepareData()
        {
            _idlist.Clear();
            RecurrentPrepareData(_model);
        }

        //Sets ID list and Parent Structure
        private static void RecurrentPrepareData(dynamic item)
        {
            PropertyInfo[] properties = item.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "id" && item.id != null)
                {
                    if (_idlist.ContainsKey(item.id))
                    {
                        throw new Exception("Error: Duplicate ID");
                    }
                    else
                    {
                        _idlist.Add(item.id, item);
                    }
                }
                if (prop.PropertyType.Namespace == "RailMLNeural.RailML" && prop.GetValue(item) != null && !prop.PropertyType.IsEnum)
                {
                    dynamic property = prop.GetValue(item);
                    property.SetParent(item);
                    RecurrentPrepareData(property);
                }
                if (prop.PropertyType.Namespace == "System.Collections.Generic")
                {
                    foreach (dynamic listitem in prop.GetValue(item))
                    {
                        if (listitem.GetType().Namespace == "RailMLNeural.RailML")
                        {
                            listitem.SetParent(item);
                            RecurrentPrepareData(listitem);
                        }
                    }
                }
                if (prop.PropertyType == typeof(object) && prop.GetValue(item) != null)
                {
                    if(prop.GetValue(item).GetType() == typeof(tConnectionData))
                    {
                        var obj = (tConnectionData)prop.GetValue(item);
                        obj.SetParent(item);
                        RecurrentPrepareData(obj);
                    }
                    
                     if(prop.GetValue(item).GetType() == typeof(tBufferStop))
                    {
                        var obj = (tBufferStop)prop.GetValue(item);
                        obj.SetParent(item);
                        RecurrentPrepareData(obj);
                    }
                    if(prop.GetValue(item).GetType() == typeof(tOpenEnd))
                    {
                        var obj = (tOpenEnd)prop.GetValue(item);
                        obj.SetParent(item);
                        RecurrentPrepareData(obj);
                    }
                    if(prop.GetValue(item).GetType() == typeof(tMacroscopicNode))
                    {
                        var obj = (tMacroscopicNode)prop.GetValue(item);
                        obj.SetParent(item);
                        RecurrentPrepareData(obj);
                    }
                }
            }
        }

        public static Hashtable prefixes = new Hashtable() {
        {"track","TR"},
        {"connection","CONN"},
        {"switch","SW"}, 
        {"eTrack", "TR"},
        {"tConnectionData", "CONN"},
        {"tSwitchConnectionData", "CONN"},
        {"eSwitch", "SW"}};
        public static string IDGenerator(string type)
        {

        Loop:
            Random rand = new Random();
            string prefix;
            if (prefixes.ContainsKey(type.ToLower()))
            {
                prefix = (string)prefixes[type.ToLower()];
            }
            else { prefix = "ID"; }

            string id = prefix + rand.Next(999999).ToString();
            if (_idlist.ContainsKey(id))
            {
                goto Loop;
            }
            _idlist.Add(id, null);
            return id;
        }

        public static void IDGenerator(dynamic input)
        {
            Random rand = new Random();
            Type T = input.GetType();
            if (T.GetProperty("id") != null)
            {

                if (T == typeof(string)) { input.id = IDGenerator((string)input); }
            Loop:
                string id;
                if (prefixes.ContainsKey(T.Name))
                { id = prefixes[T.Name] + rand.Next(999999).ToString(); }
                else { id = T.Name + rand.Next(999999).ToString(); }
                if (_idlist.ContainsKey(id))
                { goto Loop; }
                _idlist.Add(id, input);
                input.id = id;
            }
        }

        public static dynamic GetItem(string id)
        {
            if (_idlist.ContainsKey(id))
            {
                return _idlist[id];
            }
            return null;
        }

        public static List<dynamic> FindAll(Type type)
        {
            return RecurrentFindAll(_model, type, null);
        }

        private static List<dynamic> RecurrentFindAll(dynamic elem, Type type, Func<Boolean> condition)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (PropertyInfo prop in elem.GetType().GetProperties())
            {
                if (prop.PropertyType == type && prop.GetValue(elem) != null)
                {
                    list.Add(prop.GetValue(elem));
                }
                if (prop.PropertyType.Namespace == "RailML___WPF" && prop.GetValue(elem) != null && !prop.PropertyType.IsEnum)
                {
                    RecurrentFindAll(prop.GetValue(elem), type, condition);
                }

            }

            return list;


        }
    }

    public class ParentContainer
    {
        private dynamic _parent;

        public dynamic GetParent()
        {
            return _parent;
        }

        public void SetParent(dynamic parent)
        {
            _parent = parent;
        }

        public dynamic FindParent(Type T)
        {
            if (_parent == null) { return null; }
            if (_parent.GetType() == T) { return _parent; }
            else { return _parent.FindParent(T); }
        }

    }
}
