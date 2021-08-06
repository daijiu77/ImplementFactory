using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Text;
using System.Xml;

namespace System.DJ.ImplementFactory.NetCore.Commons.Attrs
{
    public class MicroServiceRoute: Attribute
    {
        private string _routeName = "";
        private string _uri = "";
        private string configFile = "MicroServiceRoute.xml";

        private static Dictionary<string, string> dic = new Dictionary<string, string>();

        public MicroServiceRoute(string RouteName)
        {
            _routeName = RouteName;

            if (0 < dic.Count) return;

            string fPath = Path.Combine(DJTools.RootPath, configFile);
            if (!File.Exists(fPath))
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);

                XmlElement XMLroot = doc.CreateElement("MicroServiceRoute");
                doc.AppendChild(XMLroot);

                XmlElement route = doc.CreateElement("Route");
                XmlAttribute attr = doc.CreateAttribute("name", "route1");
                route.Attributes.Append(attr);

                XmlElement ele = doc.CreateElement("IpAddress");
                ele.InnerText = "127.0.0.1";
                route.AppendChild(ele);

                ele = doc.CreateElement("PortNumber");
                ele.InnerText = "80";
                route.AppendChild(ele);

                XMLroot.AppendChild(route);

                try
                {
                    doc.Save(fPath);
                }
                catch (Exception)
                {

                    //throw;
                }
            }

            if (File.Exists(fPath))
            {
                XmlDocument document = new XmlDocument();
                document.Load(fPath);

                if (2 > document.ChildNodes.Count) return;

                string uri = "";
                string attrName = "";
                XmlNode node = document.ChildNodes[1];
                foreach (XmlNode routeItem in node.ChildNodes)
                {
                    attrName = "";
                    uri = "";
                    foreach (XmlAttribute item in routeItem.Attributes)
                    {
                        if (item.Name.ToLower().Equals("name"))
                        {
                            attrName = item.Value;
                        }
                        else if (item.Name.ToLower().Equals("uri"))
                        {
                            uri = item.Value;
                        }
                    }

                    if (false == string.IsNullOrEmpty(attrName) && false == string.IsNullOrEmpty(uri))
                    {
                        dic.Remove(attrName.ToLower());
                        dic.Add(attrName.ToLower(), uri);
                    }
                }
            }

            string s = "";
            dic.TryGetValue(_routeName.ToLower(), out s);
            _uri = s;
        }

        public string RouteName { get { return _routeName; } }

        public string Uri { get { return _uri; } }
    }
}
