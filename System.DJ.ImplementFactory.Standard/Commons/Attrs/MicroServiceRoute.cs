using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class MicroServiceRoute: Attribute
    {
        private string _routeName = "";
        private string _controllerName = "";
        private string _uri = "";
        private string configFile = "MicroServiceRoute.xml";

        private static Dictionary<string, string> dic = new Dictionary<string, string>();

        public MicroServiceRoute(string RouteName, string ControllerName)
        {
            msr(RouteName, ControllerName);
        }

        public MicroServiceRoute(string RouteName)
        {
            msr(RouteName, null);
        }

        private void msr(string RouteName, string ControllerName)
        {
            _routeName = RouteName;
            _controllerName = ControllerName;

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
                route.SetAttribute("name", "route1");
                route.SetAttribute("uri", "http://127.0.0.1:8080");

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

        public string ControllerName { get { return _controllerName; } }

        public string Uri { get { return _uri; } }
    }
}
