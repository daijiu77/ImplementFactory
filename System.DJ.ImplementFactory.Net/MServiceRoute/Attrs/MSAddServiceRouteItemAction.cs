using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// To add a service route entry, specify that the method parameters need to include: ServiceRouteName(RouteName), Uri, RegisterAddr(addr), RegisterActionType(actionType)
    /// </summary>
    public class MSAddServiceRouteItemAction : AbsActionFilterAttribute
    {
        private string routeName = "routeName";
        private string uri = "uri";
        private string addr = "addr";
        private string actionType = "actionType";
        /// <summary>
        /// To add a service route entry, specify that the method parameters need to include: ServiceRouteName(RouteName), Uri, RegisterAddr(addr), RegisterActionType(actionType)
        /// </summary>
        public MSAddServiceRouteItemAction() { }

        /// <summary>
        /// To add a service route entry
        /// </summary>
        /// <param name="serviceRouteNameMapping">The parameter name of the ServiceRouteName mapping</param>
        /// <param name="uriMapping">The parameter name of the Uri mapping</param>
        /// <param name="registerAddrMapping">The parameter name of the RegisterAddr mapping</param>
        /// <param name="registerActionTypeMapping">The parameter name of the RegisterActionType(Get|Post) mapping</param>
        public MSAddServiceRouteItemAction(string serviceRouteNameMapping, string uriMapping, string registerAddrMapping, string registerActionTypeMapping)
        {
            this.routeName = serviceRouteNameMapping;
            this.uri = uriMapping;
            this.addr = registerAddrMapping;
            this.actionType = registerActionTypeMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<string> list = new List<string>()
            {
               routeName,
               uri,
               addr,
               actionType
            };
            int size = list.Count;
            for (int i = 0; i < size; i++)
            {
                list[i] = list[i].Trim();
            }
            Dictionary<string, object> dic = GetKVListFromQuery(context.HttpContext, list, true);
            if (0 == dic.Count)
            {
                dic = GetKVListFromHeader(context.HttpContext, list, true);
            }

            if (0 == dic.Count)
            {
                dic = GetKVListFromBody(context.HttpContext, list, true);
            }

            if (0 == dic.Count)
            {
                dic = GetKVListFromForm(context.HttpContext, list, true);
            }

            if (0 < dic.Count)
            {
                size = list.Count;
                for (int i = 0; i < size; i++)
                {
                    list[i] = list[i].ToLower();
                }
                RouteAttr routeAttr = new RouteAttr();
                foreach (var item in dic)
                {
                    if (-1 != item.Key.ToLower().IndexOf(list[0]))
                    {
                        if (null != item.Value) routeAttr.Name = item.Value.ToString();
                    }
                    else if (-1 != item.Key.ToLower().IndexOf(list[1]))
                    {
                        if (null != item.Value) routeAttr.Uri = item.Value.ToString();
                    }
                    else if (-1 != item.Key.ToLower().IndexOf(list[2]))
                    {
                        if (null != item.Value) routeAttr.RegisterAddr = item.Value.ToString();
                    }
                    else if (-1 != item.Key.ToLower().IndexOf(list[3]))
                    {
                        if (null != item.Value)
                        {
                            int num = 0;
                            string s = item.Value.ToString().Trim();
                            int.TryParse(s, out num);
                            MethodTypes methodTypes = (MethodTypes)num;
                            if (0 > num || 1 < num) methodTypes = MethodTypes.Post;
                            routeAttr.RegisterActionType = methodTypes;
                        }
                    }
                }

                if ((false == string.IsNullOrEmpty(routeAttr.Name)) && false == string.IsNullOrEmpty(routeAttr.Uri))
                {
                    MicroServiceRoute.Add(routeAttr.Name, routeAttr.Uri, routeAttr.RegisterAddr, routeAttr.RegisterActionType);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
