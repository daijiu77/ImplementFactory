using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSDataVisitor : ImplementAdapter
    {
        [AutoCall]
        private IHttpHelper httpHelper;

        private static Dictionary<string, int> map = new Dictionary<string, int>();

        private static object _thObj = new object();
        private static int GetIndex(string key, int baseNum)
        {
            lock (_thObj)
            {
                int index = 0;
                map.TryGetValue(key, out index);
                index++;
                index = index % baseNum;
                map[key] = index;
                return index;
            }
        }

        private string InitActionName(string action, object data)
        {
            string actionName = action;
            Regex rg = new Regex(@"\{(?<PName>[a-z0-9_]+)\}", RegexOptions.IgnoreCase);
            if (rg.IsMatch(actionName) && null != data)
            {
                MatchCollection mc = rg.Matches(actionName);
                string PName = "";
                string pv = "";
                foreach (Match m in mc)
                {
                    PName = m.Groups["PName"].Value;
                    pv = GetParaValFromData(PName, data);
                    if (null != pv)
                    {
                        actionName = actionName.Replace(m.Groups[0].Value, pv);
                    }
                }
            }
            return actionName;
        }

        private string GetParaValFromData(string paraName, object data)
        {
            string paraVal = null;
            if (data.GetType().IsBaseType()) return paraVal;
            string pn = paraName.ToLower();
            data.ForeachProperty((pi, pt, fn, fv) =>
            {
                if (null != paraVal) return false;
                if (!pt.IsBaseType())
                {
                    paraVal = GetParaValFromData(paraName, fv);
                    return true;
                }
                if (!pn.Equals(fn.ToLower())) return true;
                if (null == fv) return false;
                paraVal = fv.ToString();
                return false;
            });
            return paraVal;
        }

        private string InitUri(string url)
        {
            string s = url.Trim();
            s = s.Replace("\\", "/");
            if (s.Substring(0, 1).Equals("/")) s = s.Substring(1);
            if (s.Substring(s.Length - 1).Equals("/")) s = s.Substring(0, s.Length - 1);
            return s;
        }

        public string GetResult(string routeName, string uri, string controller, string action, MethodTypes methodTypes, object data)
        {
            string result = null;
            if (null == httpHelper) return result;
            string actionName = InitActionName(action, data);
            controller = InitUri(controller);
            actionName = InitUri(actionName);
            string[] arr = uri.Split(',');
            string key = routeName + "-" + controller + "-" + action;
            int index = GetIndex(key, arr.Length);            
            string url = InitUri(arr[index]);           
            string addr = string.Format("{0}/{1}/{2}", url, controller, actionName);
            httpHelper.SendData(addr, null, data, true, methodTypes, (resultData, err) =>
            {
                if (string.IsNullOrEmpty(err) && null != resultData) result = resultData.ToString();
            });
            return result;
        }
    }
}
