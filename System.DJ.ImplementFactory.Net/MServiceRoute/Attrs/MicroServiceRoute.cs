using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public delegate void MSRouteAttribute(string MSRouteName, string Uri, string RegisterAddr, string TestAddr, string contractValue, MethodTypes RegisterActionType);
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
        private const string _ServiceManager = "ServiceManager";
        private const string _Routes = "Routes";
        private const string _ServiceName = "ServiceName";
        private const string _Port = "Port";

        private static string s_config_path = "";
        private static object s_MSObject = new object();
        private static XmlDoc s_document = new XmlDoc();
        private static XmlElement s_rootElement = null;

        /// <summary>
        /// key: serviceName_lower, value: RouteAttr
        /// </summary>
        private static Dictionary<string, RouteAttr> s_routeAttrDic = new Dictionary<string, RouteAttr>();
        /// <summary>
        /// key: serviceName_lower, value: node
        /// </summary>
        private static Dictionary<string, XmlElement> s_eleDic = new Dictionary<string, XmlElement>();

        public static MServiceManager s_serviceManager = null;
        public static string s_ServiceName { get; private set; } = "";
        public static string s_Port { get; private set; } = "";

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

        private static void ResetServiceManager(XmlNode serviceManagerNode)
        {
            lock (s_MSObject)
            {
                if (null == s_serviceManager) s_serviceManager = new MServiceManager();
                serviceManagerNode.ForeachChildNode(item =>
                {
                    s_serviceManager.SetPropertyValue(item.Name, item.InnerText.Trim());
                });

                if ((false == string.IsNullOrEmpty(s_serviceManager.Uri))
                    && (false == string.IsNullOrEmpty(s_serviceManager.Name))
                    && (false == string.IsNullOrEmpty(s_serviceManager.RegisterAddr)))
                {
                    string routeName1 = s_serviceManager.Name.ToLower();
                    s_routeAttrDic.Remove(routeName1);
                    s_eleDic.Remove(routeName1);

                    s_routeAttrDic.Add(routeName1, s_serviceManager);
                    s_eleDic.Add(routeName1, (XmlElement)serviceManagerNode);
                }
            }
        }

        private static void msr(MicroServiceRoute microServiceRoute, string RouteName, string ControllerName)
        {
            if (null != microServiceRoute)
            {
                microServiceRoute.route_name = RouteName;
                microServiceRoute.controller_name = ControllerName;
            }

            if (0 < s_routeAttrDic.Count) return;

            string fPath = s_config_path;
            if (!File.Exists(fPath))
            {
                XmlDoc doc = new XmlDoc();
                XmlElement XMLroot = doc.RootNode(_microServiceRoutes);
                MServiceManager serviceMng = new MServiceManager()
                {
                    Name = _ServiceManager,
                    Uri = "http://127.0.0.1:5000/api",
                    ServiceManagerAddr = "/Home/ReceiveManage",
                    ServiceManagerActionType = MethodTypes.Post,
                    RegisterAddr = "/Home/RegisterIP",
                    TestAddr = "/Home/Test",
                    RegisterActionType = MethodTypes.Post,
                    ContractKey = "abc2233"
                };
                XMLroot.SetAttribute(_ServiceName, "MemberService");
                XMLroot.SetAttribute(_Port, "5000");

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
                    TestAddr = "/Home/Test",
                    RegisterActionType = MethodTypes.Post,
                    ContractKey = "abc"
                };
                route_attr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (typeof(MethodTypes) == pt) return;
                    XmlElement ele = doc.CreateElement(fn);
                    if (null == fv) fv = "";
                    ele.InnerText = fv.ToString().Trim();
                    route.AppendChild(ele);
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
                if (string.IsNullOrEmpty(s_ServiceName)) s_ServiceName = XmlDoc.GetChildTextByNodeName(s_rootElement, _ServiceName);

                atr = s_rootElement.Attributes[_Port];
                if (null != atr) s_Port = atr.Value.Trim();
                if (string.IsNullOrEmpty(s_Port)) s_Port = XmlDoc.GetChildTextByNodeName(s_rootElement, _Port);

                string nodeName = "";
                string routeName1 = "";
                string txt = "";
                string serviceManagerLower = _ServiceManager.ToLower();
                string routesLower = _Routes.ToLower();
                RouteAttr routeAttr1 = null;
                s_rootElement.ForeachChildNode(node =>
                {
                    nodeName = node.Name.ToLower();
                    if (nodeName.Equals(serviceManagerLower))
                    {
                        ResetServiceManager(node);
                    }
                    else if (nodeName.Equals(routesLower))
                    {
                        node.ForeachChildNode(_routeItem =>
                        {
                            routeAttr1 = new RouteAttr();
                            foreach (XmlAttribute item in _routeItem.Attributes)
                            {
                                routeAttr1.SetPropertyValue(item.Name, item.Value);
                            }

                            routeAttr1.GetType().ForeachProperty((pi, pt, fn) =>
                            {
                                txt = XmlDoc.GetChildTextByNodeName(_routeItem, fn);
                                if (!string.IsNullOrEmpty(txt)) routeAttr1.SetPropertyValue(fn, txt);
                            });

                            if (false == string.IsNullOrEmpty(routeAttr1.Name)
                                && false == string.IsNullOrEmpty(routeAttr1.Uri))
                            {
                                routeName1 = routeAttr1.Name.ToLower();
                                if (s_routeAttrDic.ContainsKey(routeName1)) return true;
                                s_routeAttrDic.Add(routeName1, routeAttr1);
                                s_eleDic.Add(routeName1, (XmlElement)_routeItem);
                            }
                            return true;
                        });
                    }
                });
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
                    routeAttribute(routeAttr.Name, routeAttr.Uri, routeAttr.RegisterAddr, routeAttr.TestAddr, routeAttr.ContractKey, routeAttr.RegisterActionType);
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

        public static void SetServiceManager(MServiceManager manager)
        {
            lock (s_MSObject)
            {
                if (null == manager || null == s_rootElement) return;
                if (string.IsNullOrEmpty(manager.Uri)
                    || string.IsNullOrEmpty(manager.Name)
                    || string.IsNullOrEmpty(manager.ServiceManagerAddr)) return;

                string serviceNameLower = manager.Name.ToLower();
                XmlElement svrMngNode = null;
                s_eleDic.TryGetValue(serviceNameLower, out svrMngNode);

                if (null == svrMngNode)
                {
                    svrMngNode =  XmlDoc.GetChildNodeByNodeName(s_rootElement, _ServiceManager) as XmlElement;
                }

                if (null == svrMngNode)
                {
                    svrMngNode = s_document.CreateElement(_ServiceManager);
                    s_rootElement.AppendChild(svrMngNode);
                }

                string nodeNameLower = "";
                Dictionary<string, XmlElement> dic = new Dictionary<string, XmlElement>();
                svrMngNode.ForeachChildNode(item =>
                {
                    nodeNameLower = item.Name.ToLower();
                    dic[nodeNameLower] = item;
                });

                XmlElement ele = null;
                manager.ForeachProperty((pi, pt, fn, fv) =>
                {
                    nodeNameLower = fn.ToLower();
                    if (null == fv) fv = "";
                    if (dic.ContainsKey(nodeNameLower))
                    {
                        dic[nodeNameLower].InnerText = fv.ToString();
                    }
                    else
                    {
                        ele = s_document.CreateElement(fn);
                        ele.InnerText = fv.ToString();
                        svrMngNode.AppendChild(ele);
                    }
                });

                s_document.Save(s_config_path);
                ResetServiceManager(svrMngNode);
            }
        }

    }
}
