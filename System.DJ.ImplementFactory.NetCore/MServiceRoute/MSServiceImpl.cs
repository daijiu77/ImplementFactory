using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSServiceImpl : IMSService
    {
        private const string SvrIPAddressFile = "SvrIPAddr.xml";
        private readonly Regex rgIP = new Regex(@"^[1-9]{1,3}\:[0-9]{1,3}\:[0-9]{1,3}\:([1-9]{1,3})$", RegexOptions.IgnoreCase);
        private string filePath = "";

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

        void IMSService.SaveIPAddr(string IPAddr)
        {
            lock (this)
            {
                SaveIPAddress(IPAddr);
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

        public virtual void SaveIPAddress(string IPAddress)
        {
            if (string.IsNullOrEmpty(IPAddress)) return;
            string ip = IPAddress.Trim();
            if (!rgIP.IsMatch(ip)) return;

            XmlDocument doc = new XmlDocument();
            if(File.Exists(filePath)) doc.Load(filePath);

            XmlNode ipNodes = null;
            XmlElement ele = null;
            if (2 > doc.ChildNodes.Count)
            {
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);

                ipNodes = doc.CreateElement("IPAddresses");
                doc.AppendChild(ipNodes);
            }
            else
            {
                ipNodes = doc.ChildNodes[1];
            }

            ele = doc.CreateElement("IPAddr");
            ele.InnerText = IPAddress;
            ipNodes.AppendChild(ele);

            doc.Save(filePath);
        }
    }
}
