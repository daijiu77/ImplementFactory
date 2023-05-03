using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSDataVisitor : ImplementAdapter
    {
        private IHttpHelper httpHelper;

        [AutoCall]
        private IMSAllot mSAllot;

        /// <summary>
        /// key: routeName-controller-action, value: index
        /// </summary>
        private static Dictionary<string, int> map = new Dictionary<string, int>();

        /// <summary>
        /// key: routeName_lower, value: indexs
        /// </summary>
        private static Dictionary<string, List<int>> timeoutUrlDic = new Dictionary<string, List<int>>();

        private static object _thObjLock = new object();

        public MSDataVisitor()
        {
            httpHelper = new HttpHelper();
        }

        private int GetIndex(string routeName, string key, int baseNum)
        {
            lock (_thObjLock)
            {
                string rtName = routeName.ToLower();
                int index = 0;
            resetIndex:
                index = 0;
                map.TryGetValue(key, out index);
                index++;
                index = index % baseNum;
                map[key] = index;
                if (timeoutUrlDic.ContainsKey(rtName))
                {
                    List<int> ints = timeoutUrlDic[rtName];
                    if (ints.Count >= baseNum)
                    {
                        index = -1;
                    }
                    else if (ints.Contains(index))
                    {
                        goto resetIndex;
                    }
                }
                return index;
            }
        }

        private bool SetTimeoutIndex(string routeName, int urlSize, int index)
        {
            lock (_thObjLock)
            {
                List<int> ints = null;
                string routeNameLower = routeName.ToLower();
                timeoutUrlDic.TryGetValue(routeNameLower, out ints);
                if (null == ints)
                {
                    ints = new List<int>();
                    timeoutUrlDic.Add(routeNameLower, ints);
                }
                ints.Add(index);

                bool isTimeout = urlSize > ints.Count;
                return isTimeout;
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
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Replace("\\", "/");
            if (s.Substring(0, 1).Equals("/")) s = s.Substring(1);
            if (s.Substring(s.Length - 1).Equals("/")) s = s.Substring(0, s.Length - 1);
            return s;
        }

        public string GetResult(string routeName, string uri, string controller, string action, string contractKey, MethodTypes methodTypes, object data)
        {
            string result = null;
            if (null == httpHelper) return result;
            string actionName = InitActionName(action, data);
            controller = InitUri(controller);
            actionName = InitUri(actionName);

            string[] arr = null;
            if (null != mSAllot)
            {
                string[] uri1 = mSAllot.UrlCollection(routeName);
                if (null != uri1)
                {
                    if (0 < uri1.Length)
                    {
                        int size = uri1.Length;
                        arr = new string[size];
                        for (int i = 0; i < size; i++)
                        {
                            arr[i] = uri1[i];
                        }
                    }
                }
            }

            if (null == arr)
            {
                arr = uri.Split(',');
            }

            Dictionary<string, string> headers = null;
            string paras = "";
            if (null != mSAllot)
            {
                string action1 = actionName;
                if (-1 != action1.IndexOf("?")) action1 = action1.Substring(0, action1.IndexOf("?"));
                Dictionary<string, string> parameters = mSAllot.HttpParameters(routeName, controller, action1);
                if (null != parameters)
                {
                    foreach (var item in parameters)
                    {
                        paras += "&" + item.Key + "=" + item.Value;
                    }

                    if (0 < paras.Length)
                    {
                        paras = paras.Substring(1);
                        if (-1 != actionName.IndexOf("?"))
                        {
                            paras = "&" + paras;
                        }
                        else
                        {
                            paras = "?" + paras;
                        }
                    }
                }
                headers = mSAllot.HttpHeaders(routeName, controller, action1);
            }

            if (null == headers) headers = new Dictionary<string, string>();
            headers[MServiceConst.contractKey] = contractKey;

            string key = routeName + "-" + controller + "-" + action;
            int index = 0;
            string url = "";
            string http1 = "";
            string httpAddr = "";
            bool isTimeout = false;
            int urlSize = arr.Length;
        timeout_restart:
            isTimeout = false;
            index = GetIndex(routeName, key, urlSize);
            if (0 > index)
            {
                string errMsg = "The service is unavailable";
                if (1 < arr.Length) errMsg = "The service cluster is not reachable";
                if (null != mSAllot) mSAllot.HttpVisitingException(routeName, string.Join(';', arr), errMsg);
                return result;
            }
            url = InitUri(arr[index]);
            http1 = "{0}/{1}/{2}".ExtFormat(url, controller, actionName);
            httpAddr = http1 + paras;
            httpHelper.SendData(httpAddr, headers, data, true, methodTypes, (resultData, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    if (null == resultData) resultData = "";
                    result = resultData.ToString();
                }
                else
                {
                    if (-1 != err.ToLower().IndexOf("timeout"))
                    {
                        isTimeout = SetTimeoutIndex(routeName, urlSize, index);
                    }
                    if (null != mSAllot) mSAllot.HttpVisitingException(routeName, httpAddr, err);
                }

            });
            if (isTimeout) goto timeout_restart;
            return result;
        }

    }
}
