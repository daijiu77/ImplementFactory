using System.Web.Mvc;
using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSClientRegisterAction : AbsActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IDictionary<string, object> dic = context.ActionParameters;
            string key = "";
            string contractKey = MSServiceImpl.contractKey.ToLower();
            foreach (var item in dic)
            {
                if (item.Key.ToLower().Equals(contractKey))
                {
                    key = item.Value.ToString();
                    break;
                }
            }

            List<string> list = new List<string>() { contractKey };
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
                dic = GetKVListFromHead(context.HttpContext, list, false);
            }

            if (string.IsNullOrEmpty(key))
            {
                foreach (var item in dic)
                {
                    key = item.Value.ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(key)) throw new Exception("The parameter '"+ MSServiceImpl.contractKey + "' is not empty.");
            string ip = GetIP(context);
            _mSService.SaveIPAddr(ip, key);
            base.OnActionExecuting(context);
        }
    }
}