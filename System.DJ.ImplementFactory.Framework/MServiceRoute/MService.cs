using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MService
    {
        /// <summary>
        /// Start the service registration mechanism, which should be executed at project startup.
        /// </summary>
        public static void Start()
        {
            Task.Run(() =>
            {
                IHttpHelper httpHelper = new HttpHelper();
                MethodTypes methodTypes = MethodTypes.Get;
                string[] uris = null;
                char c = ' ';
                string s = "", s1 = "";
                string url = "";
                string registerAddr = "";
                Regex rg = new Regex(@"[^a-z0-9_\:\/\.]", RegexOptions.IgnoreCase);
                Regex rg1 = new Regex(@"^((http)|(https))\:\/\/", RegexOptions.IgnoreCase);
                MicroServiceRoute.ForEach(routeAttr =>
                {
                    s = routeAttr.Uri.Trim();
                    if (string.IsNullOrEmpty(s)) return;

                    registerAddr = routeAttr.RegisterAddr.Trim();
                    if (string.IsNullOrEmpty(registerAddr)) return;
                    if (registerAddr.Substring(0, 1).Equals("/"))
                    {
                        registerAddr = registerAddr.Substring(1);
                    }

                    if (rg.IsMatch(s))
                    {
                        s1 = rg.Match(s).Groups[0].Value;
                        c = s1.ToArray()[0];
                        uris = s.Split(c);
                    }
                    else
                    {
                        uris = new string[] { s };
                    }

                    methodTypes = routeAttr.RegisterActionType;
                    foreach (string item in uris)
                    {
                        url = item.Trim();
                        if (string.IsNullOrEmpty(url)) continue;
                        if (!rg1.IsMatch(url)) continue;
                        if (url.Substring(url.Length - 1).Equals("/"))
                        {
                            url = url.Substring(0, url.Length - 1);
                        }
                        url += "/" + registerAddr;
                        httpHelper.SendData(url, null, null, true, methodTypes, (result, msg) => { });
                    }
                });
            });
        }
    }
}
