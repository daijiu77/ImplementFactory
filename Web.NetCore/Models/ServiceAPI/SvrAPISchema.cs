using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Xml;

namespace Web.NetCore.Models.ServiceAPI
{
    public class SvrAPISchema
    {
        private const string _svrApiFileName = "SvrApiList.xml";
        private const string _rootNodeName = "ServiceApiCollection";
        private const string _serviceName = "ServiceName";
        private const string _contractKey = "SvrContractKey";

        /// <summary>
        /// key: ServiceName, value: ServiceApiCollection
        /// </summary>
        private static Dictionary<string, SvrAPI> s_svrApiDic = new Dictionary<string, SvrAPI>();

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

        public void Add(object data)
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
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (!node.HasChildNodes) continue;
                attr = node.Attributes[_serviceName];
                if (null == attr) continue;
                if (serviceNameLower.Equals(attr.Value.Trim().ToLower()))
                {
                    svrNode = (XmlElement)node;
                    break;
                }
            }

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

            SvrAPI svrAPI = new SvrAPI();
            s_svrApiDic.Add(serviceNameLower, svrAPI);
            svrAPI.ServiceName = serviceName;
            svrAPI.ContractKey = svrContractKey;

            svrNode = doc.CreateElement("ServiceApi");
            svrNode.SetAttribute(_serviceName, serviceName);
            svrNode.SetAttribute(contractKeyName, svrContractKey);
            rootNode.AppendChild(svrNode);

            XmlElement xmlItem = null;
            XmlElement ele = null;
            XmlElement paraItem = null;
            string fnLower = "";
            string paraJsonData = "";
            string paraName = "";
            string paraType = "";
            foreach (object item1 in collection)
            {
                if (null == item1) continue;
                xmlItem = doc.CreateElement("Item");
                svrNode.AppendChild(xmlItem);

                SvrUri svrUri = new SvrUri();                
                item1.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) fv = "";
                    fnLower = fn.ToLower();
                    if (fnLower.Equals("name"))
                    {
                        xmlItem.SetAttribute(fn, fv.ToString());
                        svrUri.Name = fv.ToString();
                    }
                    else if (fnLower.Equals("parameters"))
                    {
                        ele = doc.CreateElement(fn);
                        xmlItem.AppendChild(ele);
                        if (fv.GetType() == typeof(string)) return;
                        IEnumerable paras = fv as IEnumerable;
                        if (null == paras) return;

                        paraJsonData = "";
                        foreach (var p in paras)
                        {
                            if (null == p) continue;
                            paraName = p.GetPropertyValue<string>("name");
                            paraType = p.GetPropertyValue<string>("type");

                            paraItem = doc.CreateElement("Item");
                            ele.AppendChild(paraItem);

                            paraItem.SetAttribute("Name", paraName);
                            paraItem.SetAttribute("Type", paraType);

                            paraJsonData += ", \"" + paraName + "\": " + GetJsonValueByType(paraType);
                        }

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
                        ele = doc.CreateElement(fn);
                        ele.InnerText = fv.ToString();
                        xmlItem.AppendChild(ele);
                        svrUri.SetPropertyValue(fn, fv);
                    }
                });
                                
                svrAPI.SvrUris.Add(svrUri);
            }
            doc.Save(s_svrApiPath);
        }

        private static void loadSvrApi()
        {
            XmlDoc doc = new XmlDoc();
            XmlElement rootNode = doc.Load(s_svrApiPath);
            if (null == rootNode) return;

            XmlAttribute attr = null;
            string serviceName = "";
            string contractKey = "";
            string contractKeyName = _contractKey;
            if (contractKeyName.Substring(0, 3).ToLower().Equals("svr"))
            {
                contractKeyName = contractKeyName.Substring(3);
            }

            SvrAPI svrAPI = null;
            SvrUri svrUri = null;
            string featureName = "";
            string paraJsonData = "";
            string paraName = "";
            string paraType = "";
            foreach (XmlNode svrApiNode in rootNode.ChildNodes)
            {
                if (!svrApiNode.HasChildNodes) continue;
                attr = svrApiNode.Attributes[_serviceName];
                if (null == attr) continue;
                serviceName = attr.Value.Trim();

                contractKey = "";
                attr = svrApiNode.Attributes[contractKeyName];
                if (null != attr) contractKey = attr.Value.Trim();

                svrAPI = new SvrAPI();
                s_svrApiDic.Add(serviceName.ToLower(), svrAPI);

                svrAPI.ServiceName = serviceName;
                svrAPI.ContractKey = contractKey;

                foreach (XmlNode item in svrApiNode.ChildNodes)
                {
                    if (!item.HasChildNodes) continue;
                    attr = item.Attributes["Name"];
                    if (null == attr) attr = item.Attributes["name"];
                    if (null != attr) featureName = attr.Value.Trim();
                    svrUri = new SvrUri();
                    svrUri.Name = featureName;
                    foreach (XmlNode itemChild in item.ChildNodes)
                    {
                        if (!itemChild.HasChildNodes) continue;
                        if (itemChild.Name.ToLower().Equals("parameters"))
                        {
                            paraJsonData = "";
                            foreach (XmlNode paraItem in itemChild.ChildNodes)
                            {
                                attr = paraItem.Attributes["Name"];
                                if (null == attr) attr = paraItem.Attributes["name"];
                                if (null == attr) continue;
                                paraName = attr.Value.Trim();
                                if (string.IsNullOrEmpty(paraName)) continue;

                                attr = paraItem.Attributes["Type"];
                                if (null == attr) attr = paraItem.Attributes["type"];
                                if (null == attr) continue;
                                paraType = attr.Value.Trim();
                                if (string.IsNullOrEmpty(paraType)) continue;

                                paraJsonData += ", \"" + paraName + "\": " + GetJsonValueByType(paraType);
                            }

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
                    }

                    if ((false == string.IsNullOrEmpty(svrUri.Name)) && (false == string.IsNullOrEmpty(svrUri.Uri)))
                    {
                        svrAPI.SvrUris.Add(svrUri);
                    }
                }
            }
            //end
        }
    }
}
