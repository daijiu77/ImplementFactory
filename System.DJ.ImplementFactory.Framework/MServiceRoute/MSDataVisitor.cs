using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;

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

        public string GetResult(string routeName, string uri, string controller, string action, object data)
        {
            string result = null;
            if (null == httpHelper) return result;
            string[] arr = uri.Split(',');
            string key = routeName + "-" + controller + "-" + action;
            int index = GetIndex(key, arr.Length);
            string addr = string.Format("{0}/{1}/{2}", arr[index], controller, action);
            httpHelper.SendData(addr, null, data, true, (resultData, err) =>
            {
                if (string.IsNullOrEmpty(err) && null != resultData) result = resultData.ToString();
            });
            return result;
        }
    }
}
