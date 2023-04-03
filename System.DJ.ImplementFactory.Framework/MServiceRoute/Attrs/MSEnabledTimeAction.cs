using System.Web.Mvc;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSEnabledTimeAction : AbsActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            IDictionary<string, object> dicPara = context.ActionParameters;
            List<string> list = new List<string>() { "start", "end", "key" };
            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromBody(context.HttpContext, list, true);
            }

            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromForm(context.HttpContext, list, true);
            }

            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromHead(context.HttpContext, list, true);
            }
            string start = "";
            string end = "";
            string key = "";
            foreach (var item in dicPara)
            {
                if (-1 != item.Key.ToLower().IndexOf("start"))
                {
                    if (null != item.Value) start = item.Value.ToString();
                }
                else if (-1 != item.Key.ToLower().IndexOf("end"))
                {
                    if (null != item.Value) end = item.Value.ToString();
                }
                else if (-1 != item.Key.ToLower().IndexOf("key"))
                {
                    if (null != item.Value) key = item.Value.ToString();
                }
            }

            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end) || string.IsNullOrEmpty(key))
            {
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
                MethodName = "(" + MethodName + ") ";
                string err = "The method {0}parameter must contain and set a valid start time(startTime) and end time(endTime) and contract key(contractKey).".ExtFormat(MethodName);
                throw new Exception(err);
            }

            DateTime startTime = DateTime.Now;
            DateTime.TryParse(start, out startTime);

            DateTime endTime = DateTime.Now;
            DateTime.TryParse(end, out endTime);

            _mSService.SetEnabledTime(startTime, endTime, key);
        }
    }
}
