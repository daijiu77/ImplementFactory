using System.Web.Mvc;
using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Identify the client registration method, which must have a 'ContractKey' parameter of character type
    /// </summary>
    public class MSClientRegisterAction : AbsActionFilterAttribute
    {
        private string _contractKey = MSServiceImpl.contractKey.ToLower();
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
            IDictionary<string, object> dic = context.ActionParameters;
            string key = "";            
            foreach (var item in dic)
            {
                if (item.Key.ToLower().Equals(_contractKey))
                {
                    if (null != item.Value) key = item.Value.ToString();
                    break;
                }
            }

            List<string> list = new List<string>() { _contractKey };
            if (0 == dic.Count)
            {
                dic = GetKVListFromBody(context.HttpContext, list, false);
            }

            if (0 == dic.Count)
            {
                dic = GetKVListFromForm(context.HttpContext, list, false);
            }

            if (0 == dic.Count)
            {
                dic = GetKVListFromHeader(context.HttpContext, list, false);
            }

            if (string.IsNullOrEmpty(key))
            {
                foreach (var item in dic)
                {
                    if (null != item.Value) key = item.Value.ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(key)) throw new Exception("The parameter '" + MSServiceImpl.contractKey + "' is not empty.");
            string ip = GetIP(context.HttpContext);
            bool mbool = _mSService.SaveIPAddr(ip, key);
            if (mbool) _kvDic[ip] = ip;
            base.OnActionExecuting(context);
        }
    }
}
