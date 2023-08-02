using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPISchema
    {
        private const string _svrApiFileName = "SvrApiList.xml";
        private const string _rootNodeName = "ServiceApiCollection";
        //private const string _serviceName = "ServiceName";
        //private const string _port = "Port";
        private const string _ip = "IP";
        private const string _ip_port_split = ":";

        /// <summary>
        /// key: ServiceName_Lower, value: ServiceApiCollection
        /// </summary>
        private static Dictionary<string, SvrAPI> s_svrApiDic = new Dictionary<string, SvrAPI>();

        /// <summary>
        /// key: ip:port, value: ServiceApiCollection
        /// </summary>
        private static Dictionary<string, SvrAPIOption> ip_port_Dic = new Dictionary<string, SvrAPIOption>();
        private static List<string> s_ServiceNames = new List<string>();
        private static object _svrAPISchameLock = new object();

        private static string s_svrApiPath = "";

        static SvrAPISchema()
        {
            s_svrApiPath = Path.Combine(DJTools.RootPath, _svrApiFileName);
            loadSvrApi();
        }

        private static void SetSvrApiDic(XmlNode svrApiNode)
        {
            if (!svrApiNode.HasChildNodes) return;
            XmlAttribute attr = svrApiNode.Attributes[MicroServiceRoute._ServiceName];
            if (null == attr) return;
            string serviceName = attr.Value.Trim();
            string serviceNameLower = serviceName.ToLower();

            string contractKey = "";
            string ip = "";
            string httpType = "";
            string port = "";

            SvrAPI svrAPI = null;
            SvrUri svrUri = null;
            string featureName = "";
            string paraName = "";
            string paraType = "";

            s_svrApiDic.TryGetValue(serviceNameLower, out svrAPI);
            if (null == svrAPI)
            {
                svrAPI = new SvrAPI();
                svrAPI.ServiceName = serviceName;
                s_svrApiDic[serviceName.ToLower()] = svrAPI;
                s_ServiceNames.Add(serviceName);
            }

            svrAPI.Items.Clear();

            SvrAPIOption svrAPIOption = null;
            svrApiNode.ForeachChildNode(item =>
            {
                attr = item.Attributes[_ip];
                if (null != attr) ip = attr.Value.Trim();
                if (string.IsNullOrEmpty(ip)) ip = XmlDoc.GetChildTextByNodeName(item, _ip);
                if (string.IsNullOrEmpty(ip)) return true;

                attr = item.Attributes[MicroServiceRoute._Port];
                if (null != attr) port = attr.Value.Trim();
                if (string.IsNullOrEmpty(port)) port = XmlDoc.GetChildTextByNodeName(item, MicroServiceRoute._Port);
                if (string.IsNullOrEmpty(port)) return true;

                attr = item.Attributes[MicroServiceRoute._HttpType];
                if (null != attr) httpType = attr.Value.Trim();
                if (string.IsNullOrEmpty(httpType)) httpType = XmlDoc.GetChildTextByNodeName(item, MicroServiceRoute._HttpType);
                if (string.IsNullOrEmpty(httpType)) httpType = "http";

                attr = item.Attributes[MSConst.contractKey];
                if (null != attr) contractKey = attr.Value.Trim();
                if (string.IsNullOrEmpty(contractKey)) ip = XmlDoc.GetChildTextByNodeName(item, MSConst.contractKey);
                if (string.IsNullOrEmpty(contractKey)) return true;

                svrAPIOption = new SvrAPIOption();
                svrAPI.Items.Add(svrAPIOption);
                svrAPIOption.HttpType = httpType;
                svrAPIOption.IP = ip;
                svrAPIOption.Port = port;
                svrAPIOption.ContractKey = contractKey;

                string ip_port = ip + _ip_port_split + port;
                ip_port_Dic[ip_port] = svrAPIOption;

                item.ForeachChildNode(itemChild =>
                {
                    attr = itemChild.Attributes["Name"];
                    if (null == attr) attr = itemChild.Attributes["name"];
                    if (null != attr) featureName = attr.Value.Trim();
                    if (string.IsNullOrEmpty(featureName)) featureName = XmlDoc.GetChildTextByNodeName(itemChild, "name");
                    if (string.IsNullOrEmpty(featureName)) return true;

                    svrUri = new SvrUri();
                    svrUri.Name = featureName;

                    itemChild.ForeachChildNode(child =>
                    {
                        if (child.Name.ToLower().Equals("parameters"))
                        {
                            child.ForeachChildNode(paraItem =>
                            {
                                attr = paraItem.Attributes["Name"];
                                if (null == attr) attr = paraItem.Attributes["name"];
                                if (null != attr) paraName = attr.Value.Trim();
                                if (string.IsNullOrEmpty(paraName)) paraName = XmlDoc.GetChildTextByNodeName(paraItem, "name");
                                if (string.IsNullOrEmpty(paraName)) return true;

                                attr = paraItem.Attributes["Type"];
                                if (null == attr) attr = paraItem.Attributes["type"];
                                if (null != attr) paraType = attr.Value.Trim();
                                if (string.IsNullOrEmpty(paraType)) paraType = XmlDoc.GetChildTextByNodeName(paraItem, "type");
                                if (string.IsNullOrEmpty(paraType)) return true;

                                svrUri.ParameterItems.Add(new ParameterItem()
                                {
                                    ParameterName = paraName,
                                    ParameterType = paraType,
                                });
                                return true;
                            });
                        }
                        else
                        {
                            svrUri.SetPropertyValue(child.Name, child.InnerText.Trim());
                        }
                    });

                    if ((false == string.IsNullOrEmpty(svrUri.Name)) && (false == string.IsNullOrEmpty(svrUri.Uri)))
                    {
                        svrAPIOption.SvrUris.Add(svrUri);
                    }
                    return true;
                });
                return true;
            });
        }

        /// <summary>
        /// Save the configuration interface information data provided by the business function service
        /// </summary>
        /// <param name="ip">Client IP address</param>
        /// <param name="data">Configuration interface information data provided by business function services</param>
        public void Save(string ip, object data)
        {
            lock (_svrAPISchameLock)
            {
                AddToFile(ip, data);
            }
        }

        /// <summary>
        /// Obtain the business function service configuration interface information data
        /// </summary>
        /// <param name="action">Obtain the business function service configuration interface information data</param>
        public void Foreach(Action<SvrAPI> action)
        {
            lock (_svrAPISchameLock)
            {
                if (null == action) return;
                foreach (var item in s_svrApiDic)
                {
                    action(item.Value);
                }
            }
        }

        /// <summary>
        /// Get current all names of service
        /// </summary>
        /// <returns>Return a array of names of service</returns>
        public string[] GetServiceNames()
        {
            lock (_svrAPISchameLock)
            {
                return s_ServiceNames.ToArray();
            }
        }

        /// <summary>
        /// Get the API for a service by service name
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <returns>Return the API for a service</returns>
        public SvrAPI GetServiceAPIByServiceName(string serviceName)
        {
            lock (_svrAPISchameLock)
            {
                SvrAPI svrAPI = null;
                s_svrApiDic.TryGetValue(serviceName.ToLower(), out svrAPI);
                return svrAPI;
            }
        }

        /// <summary>
        /// Get the API by ip and port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public SvrAPIOption GetSvrAPIOptionByIP_Port(string ip, string port)
        {
            lock (_svrAPISchameLock)
            {
                string ip_port = ip + _ip_port_split + port;
                SvrAPIOption svrAPIOption = null;
                ip_port_Dic.TryGetValue(ip_port, out svrAPIOption);
                return svrAPIOption;
            }
        }

        private void AddToFile(string ip, object data)
        {
            if (null == data) return;
            Type tp = data.GetType();
            if ((typeof(JObject) == tp) || (typeof(JToken) == tp))
            {
                string json = data.ToString();
                JsonToEntity jte = new JsonToEntity();
                data = jte.GetObject(json);
            }
            string sdt = data.ToString();
            Regex rg1 = new Regex(@"^ValueKind\s*\=\s*Object\s*\:", RegexOptions.IgnoreCase);
            Regex rg2 = new Regex(@"^\{((\r\n)|.)+\}$", RegexOptions.IgnoreCase);
            if (rg1.IsMatch(sdt))
            {
                sdt = rg1.Replace(sdt, "").Trim();
                if ((false == sdt.Substring(0, 1).Equals("\"")) || (false == sdt.Substring(sdt.Length - 1).Equals("\""))) return;
                sdt = sdt.Substring(1);
                sdt = sdt.Substring(0, sdt.Length - 1);
                JsonToEntity jte = new JsonToEntity();
                data = jte.GetObject(sdt);
            }
            else if (rg2.IsMatch(sdt))
            {
                JsonToEntity jte = new JsonToEntity();
                data = jte.GetObject(sdt);
            }
            if (data.GetType().IsBaseType()) return;

            object items = null;
            data.ForeachProperty((pi, pt, fn, fv) =>
            {
                if (null == fv) return true;
                if (fn.ToLower().Equals("data"))
                {
                    items = fv;
                    return false;
                }
                return true;
            });

            if (null == items) return;
            IEnumerable collection = items as IEnumerable;
            if (null == collection) return;

            string serviceName = data.GetPropertyValue<string>(MicroServiceRoute._ServiceName);
            string svrContractKey = data.GetPropertyValue<string>(MSConst.svrMngcontractKey);
            string port = data.GetPropertyValue<string>(MicroServiceRoute._Port);
            string httpType = data.GetPropertyValue<string>(MicroServiceRoute._HttpType);
            if (null == serviceName) return;
            if (null == svrContractKey) svrContractKey = "";
            serviceName = serviceName.Trim();
            if (string.IsNullOrEmpty(serviceName)) return;

            XmlDoc doc = new XmlDoc();
            XmlElement rootNode = doc.Load(s_svrApiPath);
            if (null == rootNode) rootNode = doc.RootNode(_rootNodeName);

            XmlElement svrNode = null;
            XmlAttribute attr = null;
            string serviceNameLower = serviceName.ToLower();
            rootNode.ForeachChildNode(node =>
            {
                attr = node.Attributes[MicroServiceRoute._ServiceName];
                if (null == attr) return true;
                if (serviceNameLower.Equals(attr.Value.Trim().ToLower()))
                {
                    svrNode = (XmlElement)node;
                    return false;
                }
                return true;
            });

            if (null == svrNode)
            {
                svrNode = doc.CreateElement("ServiceApi");
                svrNode.SetAttribute(MicroServiceRoute._ServiceName, serviceName);
                rootNode.AppendChild(svrNode);
            }

            XmlElement xmlItem = null;
            string ipLower = _ip.ToLower();
            string portLower = MicroServiceRoute._Port.ToLower();
            svrNode.ForeachChildNode(eleItem =>
            {
                XmlAttributeCollection ats = eleItem.Attributes;
                string ip1 = "";
                string port1 = "";
                string anLower = "";
                foreach (XmlAttribute att in ats)
                {
                    anLower = att.Name.ToLower();
                    if (anLower.Equals(ipLower))
                    {
                        ip1 = att.Value.Trim();
                    }
                    else if (anLower.Equals(portLower))
                    {
                        port1 = att.Value.Trim();
                    }
                }

                if (ip1.Equals(ip) && port1.Equals(port))
                {
                    xmlItem = eleItem;
                    return false;
                }
                return true;
            });

            if (null != xmlItem)
            {
                svrNode.RemoveChild(xmlItem);
            }

            xmlItem = doc.CreateElement("Item");
            xmlItem.SetAttribute(_ip, ip);
            xmlItem.SetAttribute(MicroServiceRoute._HttpType, httpType);
            xmlItem.SetAttribute(MicroServiceRoute._Port, port);
            xmlItem.SetAttribute(MSConst.contractKey, svrContractKey);
            svrNode.AppendChild(xmlItem);

            XmlElement ele = null;
            XmlElement paraItem = null;
            XmlElement funNode = null;
            string fnLower = "";
            string paraName = "";
            string paraType = "";
            foreach (object item1 in collection)
            {
                if (null == item1) continue;

                funNode = doc.CreateElement("Function");
                xmlItem.AppendChild(funNode);

                item1.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) fv = "";
                    fnLower = fn.ToLower();
                    if (fnLower.Equals("name"))
                    {
                        funNode.SetAttribute(fn, fv.ToString());
                    }
                    else if (fnLower.Equals("parameters"))
                    {
                        ele = doc.CreateElement(fn);
                        funNode.AppendChild(ele);
                        if (fv.GetType() == typeof(string)) return;
                        IEnumerable paras = fv as IEnumerable;
                        if (null == paras) return;

                        foreach (var p in paras)
                        {
                            if (null == p) continue;
                            paraName = p.GetPropertyValue<string>("name");
                            paraType = p.GetPropertyValue<string>("type");

                            paraItem = doc.CreateElement("Item");
                            ele.AppendChild(paraItem);

                            paraItem.SetAttribute("Name", paraName);
                            paraItem.SetAttribute("Type", paraType);
                        }

                    }
                    else
                    {
                        ele = doc.CreateElement(fn);
                        ele.InnerText = fv.ToString();
                        funNode.AppendChild(ele);
                    }
                });
            }
            SetSvrApiDic(svrNode);
            doc.Save(s_svrApiPath);
        }

        private static void loadSvrApi()
        {
            XmlDoc doc = new XmlDoc();
            XmlElement rootNode = doc.Load(s_svrApiPath);
            if (null == rootNode) return;

            rootNode.ForeachChildNode(svrApiNode =>
            {
                SetSvrApiDic(svrApiNode);
            });
        }
    }
}
