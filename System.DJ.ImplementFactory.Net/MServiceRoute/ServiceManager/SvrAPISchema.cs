using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPISchema
    {
        private const string _svrApiFileName = "SvrApiList.xml";
        private const string _rootNodeName = "ServiceApiCollection";
        private const string _serviceName = "ServiceName";
        private const string _contractKey = "SvrContractKey";
        private const string _port = "Port";
        private const string _ip = "IP";

        /// <summary>
        /// key: ServiceName, value: ServiceApiCollection
        /// </summary>
        private static Dictionary<string, SvrAPI> s_svrApiDic = new Dictionary<string, SvrAPI>();
        private static object _svrAPISchameLock = new object();

        private static string s_svrApiPath = "";

        static SvrAPISchema()
        {
            s_svrApiPath = Path.Combine(DJTools.RootPath, _svrApiFileName);
            loadSvrApi();
        }

        private static string GetJsonValueByType(string paraType)
        {
            string jsonValue = "";
            if (null == paraType) return jsonValue;
            paraType = paraType.Trim();
            if (string.IsNullOrEmpty(paraType)) return jsonValue;
            paraType = paraType.ToLower();
            if (-1 != paraType.IndexOf("guid"))
            {
                jsonValue = "\"" + Guid.NewGuid().ToString() + "\"";
            }
            else if (-1 != paraType.IndexOf("time"))
            {
                jsonValue = "\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"";
            }
            else if (-1 != paraType.IndexOf("string"))
            {
                jsonValue = "\"string\"";
            }
            else if (-1 != paraType.IndexOf("bool"))
            {
                jsonValue = "false";
            }
            else if (-1 != paraType.IndexOf("int"))
            {
                jsonValue = "0";
            }
            return jsonValue;
        }

        private static void SetSvrApiDic(XmlNode svrApiNode)
        {
            if (!svrApiNode.HasChildNodes) return;
            XmlAttribute attr = svrApiNode.Attributes[_serviceName];
            if (null == attr) return;
            string serviceName = attr.Value.Trim();

            string contractKeyName = _contractKey;
            if (contractKeyName.Substring(0, 3).ToLower().Equals("svr"))
            {
                contractKeyName = contractKeyName.Substring(3);
            }

            string contractKey = "";
            attr = svrApiNode.Attributes[contractKeyName];
            if (null != attr) contractKey = attr.Value.Trim();
            if (string.IsNullOrEmpty(contractKey)) contractKey = XmlDoc.GetChildTextByNodeName(svrApiNode, contractKeyName);

            string ip = "";
            attr = svrApiNode.Attributes[_ip];
            if (null != attr) ip = attr.Value.Trim();
            if (string.IsNullOrEmpty(ip)) ip = XmlDoc.GetChildTextByNodeName(svrApiNode, _ip);

            string port = "";
            attr = svrApiNode.Attributes[_port];
            if (null != attr) port = attr.Value.Trim();
            if (string.IsNullOrEmpty(port)) port = XmlDoc.GetChildTextByNodeName(svrApiNode, _port);

            SvrAPI svrAPI = null;
            SvrUri svrUri = null;
            string featureName = "";
            string paraJsonData = "";
            string paraName = "";
            string paraType = "";

            svrAPI = new SvrAPI();
            s_svrApiDic[serviceName.ToLower()] = svrAPI;

            svrAPI.ServiceName = serviceName;
            svrAPI.ContractKey = contractKey;
            svrAPI.IP = ip;
            svrAPI.Port = port;

            svrApiNode.ForeachChildNode(item =>
            {
                attr = item.Attributes["Name"];
                if (null == attr) attr = item.Attributes["name"];
                if (null != attr) featureName = attr.Value.Trim();
                if (string.IsNullOrEmpty(featureName)) featureName = XmlDoc.GetChildTextByNodeName(item, "name");
                if (string.IsNullOrEmpty(featureName)) return true;
                svrUri = new SvrUri();
                svrUri.Name = featureName;
                item.ForeachChildNode(itemChild =>
                {
                    if (itemChild.Name.ToLower().Equals("parameters"))
                    {
                        paraJsonData = "";
                        itemChild.ForeachChildNode(paraItem =>
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

                            paraJsonData += ", \"" + paraName + "\": " + GetJsonValueByType(paraType);
                            return true;
                        });

                        if (!string.IsNullOrEmpty(paraJsonData))
                        {
                            paraJsonData = paraJsonData.Substring(1);
                            paraJsonData = paraJsonData.Trim();
                            paraJsonData = "{" + paraJsonData + "}";
                        }
                        svrUri.JsonData = paraJsonData;
                    }
                    else
                    {
                        svrUri.SetPropertyValue(itemChild.Name, itemChild.InnerText.Trim());
                    }
                });

                if ((false == string.IsNullOrEmpty(svrUri.Name)) && (false == string.IsNullOrEmpty(svrUri.Uri)))
                {
                    svrAPI.SvrUris.Add(svrUri);
                }
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

        private void AddToFile(string ip, object data)
        {
            if (null == data) return;
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

            string serviceName = data.GetPropertyValue<string>(_serviceName);
            string svrContractKey = data.GetPropertyValue<string>(_contractKey);
            string port = data.GetPropertyValue<string>(_port);
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
                attr = node.Attributes[_serviceName];
                if (null == attr) return true;
                if (serviceNameLower.Equals(attr.Value.Trim().ToLower()))
                {
                    svrNode = (XmlElement)node;
                    return false;
                }
                return true;
            });

            if (null != svrNode)
            {
                rootNode.RemoveChild(svrNode);
                s_svrApiDic.Remove(serviceNameLower);
            }

            string contractKeyName = _contractKey;
            if (contractKeyName.Substring(0, 3).ToLower().Equals("svr"))
            {
                contractKeyName = contractKeyName.Substring(3);
            }

            svrNode = doc.CreateElement("ServiceApi");
            svrNode.SetAttribute(_serviceName, serviceName);
            svrNode.SetAttribute(contractKeyName, svrContractKey);
            svrNode.SetAttribute(_ip, ip);
            svrNode.SetAttribute(_port, port);
            rootNode.AppendChild(svrNode);

            XmlElement xmlItem = null;
            XmlElement ele = null;
            XmlElement paraItem = null;
            string fnLower = "";
            string paraName = "";
            string paraType = "";
            foreach (object item1 in collection)
            {
                if (null == item1) continue;
                xmlItem = doc.CreateElement("Item");
                svrNode.AppendChild(xmlItem);

                item1.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) fv = "";
                    fnLower = fn.ToLower();
                    if (fnLower.Equals("name"))
                    {
                        xmlItem.SetAttribute(fn, fv.ToString());
                    }
                    else if (fnLower.Equals("parameters"))
                    {
                        ele = doc.CreateElement(fn);
                        xmlItem.AppendChild(ele);
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
                        xmlItem.AppendChild(ele);
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
