using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSClientRegisterAction : AbsActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IDictionary<string, object> dic = context.ActionArguments;
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
            string ip = GetIP(context);
            _mSService.SaveIPAddr(ip, key);
            base.OnActionExecuting(context);
        }
    }
}
