using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RockRaiders.Util.Helpers
{
    public class XMLHelper
    {
        public static T Deserialise<T>(Stream stream)
        {
            var type = typeof(T);

            try
            {
                var xmlSerialiser = new XmlSerializer(type);
                return (T)xmlSerialiser.Deserialize(stream);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserialising stream for type : {type.Name}", ex);
            }
        }

        public static T Deserialise<T>(string xmlString)
        {
            var type = typeof(T);

            try
            {
                using (TextReader sr = new StringReader(xmlString))
                {
                    var xmlSerialiser = new XmlSerializer(type);
                    return (T)xmlSerialiser.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserialising stream for type : {type.Name}", ex);
            }
        }

        public static Stream SerializeToXMLStream<T>(T src)
            where T : class
        {
            Stream result = new MemoryStream();
            var type = typeof(T);

            try
            {
                var xmlSerialiser = new XmlSerializer(type);

                using (var writer = XmlWriter.Create(result))
                {
                    xmlSerialiser.Serialize(result, src);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error : Exception thrown in SampleStreamer.Util.Extensions.ObjectExtensions.SerializeToXMLStream<{type.Name}> ", ex);
            }

            return result;
        }

        public static XmlDocument SerializeToXMLDocument<T>(T src)
            where T : class
        {
            var xml = string.Empty;
            var type = typeof(T);

            try
            {
                var xmlSerialiser = new XmlSerializer(type);
                var stringWriter = new StringWriter();

                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlSerialiser.Serialize(writer, src);
                    xml = stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error : Exception thrown in SampleStreamer.Util.Extensions.ObjectExtensions.SerializeToXMLDocument<{type.Name}> ", ex);
            }

            return new XmlDocument
            {
                InnerXml = xml
            };
        }
    }
}
