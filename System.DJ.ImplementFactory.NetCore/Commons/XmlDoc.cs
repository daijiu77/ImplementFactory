using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.DJ.ImplementFactory.Commons
{
    public class XmlDoc
    {
        private static object s_XmlDocLock = new object();
        private static XmlNode s_parentNode = null;
        private static Dictionary<string, XmlNode> s_eleDic = new Dictionary<string, XmlNode>();

        private XmlDocument doc = null;

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

        /// <summary>
        /// Loads a valid XML file and returns the root node if it exists
        /// </summary>
        /// <param name="xmlPath">The physical path to the XML file</param>
        /// <returns>Return the root node if it exist</returns>
        public XmlElement Load(string xmlPath)
        {
            if (!File.Exists(xmlPath)) return null;
            doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlElement node = null;
            if (2 == doc.ChildNodes.Count)
            {
                node = (XmlElement)doc.ChildNodes[1];
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

        public static XmlNode GetChildNodeByNodeName(XmlNode parentNode, string nodeName, ref string text)
        {
            lock (s_XmlDocLock)
            {
                text = "";
                XmlNode node = null;
                if (!parentNode.HasChildNodes) return node;

                string nodeNameLower = nodeName.ToLower();
                if (parentNode.Equals(s_parentNode) && (0 < s_eleDic.Count))
                {
                    s_eleDic.TryGetValue(nodeNameLower, out node);
                    if (null != node) text = node.InnerText.Trim();
                    return node;
                }

                s_parentNode = parentNode;
                s_eleDic.Clear();
                string nnLower = "";
                string txt = "";
                parentNode.ForeachChildNode(item =>
                {
                    nnLower = item.Name.ToLower();
                    s_eleDic[nnLower] = item;
                    if (nodeNameLower.Equals(nnLower))
                    {
                        node = item;
                        txt = item.InnerText.Trim();
                    }
                });
                text = txt;
                return node;
            }
        }

        public static XmlNode GetChildNodeByNodeName(XmlNode parentNode, string nodeName)
        {
            lock (s_XmlDocLock)
            {
                string text = "";
                return GetChildNodeByNodeName(parentNode, nodeName, ref text);
            }
        }

        public static string GetChildTextByNodeName(XmlNode parentNode, string nodeName)
        {
            lock (s_XmlDocLock)
            {
                string text = "";
                GetChildNodeByNodeName(parentNode, nodeName, ref text);
                return text;
            }
        }

        public void Save(string xmlPath)
        {
            doc.Save(xmlPath);
        }
    }
}
