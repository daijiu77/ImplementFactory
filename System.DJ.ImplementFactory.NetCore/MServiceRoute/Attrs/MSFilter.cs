using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
    /// </summary>
    public class MSFilter : AbsActionFilterAttribute
    {
        private static string tokenKeyName = "token";

        private static Dictionary<string, string> tokenKV = new Dictionary<string, string>();
        private static MSFilter mSFilter = new MSFilter();
        /// <summary>
        /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
        /// </summary>
        public MSFilter()
        {
            //
        }

        /// <summary>
        /// Set up token filtering (ignore IP registration)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenKeyName">Set the keyName of the key-value pair Token in the parameters of the front-end HTTP request</param>
        /// <param name="token">The value of the token is set in the background for gateway filtering</param>
        public static void SetToken(HttpContext context, string tokenKeyName, string token)
        {
            lock (mSFilter)
            {
                MSFilter.tokenKeyName = tokenKeyName;
                string ip = mSFilter.GetIP(context);
                tokenKV[token] = ip;
            }
        }

        private string GetIP_Token(string token)
        {
            lock (mSFilter)
            {
                string ip = "";
                if (tokenKV.ContainsKey(token))
                {
                    ip = tokenKV[token];
                }
                return ip;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool mbool = false;
            Type type = context.Controller.GetType();
            Attribute attr = type.GetCustomAttribute(typeof(MSUnlimited), true);
            if (null != attr)
            {
                base.OnActionExecuting(context);
                return;
            }
            string displayName = context.ActionDescriptor.DisplayName;
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
                Attribute atr3 = mi.GetCustomAttribute(typeof(MSUnlimited), true);
                if (null != atr1 || null != atr2 || null != atr3) mbool = true;
            }

            if (!mbool)
            {
                List<string> keys = new List<string>() { tokenKeyName };
                Dictionary<string, object> kvDic = GetKVListFromHeader(context.HttpContext, keys, false);
                if (0 == kvDic.Count)
                {
                    kvDic = GetKVListFromQuery(context.HttpContext, keys, false);
                }

                if (0 < kvDic.Count)
                {
                    string token = kvDic[tokenKeyName.ToLower()].ToString();
                    string ip1 = GetIP_Token(token);
                    string ip2 = GetIP(context.HttpContext);
                    mbool = ip2.Equals(ip1);
                }
            }

            if (!mbool)
            {
                string ip = GetIP(context.HttpContext);
                if (!_kvDic.ContainsKey(ip))
                {
                    throw new Exception("Illegal access");
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
