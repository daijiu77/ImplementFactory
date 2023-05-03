using System.Web.Mvc;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// In the MicroServiceRoute.xml file, add a new subkey under the Routes node, specify that the method parameters need to include: ServiceRouteName(RouteName), Uri, RegisterAddr(addr), RegisterActionType(actionType)
    /// </summary>
    public class MSAddServiceRouteItemAction : AbsActionFilterAttribute
    {
        #region Variable names are prohibited from being changed
        private string NameMapping = "Name";
        private string UriMapping = "Uri";
        private string TestAddrMapping = "testAddr";
        private string RegisterAddrMapping = "registerAddr";
        private string RegisterActionTypeMapping = "registerActionType";
        private string ContractKeyMapping = MServiceConst.contractKey;
        #endregion
        /// <summary>
        /// In the MicroServiceRoute.xml file, add a new subkey under the Routes node, specify that the method parameters need to include: ServiceRouteName(RouteName), Uri, RegisterAddr(addr), RegisterActionType(actionType)
        /// </summary>
        public MSAddServiceRouteItemAction() { }

        /// <summary>
        /// In the MicroServiceRoute.xml file, add a new subkey under the Routes node
        /// </summary>
        /// <param name="serviceRouteNameMapping">The parameter name of the ServiceRouteName mapping</param>
        /// <param name="uriMapping">The parameter name of the Uri mapping</param>
        /// <param name="registerAddrMapping">The parameter name of the RegisterAddr mapping</param>
        /// <param name="testAddrMapping">The parameter name of the testAddr mapping</param>
        /// <param name="contractKeyMapping">The parameter name of the contractKey mapping</param>
        /// <param name="registerActionTypeMapping">The parameter name of the RegisterActionType(Get|Post) mapping</param>
        public MSAddServiceRouteItemAction(string serviceRouteNameMapping, string uriMapping, string registerAddrMapping, string testAddrMapping, string contractKeyMapping, string registerActionTypeMapping)
        {
            this.NameMapping = serviceRouteNameMapping;
            this.UriMapping = uriMapping;
            this.RegisterAddrMapping = registerAddrMapping;
            this.TestAddrMapping = testAddrMapping;
            this.ContractKeyMapping = contractKeyMapping;
            this.RegisterActionTypeMapping = registerActionTypeMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<string> list = new List<string>();
            ForeachFields<MSAddServiceRouteItemAction>((OriginFieldName, field, fieldVal) =>
            {
                list.Add(fieldVal.ToString().Trim());
            });

            Dictionary<string, object> map = GetKVListFromBody(context.HttpContext, list, false);
            if (0 == map.Count) map = GetKVListFromForm(context.HttpContext, list, false);
            if (0 == map.Count) map = GetKVListFromQuery(context.HttpContext, list, false);
            if (0 == map.Count) map = GetKVListFromHeader(context.HttpContext, list, false);

            if (0 < map.Count)
            {
                object vObj = null;
                string key = "";
                RouteAttr routeAttr = new RouteAttr();
                ForeachFields<MSAddServiceRouteItemAction>((OriginFieldName, field, fieldVal) =>
                {
                    key = fieldVal.ToString().ToLower();
                    vObj = "";
                    if (map.ContainsKey(key))
                    {
                        vObj = map[key];
                    }
                    routeAttr.SetPropertyValue(field, vObj);
                });

                if ((false == string.IsNullOrEmpty(routeAttr.Name))
                    && (false == string.IsNullOrEmpty(routeAttr.Uri))
                    && (false == string.IsNullOrEmpty(routeAttr.ContractKey)))
                {
                    MicroServiceRoute.Add(routeAttr.Name,
                        routeAttr.Uri,
                        routeAttr.RegisterAddr,
                        routeAttr.TestAddr,
                        routeAttr.ContractKey,
                        routeAttr.RegisterActionType);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
