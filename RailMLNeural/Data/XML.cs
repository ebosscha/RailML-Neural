using RailMLNeural.RailML;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RailMLNeural.Data
{
    public static class XML
    {
        static XmlSerializer serializer = new XmlSerializer(typeof(railml));


        public static XElement CleanXML(XElement doc)
        {
            while (true)
            {
                var empties = doc.Descendants().Where(e => e.IsEmpty && !HasAttributes(e)).ToList();
                if (empties.Count() == 0) { break; }
                empties.ForEach(e => e.Remove());
            }

            foreach (XElement elem in doc.Descendants())
            {
                foreach (XAttribute attr in elem.Attributes())
                {
                    if (attr.Value == null || attr.Value == "" || attr.Value == " ")
                    {
                        attr.Remove();
                    }
                }
            }
            return doc;
        }

        public static bool HasAttributes(XElement elem)
        {
            bool result = false;
            foreach (XAttribute attr in elem.Attributes())
            {
                if (attr.Value != null && attr.Value != "" && attr.Value != " ")
                {
                    result = true;
                    return result;
                }
            }
            return result;

        }

        public static railml ImportFile(string filename)
        {
            railml railmlmodel = new railml();

            using (FileStream xmlStream = new FileStream(filename, FileMode.Open))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                {

                    railmlmodel = (railml)serializer.Deserialize(xmlReader);

                }
            }
            return railmlmodel;
        }

        public static XElement ToXElement<T>(this object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    try
                    {
                        var xmlSerializer = new XmlSerializer(typeof(T));
                        xmlSerializer.Serialize(streamWriter, obj);
                        XElement elem = XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                        elem = CleanXML(elem);
                        return elem;
                    }
                    catch (Exception ex)
                    {
                        Exception e = ex.InnerException;
                        return null;
                    }
                }
            }
        }

        public static T FromXElement<T>(this XElement xElement)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(xElement.CreateReader());
        }

        public static void WriteToFile(string filename)
        {
            RoundDecimals();
            XDocument doc = new XDocument(ToXElement<railml>(Data.DataContainer.model));
            doc.Save(filename);
        }

        private static void RoundDecimals()
        {
            RecurrentPrepareData(DataContainer.model);
        }

        private static void RecurrentPrepareData(dynamic item)
        {
            PropertyInfo[] properties = item.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "refConnection")
                {
                    continue;
                }
                if (prop.PropertyType == typeof(decimal) && prop.GetValue(item) != null)
                {

                    prop.SetValue(item, decimal.Round(prop.GetValue(item), 6));
                }
                if (prop.PropertyType.Namespace == "RailMLNeural.RailML" && prop.GetValue(item) != null && !prop.PropertyType.IsEnum)
                {
                    dynamic property = prop.GetValue(item);
                    RecurrentPrepareData(property);
                }
                if (prop.PropertyType.Namespace == "System.Collections.Generic")
                {
                    foreach (dynamic listitem in prop.GetValue(item))
                    {
                        if (listitem.GetType().Namespace == "RailMLNeural.RailML")
                        {
                            RecurrentPrepareData(listitem);
                        }
                    }
                }
                if (prop.PropertyType == typeof(object) && prop.GetValue(item) != null)
                {
                    if (prop.GetValue(item).GetType() == typeof(tConnectionData))
                    {
                        var obj = (tConnectionData)prop.GetValue(item);
                        RecurrentPrepareData(obj);
                    }

                    if (prop.GetValue(item).GetType() == typeof(tBufferStop))
                    {
                        var obj = (tBufferStop)prop.GetValue(item);
                        RecurrentPrepareData(obj);
                    }
                    if (prop.GetValue(item).GetType() == typeof(tOpenEnd))
                    {
                        var obj = (tOpenEnd)prop.GetValue(item);
                        RecurrentPrepareData(obj);
                    }
                    if (prop.GetValue(item).GetType() == typeof(tMacroscopicNode))
                    {
                        var obj = (tMacroscopicNode)prop.GetValue(item);
                        RecurrentPrepareData(obj);
                    }
                }
            }
        }
    }
}
