using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public delegate void MSRouteAttribute(string MSRouteName, string Uri, string RegisterAddr, MethodTypes RegisterActionType);
    /// <summary>
    /// Service interface local mapping interface class identifier
    /// </summary>
    public class MicroServiceRoute : Attribute
    {
        private string _routeName = "";
        private string _controllerName = "";
        private string _uri = "";
        private static string configPath = "";

        private const string routeItem = "Route";
        private const string configFile = "MicroServiceRoute.xml";
        private const string MicroServiceRoutes = "MicroServiceRoutes";

        private static object MSObject = new object();
        private static XmlDoc document = new XmlDoc();
        private static XmlElement rootElement = null;
        private static Dictionary<string, RouteAttr> routeAttrDic = new Dictionary<string, RouteAttr>();
        private static Dictionary<string, XmlElement> eleDic = new Dictionary<string, XmlElement>();

        static MicroServiceRoute()
        {
            configPath = Path.Combine(DJTools.RootPath, configFile);
            msr(null, null, null);
        }

        /// <summary>
        /// Service interface local mapping interface class identifier
        /// </summary>
        /// <param name="RouteName">The 'Name' attribute value of the 'Route' node in the 'MicroServiceRoute.xml' configuration file.</param>
        /// <param name="ControllerName">Remote Service Interface Controller Name</param>
        public MicroServiceRoute(string RouteName, string ControllerName)
        {
            msr(this, RouteName, ControllerName);
        }

        /// <summary>
        /// Service interface local mapping interface class identifier,The default current interface class name is the remote service interface controller name.
        /// </summary>
        /// <param name="RouteName">The 'Name' attribute value of the 'Route' node in the 'MicroServiceRoute.xml' configuration file.</param>
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

            if (0 < routeAttrDic.Count) return;

            string fPath = configPath;
            if (!File.Exists(fPath))
            {
                XmlDoc doc = new XmlDoc();
                XmlElement XMLroot = doc.RootNode(MicroServiceRoutes);

                XmlElement route = doc.CreateElement("Route");
                RouteAttr route_attr = new RouteAttr()
                {
                    Name = "ServiceRoute1",
                    Uri = "http://127.0.0.1:8080,http://127.0.0.1:8081",
                    RegisterAddr = "/Home/RegisterIP?ContractKey=abc123",
                    RegisterActionType = MethodTypes.Post
                };
                route_attr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (typeof(MethodTypes) == pt) return;
                    route.SetAttribute(fn, fv.ToString());
                });
                route.SetAttribute("RegisterActionType", "post");

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
                rootElement = document.Load(fPath);
                if (null == rootElement) return;

                string attrName1 = "";
                RouteAttr routeAttr1 = null;
                foreach (XmlNode routeItem in rootElement.ChildNodes)
                {
                    routeAttr1 = new RouteAttr();
                    foreach (XmlAttribute item in routeItem.Attributes)
                    {
                        routeAttr1.SetPropertyValue(item.Name, item.Value);
                    }

                    if (false == string.IsNullOrEmpty(routeAttr1.Name) && false == string.IsNullOrEmpty(routeAttr1.Uri))
                    {
                        attrName1 = routeAttr1.Name.ToLower();
                        if (routeAttrDic.ContainsKey(attrName1)) continue;
                        routeAttrDic.Add(attrName1, routeAttr1);
                        eleDic.Add(attrName1, (XmlElement)routeItem);
                    }
                }
            }

            if (null == microServiceRoute) return;
            if (string.IsNullOrEmpty(RouteName)) return;
            RouteAttr routeAttr = null;
            routeAttrDic.TryGetValue(microServiceRoute._routeName.ToLower(), out routeAttr);
            if (null == routeAttr) return;
            microServiceRoute._uri = routeAttr.Uri;
        }

        public string RouteName { get { return _routeName; } }

        public string ControllerName { get { return _controllerName; } }

        public string Uri
        {
            get
            {
                lock (MSObject)
                {
                    _uri = string.Empty;
                    if (string.IsNullOrEmpty(_routeName)) _routeName = "";
                    RouteAttr routeAttr = null;
                    routeAttrDic.TryGetValue(_routeName.ToLower(), out routeAttr);
                    if (null != routeAttr) _uri = routeAttr.Uri;
                    return _uri;
                }
            }
        }

        public static void Foreach(MSRouteAttribute routeAttribute)
        {
            lock (MSObject)
            {
                RouteAttr routeAttr = null;
                foreach (var item in routeAttrDic)
                {
                    routeAttr = item.Value;
                    routeAttribute(routeAttr.Name, routeAttr.Uri, routeAttr.RegisterAddr, routeAttr.RegisterActionType);
                }
            }
        }

        public static void Add(string ServiceRouteName, string Uri, string RegisterAddr, MethodTypes RegisterActionType)
        {
            lock (MSObject)
            {
                if (string.IsNullOrEmpty(ServiceRouteName) || string.IsNullOrEmpty(Uri)) return;
                if (null == rootElement) return;
                string key = ServiceRouteName.Trim().ToLower();
                XmlElement routeNode = null;
                eleDic.TryGetValue(key, out routeNode);
                if (null == routeNode)
                {
                    routeNode = document.CreateElement(routeItem);
                    rootElement.AppendChild(routeNode);
                }

                Dictionary<string, XmlAttribute> dic = new Dictionary<string, XmlAttribute>();
                foreach (XmlAttribute item in routeNode.Attributes)
                {
                    dic[item.Name.ToLower()] = item;
                }

                RouteAttr routeAttr = new RouteAttr()
                {
                    Name = ServiceRouteName.Trim(),
                    Uri = Uri.Trim(),
                    RegisterAddr = RegisterAddr.Trim(),
                    RegisterActionType = RegisterActionType
                };

                XmlAttribute attr = null;
                routeAttr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    key = fn.ToLower();
                    attr = null;
                    dic.TryGetValue(key, out attr);
                    if (null == attr)
                    {
                        attr = document.CreateAttribute(fn);
                        routeNode.Attributes.Append(attr);
                    }
                    if (null == fv) fv = "";
                    attr.Value = fv.ToString();
                });

                document.Save(configPath);
            }
        }

        public static void Remove(string ServiceRouteName)
        {
            lock (MSObject)
            {
                if (string.IsNullOrEmpty(ServiceRouteName)) return;
                string key = ServiceRouteName.Trim().ToLower();
                if (!routeAttrDic.ContainsKey(key)) return;
                XmlElement ele = eleDic[key];
                rootElement.RemoveChild(ele);
                routeAttrDic.Remove(key);
                eleDic.Remove(key);
                document.Save(configPath);
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
