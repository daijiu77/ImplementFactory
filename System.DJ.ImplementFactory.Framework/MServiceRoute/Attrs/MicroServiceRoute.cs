using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public delegate void MSRouteAttribute(string MSRouteName, string Uri, string RegisterAddr, string contractValue, MethodTypes RegisterActionType);
    /// <summary>
    /// Service interface local mapping interface class identifier
    /// </summary>
    public class MicroServiceRoute : Attribute
    {
        private string route_name = "";
        private string controller_name = "";
        private string uri_str = "";        

        private const string _routeItem = "Route";
        private const string _configFile = "MicroServiceRoute.xml";
        private const string _microServiceRoutes = "MicroServiceRoutes";

        private static string s_config_path = "";
        private static object s_MSObject = new object();
        private static XmlDoc s_document = new XmlDoc();
        private static XmlElement s_rootElement = null;
        private static Dictionary<string, RouteAttr> s_routeAttrDic = new Dictionary<string, RouteAttr>();
        private static Dictionary<string, XmlElement> s_eleDic = new Dictionary<string, XmlElement>();

        public static ServiceManager s_serviceManager = null;
        public static string s_ServiceName = "";

        static MicroServiceRoute()
        {
            s_config_path = Path.Combine(DJTools.RootPath, _configFile);
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
                microServiceRoute.route_name = RouteName;
                microServiceRoute.controller_name = ControllerName;
            }

            if (0 < s_routeAttrDic.Count) return;

            const string _ServiceManager = "ServiceManager";
            const string _Routes = "Routes";
            const string _ServiceName = "ServiceName";

            string fPath = s_config_path;
            if (!File.Exists(fPath))
            {
                XmlDoc doc = new XmlDoc();
                XmlElement XMLroot = doc.RootNode(_microServiceRoutes);
                ServiceManager serviceMng = new ServiceManager()
                {
                    Name = _ServiceManager,
                    Uri = "http://127.0.0.1:5000/api",
                    ServiceManagerAddr = "/Home/ReceiveManage",
                    ServiceManagerActionType = MethodTypes.Post,
                    RegisterAddr = "/Home/RegisterIP",
                    RegisterActionType = MethodTypes.Post,
                    ContractKey = "abc2233"
                };
                XMLroot.SetAttribute(_ServiceName, "MemberService");

                XmlElement serviceManagerNodes = doc.CreateElement(_ServiceManager);
                XMLroot.AppendChild(serviceManagerNodes);

                XmlElement smItem = null;
                serviceMng.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) return;
                    smItem = doc.CreateElement(fn);
                    smItem.InnerText = fv.ToString();
                    serviceManagerNodes.AppendChild(smItem);
                });

                XmlElement routeNodes = doc.CreateElement(_Routes);
                XMLroot.AppendChild(routeNodes);

                XmlElement route = doc.CreateElement("Route");
                RouteAttr route_attr = new RouteAttr()
                {
                    Name = "ServiceRoute1",
                    Uri = "http://127.0.0.1:8080,http://127.0.0.1:8081",
                    RegisterAddr = "/Home/RegisterIP",
                    RegisterActionType = MethodTypes.Post,
                    ContractKey = "abc"
                };
                route_attr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (typeof(MethodTypes) == pt) return;
                    route.SetAttribute(fn, fv.ToString());
                });
                route.SetAttribute("RegisterActionType", "post");

                routeNodes.AppendChild(route);

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
                s_rootElement = s_document.Load(fPath);
                if (null == s_rootElement) return;

                XmlAttribute atr = s_rootElement.Attributes[_ServiceName];
                if (null != atr) s_ServiceName = atr.Value.Trim();

                s_serviceManager = new ServiceManager();
                string nodeName = "";
                string routeName1 = "";
                string serviceManagerLower = _ServiceManager.ToLower();
                string routesLower = _Routes.ToLower();
                RouteAttr routeAttr1 = null;
                foreach (XmlNode node in s_rootElement.ChildNodes)
                {
                    nodeName = node.Name.ToLower();
                    if (nodeName.Equals(serviceManagerLower))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                        {
                            s_serviceManager.SetPropertyValue(item.Name, item.InnerText.Trim());
                        }
                    }
                    else if (nodeName.Equals(routesLower))
                    {
                        foreach (XmlNode _routeItem in node.ChildNodes)
                        {
                            if (null == _routeItem) continue;
                            if (null == _routeItem.Attributes) continue;
                            if (0 == _routeItem.Attributes.Count) continue;
                            routeAttr1 = new RouteAttr();
                            foreach (XmlAttribute item in _routeItem.Attributes)
                            {
                                routeAttr1.SetPropertyValue(item.Name, item.Value);
                            }

                            if (false == string.IsNullOrEmpty(routeAttr1.Name)
                                && false == string.IsNullOrEmpty(routeAttr1.Uri))
                            {
                                routeName1 = routeAttr1.Name.ToLower();
                                if (s_routeAttrDic.ContainsKey(routeName1)) continue;
                                s_routeAttrDic.Add(routeName1, routeAttr1);
                                s_eleDic.Add(routeName1, (XmlElement)_routeItem);
                            }
                        }
                    }
                }

                if ((false == string.IsNullOrEmpty(s_serviceManager.Uri)))
                {
                    routeName1 = s_serviceManager.Name.ToLower();
                    s_routeAttrDic.Add(routeName1, s_serviceManager);
                    s_eleDic.Add(routeName1, s_rootElement);
                }

            }

            if (null == microServiceRoute) return;
            if (string.IsNullOrEmpty(RouteName)) return;
            RouteAttr routeAttr = null;
            s_routeAttrDic.TryGetValue(microServiceRoute.route_name.ToLower(), out routeAttr);
            if (null == routeAttr) return;
            microServiceRoute.uri_str = routeAttr.Uri;
        }

        public string RouteName { get { return route_name; } }

        public string ControllerName { get { return controller_name; } }

        public string Uri
        {
            get
            {
                lock (s_MSObject)
                {
                    uri_str = string.Empty;
                    if (string.IsNullOrEmpty(route_name)) route_name = "";
                    RouteAttr routeAttr = null;
                    s_routeAttrDic.TryGetValue(route_name.ToLower(), out routeAttr);
                    if (null != routeAttr) uri_str = routeAttr.Uri;
                    return uri_str;
                }
            }
        }

        public static void Foreach(MSRouteAttribute routeAttribute)
        {
            lock (s_MSObject)
            {
                RouteAttr routeAttr = null;
                foreach (var item in s_routeAttrDic)
                {
                    routeAttr = item.Value;
                    routeAttribute(routeAttr.Name, routeAttr.Uri, routeAttr.RegisterAddr, routeAttr.ContractKey, routeAttr.RegisterActionType);
                }
            }
        }

        public static void Add(string ServiceRouteName, string Uri, string RegisterAddr, string ContractValue, MethodTypes RegisterActionType)
        {
            lock (s_MSObject)
            {
                if (string.IsNullOrEmpty(ServiceRouteName) || string.IsNullOrEmpty(Uri)) return;
                if (null == s_rootElement) return;
                string key = ServiceRouteName.Trim().ToLower();
                XmlElement routeNode = null;
                s_eleDic.TryGetValue(key, out routeNode);
                RouteAttr routeAttr = null;
                if (null == routeNode)
                {
                    routeNode = s_document.CreateElement(_routeItem);
                    s_rootElement.AppendChild(routeNode);
                    s_eleDic.Add(key, routeNode);

                    routeAttr = new RouteAttr();
                    s_routeAttrDic.Add(key, routeAttr);
                }
                else
                {
                    routeAttr = s_routeAttrDic[key];
                }

                Dictionary<string, XmlAttribute> dic = new Dictionary<string, XmlAttribute>();
                foreach (XmlAttribute item in routeNode.Attributes)
                {
                    dic[item.Name.ToLower()] = item;
                }

                routeAttr.Name = ServiceRouteName.Trim();
                routeAttr.Uri = Uri.Trim();
                routeAttr.RegisterAddr = RegisterAddr.Trim();
                routeAttr.RegisterActionType = RegisterActionType;
                routeAttr.ContractKey = ContractValue;

                XmlAttribute attr = null;
                routeAttr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    key = fn.ToLower();
                    attr = null;
                    dic.TryGetValue(key, out attr);
                    if (null == attr)
                    {
                        attr = s_document.CreateAttribute(fn);
                        routeNode.Attributes.Append(attr);
                    }
                    if (null == fv) fv = "";
                    attr.Value = fv.ToString();
                });

                s_document.Save(s_config_path);
            }
        }

        public static void Remove(string ServiceRouteName)
        {
            lock (s_MSObject)
            {
                if (string.IsNullOrEmpty(ServiceRouteName)) return;
                string key = ServiceRouteName.Trim().ToLower();
                if (!s_routeAttrDic.ContainsKey(key)) return;
                XmlElement ele = s_eleDic[key];
                s_rootElement.RemoveChild(ele);
                s_routeAttrDic.Remove(key);
                s_eleDic.Remove(key);
                s_document.Save(s_config_path);
            }
        }

        public static RouteAttr GetRouteAttributeByName(string name)
        {
            lock (s_MSObject)
            {
                RouteAttr routeAttr = null;
                if (null == name) return routeAttr;
                name = name.Trim();
                if (string.IsNullOrEmpty(name)) return routeAttr;
                s_routeAttrDic.TryGetValue(name.ToLower(), out routeAttr);
                return routeAttr;
            }
        }

        public class RouteAttr
        {
            public string Name { get; set; }
            public string Uri { get; set; }
            public string RegisterAddr { get; set; }

            public string ContractKey { get; set; }

            private MethodTypes _method = MethodTypes.Get;
            public MethodTypes RegisterActionType
            {
                get { return _method; }
                set { _method = value; }
            }
        }

        public class ServiceManager : RouteAttr
        {
            public string ServiceManagerAddr { get; set; }
            private MethodTypes _method1 = MethodTypes.Get;
            public MethodTypes ServiceManagerActionType
            {
                get { return _method1; }
                set { _method1 = value; }
            }
        }
    }
}
