using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Web;
using System.Web.Mvc;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public abstract class AbsActionFilterAttribute : ActionFilterAttribute
    {
        protected static Dictionary<string, string> _kvDic = new Dictionary<string, string>();

        protected static IMSService _mSService = null;

        public static void SetMSServiceInstance(IMSService mSService)
        {
            _mSService = mSService;
            List<string> ips = _mSService.IPAddrSources();
            if (null == ips) return;
            foreach (var item in ips)
            {
                _kvDic[item] = item;
            }
        }

        protected string GetIP(ActionExecutingContext context)
        {
            // ip
            string ip = "";

            // context 是 从过滤器拿的ActionExecutingContext 
            try
            {
                HttpRequestBase request = context.HttpContext.Request;
                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }

                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.UserHostAddress;
                }

                // 判断是否多个ip
                if (ip.IndexOf(",") != -1)
                {
                    //有“,”，估计多个代理。取最后一个的IP。  
                    string[] temparyip = ip.Split(',');
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
                }
            }
            catch
            {
                ip = "NoGet";
            }
            string dIP = "127.0.0.1";
            if (null == ip) ip = dIP;
            if (ip.Equals("::1")) ip = dIP;
            return ip;
        }
    }
}
