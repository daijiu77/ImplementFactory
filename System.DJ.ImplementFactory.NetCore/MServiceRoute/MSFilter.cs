using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSFilter : ActionFilterAttribute
    {
        private static Dictionary<string, string> _kvDic = new Dictionary<string, string>();

        private static IMSService _mSService = null;

        public MSFilter()
        {
            //
        }

        public static void SetMSServiceInstance(IMSService mSService)
        {
            _mSService = mSService;
            List<string> ips = _mSService.IPAddrSources();
            foreach (var item in ips)
            {
                _kvDic[item] = item;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {            
            string ip = GetIP(context);
            if(!_kvDic.ContainsKey(ip))
            {
                throw new Exception("Illegal access");
            }
            base.OnActionExecuting(context);
        }

        private string GetIP(ActionExecutingContext context)
        {
            // ip
            string ip = "";
            string type = "";

            // context 是 从过滤器拿的ActionExecutingContext 
            try
            {
                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                    type = "X-Real-IP";
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                    type = "X-Forwarded-For";
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
                    type = "RemoteIp";
                }

                // 判断是否多个ip
                if (ip.IndexOf(",") != -1)
                {
                    //有“,”，估计多个代理。取最后一个的IP。  
                    string[] temparyip = ip.Split(",");
                    ip = temparyip[temparyip.Length - 1].Trim();
                }

                // 去除::ffff:
                if (ip.IndexOf("::ffff:") != -1)
                {
                    ip = ip.Substring(7);
                }

                if (string.IsNullOrEmpty(ip))
                {
                    ip = "NoGet";
                    type = "NoGet";
                }
            }
            catch
            {
                ip = "NoGet";
                type = "NoGet";
            }
            string dIP = "127.0.0.1";
            if (null == ip) ip = dIP;
            if (ip.Equals("::1")) ip = dIP;
            return ip;
        }
    }
}
