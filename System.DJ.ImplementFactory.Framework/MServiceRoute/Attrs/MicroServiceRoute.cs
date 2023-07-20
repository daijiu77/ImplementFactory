using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Reflection;
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
        private const string _RegisterActionType = "RegisterActionType";
        private const string _Routes = "Routes";
        private const string _Route = "Route";
        private const string _Groups = "Groups";
        private const string _ServiceName = "ServiceName";
        private const string _Port = "Port";
        private const string _DataSync = "DataSyncs";

        private static string s_config_path = "";
        private static object s_MSObject = new object();
        private static XmlDoc s_document = new XmlDoc();
        private static XmlElement s_rootElement = null;
        private static Guid s_id = Guid.NewGuid();

        /// <summary>
        /// key: routeName_lower, value: RouteAttr
        /// </summary>
        private static Dictionary<string, RouteAttr> s_routeAttrDic = new Dictionary<string, RouteAttr>();
        /// <summary>
        /// key: routeName_lower, value: node
        /// </summary>
        private static Dictionary<string, XmlElement> s_eleDic = new Dictionary<string, XmlElement>();

        /// <summary>
        /// key: groupsName_lower, value: groupsRoute
        /// </summary>
        private static Dictionary<string, GroupsRoute> s_groupDic = new Dictionary<string, GroupsRoute>();

        /// <summary>
        /// key: routeName_Lower, value: DataSyncs-Name
        /// </summary>
        private static Dictionary<string, string> s_routeName_syncName = new Dictionary<string, string>();

        /// <summary>
        /// key: DataSyncs-Name_Lower, value: config and data
        /// </summary>
        private static Dictionary<string, DataSyncConfigList<DataSyncConfig>> DataSyncDic = new Dictionary<string, DataSyncConfigList<DataSyncConfig>>();
        public static MServiceManager ServiceManager = null;
        public static string ServiceName { get; private set; } = "";
        public static string Port { get; private set; } = "";

        public static Guid ID
        {
            get
            {
                return s_id;
            }
        }

        public static string Key
        {
            get
            {
                return ServiceName + "@" + s_id.ToString();
            }
        }

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

        /// <summary>
        /// Load the routing cluster
        /// </summary>
        /// <typeparam name="T">An interface type identified by the MicroServiceRoute property</typeparam>
        /// <returns>Returns the routed cluster interface instance class</returns>
        public static List<T> GroupsRouteVisit<T>()
        {
            List<T> list = new List<T>();
            if ((null == ImplementAdapter.microServiceMethod) || (null == ImplementAdapter.codeCompiler)) return list;

            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface) return list;

            MicroServiceRoute msr = interfaceType.GetCustomAttribute(typeof(MicroServiceRoute), true) as MicroServiceRoute;
            if (msr == null) return list;

            string groupName = msr.RouteName;
            GroupsRoute groupsRoute = null;
            s_groupDic.TryGetValue(groupName.ToLower(), out groupsRoute);
            if (null == groupsRoute) return list;
            string controllerName = msr.ControllerName;
            Type type = null;
            object iObj = null;
            AutoCall autoCall = new AutoCall();
            foreach (var item in groupsRoute)
            {
                type = ImplementAdapter.microServiceMethod.GetMS(ImplementAdapter.codeCompiler, autoCall, new MicroServiceRoute(item.Name, controllerName), interfaceType);
                if (null == type) continue;
                try
                {
                    iObj = Activator.CreateInstance(type);
                    list.Add((T)iObj);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            return list;
        }

        private static void ResetServiceManager(XmlNode serviceManagerNode)
        {
            lock (s_MSObject)
            {
                if (null == MicroServiceRoute.ServiceManager) MicroServiceRoute.ServiceManager = new MServiceManager();
                serviceManagerNode.ForeachChildNode(item =>
                {
                    MicroServiceRoute.ServiceManager.SetPropertyValue(item.Name, item.InnerText.Trim());
                });

                if ((false == string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.Uri))
                    && (false == string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.Name))
                    && (false == string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.RegisterAddr)))
                {
                    string routeName1 = MicroServiceRoute.ServiceManager.Name.ToLower();
                    s_routeAttrDic.Remove(routeName1);
                    s_eleDic.Remove(routeName1);

                    s_routeAttrDic.Add(routeName1, MicroServiceRoute.ServiceManager);
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
                InitXml_ServiceManage(doc, XMLroot);

                InitXml_DataSync(doc, XMLroot);

                InitXml_Routes(doc, XMLroot);

                InitXml_Groups(doc, XMLroot);

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
                if (null != atr) MicroServiceRoute.ServiceName = atr.Value.Trim();
                if (string.IsNullOrEmpty(MicroServiceRoute.ServiceName)) MicroServiceRoute.ServiceName = XmlDoc.GetChildTextByNodeName(s_rootElement, _ServiceName);

                atr = s_rootElement.Attributes[_Port];
                if (null != atr) MicroServiceRoute.Port = atr.Value.Trim();
                if (string.IsNullOrEmpty(MicroServiceRoute.Port)) MicroServiceRoute.Port = XmlDoc.GetChildTextByNodeName(s_rootElement, _Port);

                string nodeName = "";
                string serviceManagerLower = _ServiceManager.ToLower();
                string routesLower = _Routes.ToLower();
                string groupsLower = _Groups.ToLower();
                string dataSyncLower = _DataSync.ToLower();
                string dataSyncsName = "";
                RouteAttr routeAttr1 = null;
                s_rootElement.ForeachChildNode(node =>
                {
                    nodeName = node.Name.ToLower();
                    if (nodeName.Equals(serviceManagerLower))
                    {
                        ResetServiceManager(node);
                    }
                    else if (nodeName.Equals(dataSyncLower))
                    {
                        XmlAttribute attr = node.Attributes["Name"];
                        if (null == attr) attr = node.Attributes["name"];
                        if (null == attr) return;
                        dataSyncsName = attr.Value.Trim();
                        string dataSyncsNameLower = dataSyncsName.ToLower();
                        DataSyncConfig dataSync = null;
                        DataSyncConfigList<DataSyncConfig> dataSyncConfigs = null;
                        DataSyncDic.TryGetValue(dataSyncsNameLower, out dataSyncConfigs);
                        if (null == dataSyncConfigs)
                        {
                            dataSyncConfigs = new DataSyncConfigList<DataSyncConfig>();
                            DataSyncDic[dataSyncsNameLower] = dataSyncConfigs;
                        }
                        node.ForeachChildNode(nodeItem =>
                        {
                            dataSync = InitValFromNode<DataSyncConfig>(nodeItem);
                            dataSync.GroupName = dataSyncsName;
                            if (string.IsNullOrEmpty(dataSync.GroupName)) throw new Exception("The Uri '{0}' lost a value of GroupName in the data sync.".ExtFormat(dataSync.Uri));
                            dataSyncConfigs.Add(dataSync);
                            s_routeName_syncName[dataSync.Name.ToLower()] = dataSync.GroupName;
                        });
                    }
                    else if (nodeName.Equals(groupsLower))
                    {
                        XmlAttribute attr = node.Attributes["Name"];
                        if (null == attr) attr = node.Attributes["name"];
                        if (null == attr) return;
                        string groupName = attr.Value;
                        GroupsRoute groupsRoute = new GroupsRoute();
                        groupsRoute.Name = groupName;
                        s_groupDic[groupName.ToLower()] = groupsRoute;
                        node.ForeachChildNode(_routeItem =>
                        {
                            routeAttr1 = InitValFromNode<RouteAttr>(_routeItem);
                            groupsRoute.Children.Add(routeAttr1);
                            return true;
                        });
                    }
                    else if (nodeName.Equals(routesLower))
                    {
                        node.ForeachChildNode(_routeItem =>
                        {
                            routeAttr1 = InitValFromNode<RouteAttr>(_routeItem);
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

        #region Initial defaul datas in the configuration file        
        private static void InitXml_ServiceManage(XmlDoc doc, XmlElement XMLroot)
        {
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
        }

        private static void InitXml_Routes(XmlDoc doc, XmlElement XMLroot)
        {
            XmlElement routeNodes = doc.CreateElement(_Routes);
            XMLroot.AppendChild(routeNodes);

            XmlElement route = doc.CreateElement(_Route);
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
            route.SetAttribute(_RegisterActionType, "post");
            routeNodes.AppendChild(route);
        }

        private static void InitXml_Groups(XmlDoc doc, XmlElement XMLroot)
        {
            #region 路由集群，表示所有路由目标地址都有相同的访问接口，可以通过 GroupsRouteVisit 静态方法获取                
            List<RouteAttr> groups = new List<RouteAttr>();
            groups.Add(new RouteAttr()
            {
                Name = "MemberService",
                Uri = "http://127.0.0.1:5000",
                RegisterAddr = "/Home/RegisterIP",
                TestAddr = "/Home/Test",
                RegisterActionType = MethodTypes.Post,
                ContractKey = "abc"
            });

            groups.Add(new RouteAttr()
            {
                Name = "OrderService",
                Uri = "http://127.0.0.1:5001",
                RegisterAddr = "/Home/RegisterIP",
                TestAddr = "/Home/Test",
                RegisterActionType = MethodTypes.Post,
                ContractKey = "abc"
            });

            XmlElement groupsNode = doc.CreateElement(_Groups);
            groupsNode.SetAttribute("Name", "BaseInfoRoutes");
            XMLroot.AppendChild(groupsNode);

            XmlElement route = null;
            foreach (RouteAttr item in groups)
            {
                route = doc.CreateElement(_Route);
                item.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (typeof(MethodTypes) == pt) return;
                    XmlElement ele = doc.CreateElement(fn);
                    if (null == fv) fv = "";
                    ele.InnerText = fv.ToString().Trim();
                    route.AppendChild(ele);
                });
                route.SetAttribute(_RegisterActionType, "post");
                groupsNode.AppendChild(route);
            }

            #endregion
        }

        private static void InitXml_DataSync(XmlDoc doc, XmlElement XMLroot)
        {
            List<DataSyncConfig> groups = new List<DataSyncConfig>();
            groups.Add(new DataSyncConfig()
            {
                GroupName = "UserInfoSync",
                Name = "MemberService",
                Uri = "http://127.0.0.1:5000",
                RegisterAddr = "/Home/RegisterIP",
                TestAddr = "/Home/Test",
                RegisterActionType = MethodTypes.Post,
                ContractKey = "abc"
            });

            XmlElement dataSyncNodes = doc.CreateElement(_DataSync);
            XMLroot.AppendChild(dataSyncNodes);

            string registerAction = _RegisterActionType.ToLower();
            foreach (DataSyncConfig item in groups)
            {
                XmlElement route = doc.CreateElement(_routeItem);
                dataSyncNodes.AppendChild(route);
                item.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (fn.ToLower().Equals(registerAction)) return false;
                    if (null == fv) fv = "";
                    XmlElement ele = doc.CreateElement(fn);
                    ele.InnerText = fv.ToString();
                    route.AppendChild(ele);
                    return true;
                });
                XmlElement registerNode = doc.CreateElement(_RegisterActionType);
                registerNode.InnerText = Enum.GetName(typeof(MethodTypes), item.RegisterActionType);
                route.AppendChild(registerNode);
            }
        }
        #endregion

        private static T InitValFromNode<T>(XmlElement _ele) where T : RouteAttr
        {
            T _routeAttr1 = (T)Activator.CreateInstance(typeof(T));
            foreach (XmlAttribute item in _ele.Attributes)
            {
                _routeAttr1.SetPropertyValue(item.Name, item.Value);
            }

            _routeAttr1.GetType().ForeachProperty((pi, pt, fn) =>
            {
                string txt = XmlDoc.GetChildTextByNodeName(_ele, fn);
                if (!string.IsNullOrEmpty(txt)) _routeAttr1.SetPropertyValue(fn, txt);
            });

            if (false == string.IsNullOrEmpty(_routeAttr1.Name)
                        && false == string.IsNullOrEmpty(_routeAttr1.Uri))
            {
                string routeName1 = _routeAttr1.Name.ToLower();
                if (s_routeAttrDic.ContainsKey(routeName1)) return (T)_routeAttr1;
                s_routeAttrDic.Add(routeName1, _routeAttr1);
                s_eleDic.Add(routeName1, (XmlElement)_ele);
            }
            return (T)_routeAttr1;
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

        public static void Add(string ServiceRouteName, string Uri, string RegisterAddr, string TestAddr, string ContractValue, MethodTypes RegisterActionType)
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
                routeAttr.TestAddr = TestAddr.Trim();
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
                name = name.Replace("\\", "/");
                if (name.Substring(0, 1).Equals("/")) name = name.Substring(1);
                s_routeAttrDic.TryGetValue(name.ToLower(), out routeAttr);
                return routeAttr;
            }
        }

        public static DataSyncConfigList<DataSyncConfig> GetDataSyncMessageByName(string datSyncsName)
        {
            lock (s_MSObject)
            {
                string dn = datSyncsName.Trim().ToLower();
                DataSyncConfigList<DataSyncConfig> syncConfigs = null;
                DataSyncDic.TryGetValue(dn, out syncConfigs);
                return syncConfigs;
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
                    svrMngNode = XmlDoc.GetChildNodeByNodeName(s_rootElement, _ServiceManager) as XmlElement;
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

        public static string GetDataSyncsNameByRouteName(string routeName)
        {
            if (null == routeName) return null;
            string fn = routeName.Trim().ToLower();
            if (!s_routeName_syncName.ContainsKey(fn)) return null;
            return s_routeName_syncName[fn];
        }

    }
}
