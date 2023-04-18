using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSServiceImpl : IMSService
    {
        private Dictionary<string, string> ipDic = new Dictionary<string, string>();
        private const string SvrIPAddressFile = "SvrIPAddr.xml";
        private readonly Regex rgIP = new Regex(@"^[1-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.([1-9]{1,3})$", RegexOptions.IgnoreCase);
        private string filePath = "";
        private const string rootNode = "IPAddresses";
        public const string startName = "StartTime";
        public const string endName = "EndTime";
        public const string contractKey = "ContractKey";

        public MSServiceImpl()
        {
            filePath = Path.Combine(DJTools.RootPath, SvrIPAddressFile);
        }

        List<string> IMSService.IPAddrSources()
        {
            lock (this)
            {
                return GetIPAddresses();
            }
        }

        bool IMSService.SaveIPAddr(string IPAddr, string contractKey)
        {
            lock (this)
            {
                return SaveIPAddress(IPAddr, contractKey);
            }
        }

        bool IMSService.SetEnabledTime(DateTime startTime, DateTime endTime, string contractKey)
        {
            lock (this)
            {
                return SetEnabledTime(startTime, endTime, contractKey);
            }
        }

        public virtual List<string> GetIPAddresses()
        {
            List<string> ipAddrs = new List<string>();
            if (!File.Exists(filePath)) return ipAddrs;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            if (2 > doc.ChildNodes.Count) return ipAddrs;
            XmlNode node = doc.ChildNodes[1];
            string ip = "";
            foreach (XmlNode item in node.ChildNodes)
            {
                ip = item.InnerText.Trim();
                if (!rgIP.IsMatch(ip)) continue;
                ipAddrs.Add(ip);
            }
            return ipAddrs;
        }

        public virtual bool SaveIPAddress(string IPAddress, string contractKey)
        {
            bool mbool = false;
            if (string.IsNullOrEmpty(IPAddress)) return mbool;
            string ip = IPAddress.Trim();
            if (!rgIP.IsMatch(ip)) return mbool;

            XmlDoc doc = new XmlDoc();
            doc.Load(filePath);
                        
            XmlNode ipNodes = doc.RootNode(rootNode, null);
            XmlElement ele = null;

            XmlAttribute att = ipNodes.Attributes[startName];
            if (null == att) return mbool;//throw new Exception("The XmlAttribute(" + startName + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");

            DateTime startTime = DateTime.Now;
            DateTime.TryParse(att.Value.Trim(), out startTime);

            att = ipNodes.Attributes[endName];
            if (null == att) return mbool;//throw new Exception("The XmlAttribute(" + endName + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");

            DateTime endTime = DateTime.Now;
            DateTime.TryParse(att.Value.Trim(), out endTime);

            DateTime dt = DateTime.Now;
            if (dt < startTime || dt > endTime) return mbool;//throw new Exception("Invalid validity period start or end time.");

            att = ipNodes.Attributes[MSServiceImpl.contractKey];
            if (null == att) return mbool;//throw new Exception("The XmlAttribute(" + MSServiceImpl.contractKey + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");
            string key = att.Value.Trim();
            if (!key.Equals(contractKey)) return mbool;//throw new Exception("Invalid contract string.");

            if (0 == ipDic.Count)
            {
                string txt = "";
                foreach (XmlNode item in ipNodes.ChildNodes)
                {
                    txt = item.InnerText.Trim();
                    if (!rgIP.IsMatch(txt)) continue;
                    ipDic[txt] = txt;
                }
            }

            if (ipDic.ContainsKey(IPAddress)) return mbool;
            ipDic.Add(IPAddress, IPAddress);

            ele = doc.CreateElement("IPAddr");
            ele.InnerText = IPAddress;
            ipNodes.AppendChild(ele);

            doc.Save(filePath);
            mbool = true;
            return mbool;
        }

        public virtual bool SetEnabledTime(DateTime startTime, DateTime endTime, string contractKey)
        {
            bool mbool = false;
            XmlDoc doc = new XmlDoc();
            doc.Load(filePath);

            XmlNode ipNodes = doc.RootNode(rootNode);            
            string startNameL = startName.ToLower();
            string endNameL = endName.ToLower();
            string contractKeyL = MSServiceImpl.contractKey.ToLower();
            Dictionary<string, XmlAttribute> dic = new Dictionary<string, XmlAttribute>();
            XmlAttributeCollection atc = ipNodes.Attributes;
            foreach (XmlAttribute item in atc)
            {
                if (item.Name.ToLower().Equals(startNameL))
                {
                    dic[startNameL] = item;
                }
                else if (item.Name.ToLower().Equals(endNameL))
                {
                    dic[endNameL] = item;
                }
                else if (item.Name.ToLower().Equals(contractKeyL))
                {
                    dic[contractKeyL] = item;
                }
            }

            XmlAttribute attr = null;
            if (!dic.ContainsKey(startNameL))
            {
                attr = doc.CreateAttribute(startName);
                ipNodes.Attributes.Append(attr);
                dic[startNameL] = attr;
            }

            if (!dic.ContainsKey(endNameL))
            {
                attr = doc.CreateAttribute(endName);
                ipNodes.Attributes.Append(attr);
                dic[endNameL] = attr;
            }

            if (!dic.ContainsKey(contractKeyL))
            {
                attr = doc.CreateAttribute(MSServiceImpl.contractKey);
                ipNodes.Attributes.Append(attr);
                dic[contractKeyL] = attr;
            }

            dic[startNameL].Value = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            dic[endNameL].Value = endTime.ToString("yyyy-MM-dd HH:mm:ss");
            dic[contractKeyL].Value = contractKey;

            doc.Save(filePath);
            mbool = true;
            return mbool;
        }
    }
}
