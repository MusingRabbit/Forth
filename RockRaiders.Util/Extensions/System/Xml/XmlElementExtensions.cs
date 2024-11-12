using RockRaiders.Util.Helpers;
using System.Xml;

namespace RockRaiders.Util.Extensions.System.Xml
{
    public static class XmlElementExtensions
    {
        public static T Deserialise<T>(this XmlElement element)
        {
            return XMLHelper.Deserialise<T>(element.InnerXml);
        }
    }
}
