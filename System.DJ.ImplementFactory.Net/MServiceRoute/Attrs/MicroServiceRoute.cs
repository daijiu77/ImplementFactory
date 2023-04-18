﻿using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Service interface local mapping interface class identifier
    /// </summary>
    public class MicroServiceRoute : Attribute
    {
        private string _routeName = "";
        private string _controllerName = "";
        private string _uri = "";
        private const string configFile = "MicroServiceRoute.xml";

        private static Dictionary<string, RouteAttr> dic = new Dictionary<string, RouteAttr>();

        static MicroServiceRoute()
        {
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

        private static void msr(MicroServiceRoute microServiceRoute, string RouteName, string ControllerName)
        {
            if (null != microServiceRoute)
            {
                microServiceRoute._routeName = RouteName;
                microServiceRoute._controllerName = ControllerName;
            }

            if (0 < dic.Count) return;

            const string MicroServiceRoutes = "MicroServiceRoutes";
            string fPath = Path.Combine(DJTools.RootPath, configFile);
            if (!File.Exists(fPath))
            {
                XmlDoc doc = new XmlDoc();
                XmlElement XMLroot = doc.RootNode(MicroServiceRoutes);

                XmlElement route = doc.CreateElement("Route");
                RouteAttr route_attr = new RouteAttr()
                {
                    Name = "ServiceRoute1",
                    Uri = "http://127.0.0.1:8080,http://127.0.0.1:8081",
                    RegisterAddr = "/Home/RegisterIP?ContractKey=abc123",
                    RegisterActionType = MethodTypes.Post
                };
                route_attr.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (typeof(MethodTypes) == pt) return;
                    route.SetAttribute(fn, fv.ToString());
                });
                route.SetAttribute("RegisterActionType", "post");

                XMLroot.AppendChild(route);

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
                XmlDoc document = new XmlDoc();
                document.Load(fPath);
                XmlNode node = document.RootNode(MicroServiceRoutes);
                if (null == node) return;

                string attrName1 = "";
                RouteAttr routeAttr1 = null;
                foreach (XmlNode routeItem in node.ChildNodes)
                {
                    routeAttr1 = new RouteAttr();
                    foreach (XmlAttribute item in routeItem.Attributes)
                    {
                        routeAttr1.SetPropertyValue(item.Name, item.Value);
                    }

                    if (false == string.IsNullOrEmpty(routeAttr1.Name) && false == string.IsNullOrEmpty(routeAttr1.Uri))
                    {
                        attrName1 = routeAttr1.Name.ToLower();
                        if (!dic.ContainsKey(attrName1)) dic.Add(attrName1, routeAttr1);
                    }
                }
            }

            if (null == microServiceRoute) return;
            if (string.IsNullOrEmpty(RouteName)) return;
            RouteAttr routeAttr = null;
            dic.TryGetValue(microServiceRoute._routeName.ToLower(), out routeAttr);
            if (null == routeAttr) return;
            microServiceRoute._uri = routeAttr.Uri;
        }

        public string RouteName { get { return _routeName; } }

        public string ControllerName { get { return _controllerName; } }

        public string Uri
        {
            get
            {
                _uri = string.Empty;
                if (string.IsNullOrEmpty(_routeName)) _routeName = "";
                RouteAttr routeAttr = null;
                dic.TryGetValue(_routeName.ToLower(), out routeAttr);
                if (null != routeAttr) _uri = routeAttr.Uri;
                return _uri;
            }
        }

        public static void ForEach(Action<RouteAttr> action)
        {
            foreach (var item in dic)
            {
                action(item.Value);
            }
        }

        public class RouteAttr
        {
            public string Name { get; set; }
            public string Uri { get; set; }
            public string RegisterAddr { get; set; }

            private MethodTypes _method = MethodTypes.Get;
            public MethodTypes RegisterActionType
            {
                get { return _method; }
                set { _method = value; }
            }
        }
    }
}
