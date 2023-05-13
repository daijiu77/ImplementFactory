using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Remove the children under the Routes node in the MicroServiceRoute.xml file based on the Route-Name value
    /// </summary>
    public class MSRemoveServiceRouteItemAction : AbsSysAttributer
    {
        private string ServiceRouteName = "RouteName";
        /// <summary>
        /// Remove the children under the Routes node in the MicroServiceRoute.xml file based on the Route-Name value
        /// </summary>
        public MSRemoveServiceRouteItemAction() { }

        /// <summary>
        /// Remove the children under the Routes node in the MicroServiceRoute.xml file based on the Route-Name value
        /// </summary>
        /// <param name="serviceRouteNameMapping">The parameter name of the ServiceRouteName mapping</param>
        public MSRemoveServiceRouteItemAction(string serviceRouteNameMapping)
        {
            this.ServiceRouteName = serviceRouteNameMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<string> list = new List<string>() { ServiceRouteName };
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
                string routoName = "";
                foreach (var item in dic)
                {
                    if (null != item.Value)
                    {
                        routoName = item.Value.ToString().Trim();
                    }
                }

                MicroServiceRoute.Remove(routoName);
            }
            base.OnActionExecuting(context);
        }
    }
}
