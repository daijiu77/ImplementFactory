using System.Web.Mvc;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
    /// </summary>
    public class MSFilter : AbsActionFilterAttribute
    {
        /// <summary>
        /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
        /// </summary>
        public MSFilter()
        {
            //
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool mbool = false;
            Type type = context.Controller.GetType();
            string displayName = context.ActionDescriptor.ActionName;
            Regex rg = new Regex(@"controller\.(?<MethodName>[a-z0-9_]+)\s*\(", RegexOptions.IgnoreCase);
            string MethodName = "";
            if (rg.IsMatch(displayName))
            {
                MethodName = rg.Match(displayName).Groups["MethodName"].Value;
            }
            else
            {
                MethodName = displayName;
            }
            MethodInfo mi = type.GetMethod(MethodName);
            if (null != mi)
            {
                Attribute atr1 = mi.GetCustomAttribute(typeof(MSClientRegisterAction), true);
                Attribute atr2 = mi.GetCustomAttribute(typeof(MSConfiguratorAction), true);
                Attribute atr3 = mi.GetCustomAttribute(typeof(MSUnlimitedAction), true);
                if (null != atr1 || null != atr2 || null != atr3) mbool = true;
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
