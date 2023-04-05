using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSFilter : AbsActionFilterAttribute
    {

        public MSFilter()
        {
            //
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool mbool = false;
            Type type = context.Controller.GetType();
            string actionName = context.ActionDescriptor.DisplayName;
            MethodInfo mi = type.GetMethod(actionName);
            if (null != mi)
            {
                Attribute atr1 = mi.GetCustomAttribute(typeof(MSClientRegisterAction), true);
                Attribute atr2 = mi.GetCustomAttribute(typeof(MSConfiguratorAction), true);
                if (null != atr1 || null != atr2) mbool = true;
            }

            if(!mbool)
            {
                string ip = GetIP(context);
                if (!_kvDic.ContainsKey(ip))
                {
                    throw new Exception("Illegal access");
                }
            }
            
            base.OnActionExecuting(context);
        }

    }
}
