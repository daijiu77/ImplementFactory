using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MServiceManagerConfigAction : AbsSysAttributer
    {
        #region Variable names are prohibited from being changed
        private string NameMapping = "serviceManagerName";
        private string UriMapping = "httpUri";
        private string ServiceManagerAddrMapping = "serviceManagerAddr";
        private string ServiceManagerActionTypeMapping = "serviceManagerActionType";
        private string TestAddrMapping = "testAddr";
        private string RegisterAddrMapping = "registerAddr";
        private string RegisterActionTypeMapping = "registerActionType";
        private string ContractKeyMapping = MServiceConst.contractKey;
        #endregion

        /// <summary>
        /// Identify the API method as receiving service manager related parameters and set them to a configuration file
        /// </summary>
        public MServiceManagerConfigAction()
        {
            //
        }

        /// <summary>
        /// Identify the API method as receiving service manager related parameters and set them to a configuration file
        /// </summary>
        /// <param name="serviceNameMapping">The parameter name of the serviceName mapping</param>
        /// <param name="uriMapping">The parameter name of the http uri mapping</param>
        /// <param name="serviceManagerAddrMapping">The parameter name of the serviceManagerAddr mapping</param>
        /// <param name="serviceManagerActionTypeMapping">The parameter name of the serviceManagerActionType(Get|Post) mapping</param>
        /// <param name="testAddrMapping">The parameter name of the testAddr mapping</param>
        /// <param name="registerAddrMapping">The parameter name of the registerAddr mapping</param>
        /// <param name="registerActionTypeMapping">The parameter name of the registerActionType(Get|Post) mapping</param>
        /// <param name="contractKeyMapping">The parameter name of the contractKey mapping</param>
        public MServiceManagerConfigAction(string serviceNameMapping, string uriMapping, string serviceManagerAddrMapping, string serviceManagerActionTypeMapping, string testAddrMapping, string registerAddrMapping, string registerActionTypeMapping, string contractKeyMapping)
        {
            NameMapping = serviceNameMapping;
            UriMapping = uriMapping;
            ServiceManagerAddrMapping = serviceManagerAddrMapping;
            ServiceManagerActionTypeMapping = serviceManagerActionTypeMapping;
            TestAddrMapping = testAddrMapping;
            RegisterAddrMapping = registerAddrMapping;
            RegisterActionTypeMapping = registerActionTypeMapping;
            ContractKeyMapping = contractKeyMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<string> list = new List<string>();
            ForeachFields<MServiceManagerConfigAction>((OriginFieldName, field, fieldVal) =>
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
                MServiceManager manager = new MServiceManager();
                ForeachFields<MServiceManagerConfigAction>((OriginFieldName, field, fieldVal) =>
                {
                    key = fieldVal.ToString().ToLower();
                    vObj = "";
                    if (map.ContainsKey(key))
                    {
                        vObj = map[key];
                    }
                    manager.SetPropertyValue(field, vObj);
                });
                MicroServiceRoute.SetServiceManager(manager);
            }
            base.OnActionExecuting(context);
        }

    }
}
