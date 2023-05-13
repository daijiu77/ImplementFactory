using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Linq;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Identify the client registration method, which must have a 'ContractKey' parameter of character type
    /// </summary>
    public class MSClientRegisterAction : AbsSysAttributer
    {
        private string _contractKey = MServiceConst.contractKey.ToLower();
        /// <summary>
        /// Identify the client registration method, which must have a 'ContractKey' parameter of character type.
        /// </summary>
        public MSClientRegisterAction() { }

        /// <summary>
        /// Identify the client registration method, which must have a 'ContractKey' parameter of character type.
        /// </summary>
        /// <param name="contractKeyMapping">The parameter name of the contractKey mapping</param>
        public MSClientRegisterAction(string contractKeyMapping)
        {
            _contractKey = contractKeyMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IDictionary<string, object> dic = new Dictionary<string, object>();
            string key = "";
            List<string> list = new List<string>() { _contractKey };
            if (0 == dic.Count || string.IsNullOrEmpty(key))
            {
                dic = GetKVListFromHeader(context.HttpContext, list, false);
                key = GetVal(dic);
            }

            if (0 == dic.Count || string.IsNullOrEmpty(key))
            {
                dic = GetKVListFromQuery(context.HttpContext, list, false);
                key = GetVal(dic);
            }

            if (0 == dic.Count || string.IsNullOrEmpty(key))
            {
                dic = GetKVListFromBody(context.HttpContext, list, false);
                key = GetVal(dic);
            }

            if (0 == dic.Count || string.IsNullOrEmpty(key))
            {
                dic = GetKVListFromForm(context.HttpContext, list, false);
                key = GetVal(dic);
            }

            if (string.IsNullOrEmpty(key)) throw new Exception("The parameter '" + MServiceConst.contractKey + "' is not empty.");
            string ip = GetIP(context.HttpContext);
            bool mbool = _mSService.SaveIPAddr(ip, key);
            string msg = "{0} enroll {1}.".ExtFormat(ip, (mbool ? "successfully" : "failly"));
            PrintIpToLogs(msg);
            if (mbool) SetIpToDic(ip);
            base.OnActionExecuting(context);
        }
    }
}
