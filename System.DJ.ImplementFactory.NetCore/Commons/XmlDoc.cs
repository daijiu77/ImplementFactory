using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.DJ.ImplementFactory.Commons
{
    public class XmlDoc
    {
        XmlDocument doc = null;
        public XmlDoc()
        {
            //
        }

        private void CreateXmlDocument()
        {
            doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);
        }

        /// <summary>
        /// Gets the root node of the XML and creates it if it does not exist
        /// </summary>
        /// <param name="nodeName">The name of the root node</param>
        /// <param name="attributes">Properties of the root node</param>
        /// <returns>Returns the root node</returns>
        public XmlElement RootNode(string nodeName, Dictionary<string, string> attributes)
        {
            XmlElement node = null;
            if (null == doc)
            {
                CreateXmlDocument();
            }

            if (2 == doc.ChildNodes.Count)
            {
                node = (XmlElement)doc.ChildNodes[1];
            }
            else
            {
                node = doc.CreateElement(nodeName);
                doc.AppendChild(node);
            }

            if (null != attributes)
            {
                Dictionary<string, XmlAttribute> dic = new Dictionary<string, XmlAttribute>();
                string kn = "";
                foreach (XmlAttribute item in node.Attributes)
                {
                    kn = item.Name.ToLower();
                    if (dic.ContainsKey(kn)) continue;
                    dic[kn] = item;
                }

                foreach (var item in attributes)
                {
                    kn = item.Key.Trim().ToLower();
                    XmlAttribute attr = null;
                    if (dic.ContainsKey(kn))
                    {
                        attr = dic[kn];
                    }
                    else
                    {
                        attr = doc.CreateAttribute(item.Key.Trim());
                        node.Attributes.Append(attr);
                    }                    
                    attr.Value = item.Value;                    
                }
            }
            return node;
        }

        /// <summary>
        /// Gets the root node of the XML and creates it if it does not exist
        /// </summary>
        /// <param name="nodeName">The name of the root node</param>
        /// <returns>Returns the root node</returns>
        public XmlElement RootNode(string nodeName)
        {
            return RootNode(nodeName, null);
        }

        public XmlNode Load(string xmlPath)
        {
            if (!File.Exists(xmlPath)) return null;
            doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode node = null;
            if (2 == doc.ChildNodes.Count)
            {
                node = doc.ChildNodes[1];
            }
            return node;
        }

        public XmlElement CreateElement(string nodeName)
        {
            return doc.CreateElement(nodeName);
        }

        public XmlNode CreateNode(string nodeName)
        {
            return doc.CreateElement(nodeName);
        }

        public XmlAttribute CreateAttribute(string name)
        {
            return doc.CreateAttribute(name);
        }

        public void Save(string xmlPath)
        {
            doc.Save(xmlPath);
        }
    }
}
