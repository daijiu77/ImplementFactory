using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MicroServiceRoute : Attribute
    {
        private string _routeName = "";
        private string _controllerName = "";
        private string _uri = "";
        private const string configFile = "MicroServiceRoute.xml";

        private static Dictionary<string, RouteAttr> dic = new Dictionary<string, RouteAttr>();

        static MicroServiceRoute()
        {
            msr(null, null, null);
        }

        public MicroServiceRoute(string RouteName, string ControllerName)
        {
            msr(this, RouteName, ControllerName);
        }

        public MicroServiceRoute(string RouteName)
        {
            msr(this, RouteName, null);
        }

        private static void msr(MicroServiceRoute microServiceRoute, string RouteName, string ControllerName)
        {
            if (null != microServiceRoute)
            {
                microServiceRoute._routeName = RouteName;
                microServiceRoute._controllerName = ControllerName;
            }

            if (0 < dic.Count) return;

            string fPath = Path.Combine(DJTools.RootPath, configFile);
            if (!File.Exists(fPath))
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);

                XmlElement XMLroot = doc.CreateElement("MicroServiceRoutes");
                doc.AppendChild(XMLroot);

                XmlElement route = doc.CreateElement("Route");
                route.SetAttribute("Name", "ServiceRoute1");
                route.SetAttribute("Uri", "http://127.0.0.1:8080,http://127.0.0.1:8081");
                route.SetAttribute("RegisterAddr", "/Home/RegisterIP?ContractKey=abc123");
                route.SetAttribute("RegisterActionType", "get");

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
                string attrName1 = "";
                string registerAddr = "";
                string method = "";
                XmlNode node = document.ChildNodes[1];
                foreach (XmlNode routeItem in node.ChildNodes)
                {
                    attrName = "";
                    uri = "";
                    registerAddr = "";
                    method = "get";
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
                        else if (item.Name.ToLower().Equals("registeraddr"))
                        {
                            registerAddr = item.Value;
                        }
                        else if (item.Name.ToLower().Equals("method"))
                        {
                            method = item.Value;
                        }
                    }

                    if (false == string.IsNullOrEmpty(attrName) && false == string.IsNullOrEmpty(uri))
                    {
                        attrName1 = attrName.ToLower();
                        if (!dic.ContainsKey(attrName1)) dic.Add(attrName1, new RouteAttr()
                        {
                            Name = attrName,
                            Uri = uri,
                            RegisterAddr = registerAddr,
                            RegisterActionType = method.Equals("get") ? MethodTypes.Get : MethodTypes.Post
                        });
                    }
                }
            }

            if (null == microServiceRoute) return;
            if (string.IsNullOrEmpty(RouteName)) return;
            RouteAttr routeAttr = null;
            dic.TryGetValue(microServiceRoute._routeName.ToLower(), out routeAttr);
            if (null == routeAttr) return;
            microServiceRoute._uri = routeAttr.Uri;
        }

        public string RouteName { get { return _routeName; } }

        public string ControllerName { get { return _controllerName; } }

        public string Uri
        {
            get
            {
                _uri = string.Empty;
                if (string.IsNullOrEmpty(_routeName)) _routeName = "";
                RouteAttr routeAttr = null;
                dic.TryGetValue(_routeName.ToLower(), out routeAttr);
                if (null != routeAttr) _uri = routeAttr.Uri;
                return _uri;
            }
        }

        public static void ForEach(Action<RouteAttr> action)
        {
            foreach (var item in dic)
            {
                action(item.Value);
            }
        }

        public class RouteAttr
        {
            public string Name { get; set; }
            public string Uri { get; set; }
            public string RegisterAddr { get; set; }

            private MethodTypes _method = MethodTypes.Get;
            public MethodTypes RegisterActionType
            {
                get { return _method; }
                set { _method = value; }
            }
        }
    }
}
