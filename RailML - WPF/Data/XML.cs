using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;



namespace RailML___WPF.Data
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
                    catch(Exception ex)
                    {
                        Exception e = ex.InnerException;
                        int q = 1;
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

        public static void WriteToFile( string filename)
        {
            XDocument doc = new XDocument(ToXElement<railml>(Data.DataContainer.model));
            doc.Save(filename);
        }
    }
}
