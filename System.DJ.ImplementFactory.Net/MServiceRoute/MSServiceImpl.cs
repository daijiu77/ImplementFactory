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
        private readonly Regex rgIP = new Regex(@"^[1-9][0-9]{0,2}\.[0-9]{1,3}\.[0-9]{1,3}\.([1-9][0-9]{0,2})$", RegexOptions.IgnoreCase);
        private string filePath = "";
        private static string contractValue = "";
        private const string rootNode = "IPAddresses";
        
        private event changeEnabled ChangeEnabled = null;
        private event registerIP RegisterIP = null;

        static MSServiceImpl()
        {
            MSServiceImpl mSServiceImpl = new MSServiceImpl();
            if (!File.Exists(mSServiceImpl.filePath))
            {
                DateTime start = DateTime.Now;
                DateTime end = DateTime.Now.AddHours(24);
                mSServiceImpl.SetEnabledTime(start, end, Guid.NewGuid().ToString());
            }
        }

        public MSServiceImpl()
        {
            filePath = Path.Combine(DJTools.RootPath, SvrIPAddressFile);            
        }

        event changeEnabled IMSService.ChangeEnabled
        {
            add
            {
                ChangeEnabled += value;
            }

            remove
            {
                ChangeEnabled -= value;
            }
        }

        event registerIP IMSService.RegisterIP
        {
            add
            {
                RegisterIP += value;
            }

            remove
            {
                RegisterIP -= value;
            }
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

        public static string GetContractValue()
        {
            if (!string.IsNullOrEmpty(contractValue)) return contractValue;
            string filePath1 = Path.Combine(DJTools.RootPath, SvrIPAddressFile);
            XmlDoc doc = new XmlDoc();
            XmlNode ipNodes = doc.Load(filePath1);
            if (null == ipNodes) return contractValue;
            XmlAttribute att = ipNodes.Attributes[MServiceConst.contractKey];
            if (null == att) return contractValue;
            contractValue = att.Value.Trim();
            return contractValue;
        }

        public virtual List<string> GetIPAddresses()
        {
            List<string> ipAddrs = new List<string>();
            if (!File.Exists(filePath)) return ipAddrs;
            XmlDoc doc = new XmlDoc();
            XmlNode node = doc.Load(filePath);
            if (null == node) return ipAddrs;
            string ip = "";
            node.ForeachChildNode(item =>
            {
                ip = item.InnerText.Trim();
                if (!rgIP.IsMatch(ip)) return true;
                ipAddrs.Add(ip);
                return true;
            });
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

            XmlAttribute att = ipNodes.Attributes[MServiceConst.startName];
            if (null == att) throw new Exception("The XmlAttribute(" + MServiceConst.startName + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");
            //if (null == att) return mbool;

            DateTime startTime = DateTime.Now;
            DateTime.TryParse(att.Value.Trim(), out startTime);

            att = ipNodes.Attributes[MServiceConst.endName];
            if (null == att) throw new Exception("The XmlAttribute(" + MServiceConst.endName + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");
            //if (null == att) return mbool;

            DateTime endTime = DateTime.Now;
            DateTime.TryParse(att.Value.Trim(), out endTime);

            DateTime dt = DateTime.Now;
            if (dt < startTime || dt > endTime) throw new Exception("Invalid validity period start or end time.");
            //if (dt < startTime || dt > endTime) return mbool;

            att = ipNodes.Attributes[MServiceConst.contractKey];
            if (null == att) throw new Exception("The XmlAttribute(" + MServiceConst.contractKey + ") is missing in XmlNode 'IPAddresses' of file '" + SvrIPAddressFile + "'.");
            //if (null == att) return mbool;

            string key = att.Value.Trim();
            if (!key.Equals(contractKey)) throw new Exception("Invalid contract string.");
            if (!key.Equals(contractKey)) return mbool;

            if (0 == ipDic.Count)
            {
                string txt = "";
                ipNodes.ForeachChildNode(item =>
                {
                    txt = item.InnerText.Trim();
                    if (!rgIP.IsMatch(txt)) return true;
                    ipDic[txt] = txt;
                    return true;
                });
            }

            if (ipDic.ContainsKey(IPAddress)) return mbool;
            ipDic.Add(IPAddress, IPAddress);

            ele = doc.CreateElement("IPAddr");
            ele.InnerText = IPAddress;
            ipNodes.AppendChild(ele);

            doc.Save(filePath);
            mbool = true;

            if (null != RegisterIP)
            {
                try
                {
                    RegisterIP(IPAddress);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            return mbool;
        }

        public virtual bool SetEnabledTime(DateTime startTime, DateTime endTime, string contractKey)
        {
            bool mbool = false;
            XmlDoc doc = new XmlDoc();
            doc.Load(filePath);

            XmlNode ipNodes = doc.RootNode(rootNode);
            string startNameL = MServiceConst.startName.ToLower();
            string endNameL = MServiceConst.endName.ToLower();
            string contractKeyL = MServiceConst.contractKey.ToLower();
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
                attr = doc.CreateAttribute(MServiceConst.startName);
                ipNodes.Attributes.Append(attr);
                dic[startNameL] = attr;
            }

            if (!dic.ContainsKey(endNameL))
            {
                attr = doc.CreateAttribute(MServiceConst.endName);
                ipNodes.Attributes.Append(attr);
                dic[endNameL] = attr;
            }

            if (!dic.ContainsKey(contractKeyL))
            {
                attr = doc.CreateAttribute(MServiceConst.contractKey);
                ipNodes.Attributes.Append(attr);
                dic[contractKeyL] = attr;
            }

            dic[startNameL].Value = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            dic[endNameL].Value = endTime.ToString("yyyy-MM-dd HH:mm:ss");
            dic[contractKeyL].Value = contractKey;
            contractValue = contractKey;

            doc.Save(filePath);
            mbool = true;

            if (null != ChangeEnabled)
            {
                try
                {
                    ChangeEnabled(startTime, endTime, contractKey);
                }
                catch (Exception)
                {

                    //throw;
                }
            }

            return mbool;
        }
    }
}
