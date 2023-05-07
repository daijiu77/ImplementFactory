using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MSDataVisitor : ImplementAdapter
    {
        private IHttpHelper httpHelper;

        [AutoCall]
        private IMSAllot mSAllot;

        private const string IPAddr = "IPAddress";
        private static Regex httpRg = new Regex(@"^((http)|(https)):\/\/(?<IPAddress>[^\/\?]+)", RegexOptions.IgnoreCase);

        /// <summary>
        /// key: routeName-controller-action, value: index
        /// </summary>
        private static Dictionary<string, int> map = new Dictionary<string, int>();

        /// <summary>
        /// key: ip and port, value: indexs
        /// </summary>
        private static Dictionary<string, HttpList> timeoutUrlDic = new Dictionary<string, HttpList>();

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
                    HttpList ints = timeoutUrlDic[rtName];
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

        private bool SetTimeoutIndex(string routeName, string url, int urlSize, int index)
        {
            lock (_thObjLock)
            {
                string ipAddr = "";
                if (httpRg.IsMatch(url))
                {
                    ipAddr = httpRg.Match(url).Groups[IPAddr].Value;
                }
                HttpList ints = null;
                string rtName = routeName.ToLower();
                timeoutUrlDic.TryGetValue(rtName, out ints);
                if (null == ints)
                {
                    ints = new HttpList();
                    timeoutUrlDic.Add(rtName, ints);
                }
                ints.Add(ipAddr, index);

                bool isTimeout = urlSize > ints.Count;
                return isTimeout;
            }
        }

        /// <summary>
        /// It is illegal call.
        /// </summary>
        /// <returns></returns>
        private static bool isIllegalCall()
        {
            Type srcType = DJTools.GetSrcType<MSDataVisitor>();
            if (null == srcType) return true;
            if (srcType != typeof(ServiceRegisterMessage)) return true;
            return false;
        }

        private static void RemoveTimeoutItem(string routeName, string url)
        {
            if (!httpRg.IsMatch(url)) return;
            string rtName = routeName.ToLower();
            HttpList httpItems = null;
            timeoutUrlDic.TryGetValue(rtName, out httpItems);
            if (null == httpItems) return;
            string IPAddress = httpRg.Match(url).Groups[IPAddr].Value;
            httpItems.Remove(IPAddress);
        }

        public static void RegisterSuccess(string routeName, string url, MethodTypes methodTypes, string contractValue, string message)
        {
            lock (_thObjLock)
            {
                if (isIllegalCall()) return;
                RemoveTimeoutItem(routeName, url);
            }
        }

        public static void TestVisit(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            lock (_thObjLock)
            {
                if (isIllegalCall()) return;
                if (string.IsNullOrEmpty(err)) return;
                RemoveTimeoutItem(routeName, url);
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
                Regex rg1 = new Regex(@"[\|\,\;\@\$\*\s]", RegexOptions.IgnoreCase);
                if (rg1.IsMatch(uri))
                {
                    string sc = rg1.Match(uri).Groups[0].Value;
                    char c = sc.ToCharArray()[0];
                    string[] uriArr = uri.Split(c);
                    string txt = "";
                    List<string> list = new List<string>();
                    foreach (var item in uriArr)
                    {
                        txt = item.Trim();
                        if (string.IsNullOrEmpty(txt)) continue;
                        if (!MService.httpRg.IsMatch(txt)) continue;
                        list.Add(txt);
                    }
                    arr = list.ToArray();
                }
                else
                {
                    arr = new string[] { uri };
                }
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
            Regex badRg = new Regex(@"bad\s+gateway", RegexOptions.IgnoreCase);
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
                    string errLower = err.ToLower();
                    if ((-1 != errLower.IndexOf("timeout")) || badRg.IsMatch(errLower))
                    {
                        isTimeout = SetTimeoutIndex(routeName, httpAddr, urlSize, index);

                    }
                    if (null != mSAllot) mSAllot.HttpVisitingException(routeName, httpAddr, err);
                }

            });
            if (isTimeout) goto timeout_restart;
            return result;
        }

        class HttpItem
        {
            public int index { get; set; } = -1;
            public string IPAddr { get; set; }
        }

        class HttpList : IEnumerable<HttpItem>
        {
            private Dictionary<string, HttpItem> ipDic = new Dictionary<string, HttpItem>();
            private Dictionary<int, HttpItem> indexDic = new Dictionary<int, HttpItem>();

            private EnumerableItem enumerableItem = new EnumerableItem();

            public HttpItem this[string ipAddr]
            {
                get
                {
                    HttpItem item = null;
                    ipDic.TryGetValue(ipAddr, out item);
                    return item;
                }
            }

            public HttpItem this[int index]
            {
                get
                {
                    HttpItem item = null;
                    indexDic.TryGetValue(index, out item);
                    return item;
                }
            }

            public bool Contains(string ipAddr)
            {
                return ipDic.ContainsKey(ipAddr);
            }

            public bool Contains(int index)
            {
                return indexDic.ContainsKey(index);
            }

            public int Count
            {
                get
                {
                    int count = ipDic.Count;
                    if (count < indexDic.Count) count = indexDic.Count;
                    return count;
                }
            }

            public void Remove(string ipAddr)
            {
                if (!ipDic.ContainsKey(ipAddr)) return;
                HttpItem item = ipDic[ipAddr];
                if (-1 != item.index) indexDic.Remove(item.index);
                ipDic.Remove(ipAddr);
            }

            public void Remove(int index)
            {
                if (!indexDic.ContainsKey(index)) return;
                HttpItem item = indexDic[index];
                if (!string.IsNullOrEmpty(item.IPAddr)) ipDic.Remove(item.IPAddr);
                indexDic.Remove(index);
            }

            public void Add(string ipAddr, int index)
            {
                HttpItem item = null;
                if (!string.IsNullOrEmpty(ipAddr))
                {
                    ipDic.TryGetValue(ipAddr, out item);
                }
                else if (0 <= index)
                {
                    indexDic.TryGetValue(index, out item);
                }

                if (null == item)
                {
                    item = new HttpItem()
                    {
                        index = index,
                        IPAddr = ipAddr
                    };
                }

                if (!string.IsNullOrEmpty(ipAddr))
                {
                    ipDic[ipAddr] = item;
                }

                if (0 <= index)
                {
                    indexDic[index] = item;
                }
            }

            public void Add(string ipAddr)
            {
                Add(ipAddr, -1);
            }

            public void Add(int index)
            {
                Add(null, index);
            }

            IEnumerator<HttpItem> IEnumerable<HttpItem>.GetEnumerator()
            {
                return enumerableItem;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return enumerableItem;
            }

            class EnumerableItem : IEnumerator<HttpItem>, IEnumerator
            {
                HttpItem IEnumerator<HttpItem>.Current => throw new NotImplementedException();

                object IEnumerator.Current => throw new NotImplementedException();

                void IDisposable.Dispose()
                {
                    throw new NotImplementedException();
                }

                bool IEnumerator.MoveNext()
                {
                    throw new NotImplementedException();
                }

                void IEnumerator.Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
