using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public abstract class AbsActionFilterAttribute : ActionFilterAttribute
    {
        protected static Dictionary<string, string> _ipDic = new Dictionary<string, string>();

        protected static IMSService _mSService = null;

        private static object _AbsActionFilterAttributeLock = new object();

        public static void SetMSServiceInstance(IMSService mSService)
        {
            lock (_AbsActionFilterAttributeLock)
            {
                _mSService = mSService;
                List<string> ips = _mSService.IPAddrSources();
                if (null == ips) return;
                foreach (var item in ips)
                {
                    _ipDic[item] = item;
                }
            }
        }

        protected Dictionary<string, object> GetKVListFromBody(HttpContext context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = null;
            string txt = "";
            MemoryStream ms = new MemoryStream();
            try
            {
                Stream stream = context.Request.Body;
                byte[] buffer = new byte[1024];
                int size = 0;
                while (0 < (size = stream.ReadAsync(buffer, 0, buffer.Length).Result))
                {
                    ms.Write(buffer, 0, size);
                }
                txt = Encoding.UTF8.GetString(ms.ToArray());
                context.Request.Body = ms;
            }
            catch (Exception)
            {
                //throw;
            }
            finally
            {
                //ms.Dispose();
            }

            dic = getDic(txt, keys, contain);
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromForm(HttpContext context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                IFormCollection forms = context.Request.Form;
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in forms)
                {
                    kn = item.Key.ToLower();
                    kv = item.Value;
                    mbool = false;
                    foreach (var k in keys)
                    {
                        if (contain)
                        {
                            if (-1 != kn.IndexOf(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                        else
                        {
                            if (kn.Equals(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                    }
                    if (mbool) dic[kn] = kv;
                }
            }
            catch (Exception)
            {

                //throw;
            }
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromQuery(HttpContext context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                IQueryCollection querys = context.Request.Query;
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in querys)
                {
                    kn = item.Key.ToLower();
                    kv = item.Value;
                    mbool = false;
                    foreach (var k in keys)
                    {
                        if (contain)
                        {
                            if (-1 != kn.IndexOf(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                        else
                        {
                            if (kn.Equals(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                    }
                    if (mbool) dic[kn] = kv;
                }
            }
            catch (Exception)
            {

                //throw;
            }
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromHeader(HttpContext context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                IHeaderDictionary heads = context.Request.Headers;
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in heads)
                {
                    kn = item.Key.ToLower();
                    kv = item.Value.ToString();
                    mbool = false;
                    foreach (var k in keys)
                    {
                        if (contain)
                        {
                            if (-1 != kn.IndexOf(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                        else
                        {
                            if (kn.Equals(k.ToLower()))
                            {
                                mbool = true;
                                break;
                            }
                        }
                    }
                    if (mbool) dic[kn] = kv;
                }
            }
            catch (Exception)
            {

                //throw;
            }
            return dic;
        }

        private Dictionary<string, object> getDic(string txt, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(txt) || null == keys) return dic;
            string s = "";
            foreach (var item in keys)
            {
                if (contain)
                {
                    s += @"|([a-z0-9_]*" + item + @"[a-z0-9_]*)";
                }
                else
                {
                    s += @"|(" + item + @")";
                }
            }
            if (string.IsNullOrEmpty(s)) return dic;
            s = s.Substring(1);
            s = @"""?(?<FKey>" + s + @")""?\s*\:\s*""?(?<FValue>((?!"").)+)""?";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            if (!rg.IsMatch(txt)) return dic;
            MatchCollection mc = rg.Matches(txt);
            string FKey = "";
            string FValue = "";
            foreach (Match item in mc)
            {
                FKey = item.Groups["FKey"].Value.ToLower();
                FValue = item.Groups["FValue"].Value;
                dic[FKey] = FValue;
            }
            return dic;
        }

        protected string GetIP(HttpContext context)
        {
            // ip
            string ip = "";

            // context 是 从过滤器拿的ActionExecutingContext 
            try
            {
                const string _XRealIP = "X-Real-IP";
                const string _XForwardedFor = "X-Forwarded-For";

                if (context.Request.Headers.ContainsKey(_XRealIP))
                {
                    ip = context.Request.Headers[_XRealIP].FirstOrDefault();
                }

                if (context.Request.Headers.ContainsKey(_XForwardedFor))
                {
                    ip = context.Request.Headers[_XForwardedFor].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.Connection.RemoteIpAddress.ToString();
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
            }
            catch
            {
                //ip = "NoGet";
            }
            string dIP = "127.0.0.1";
            if (string.IsNullOrEmpty(ip)) ip = dIP;
            if (ip.Equals("::1")) ip = dIP;
            return ip;
        }

        protected string GetVal(IDictionary<string, object> dic)
        {
            string val = "";
            if (null == dic) return val;
            if (0 == dic.Count) return val;
            string[] keys = dic.Keys.ToArray();
            if (null != dic[keys[0]]) val = dic[keys[0]].ToString();
            return val;
        }

        /// <summary>
        /// 遍历当前类后缀为 Mapping 的所有字段
        /// </summary>
        /// <param name="action">string: 原字段名称, string: 去除后缀 Mapping 的字段名, object: 字段值</param>
        protected void ForeachFields(Type type, Action<string, string, object> action)
        {
            Type meType = type;
            const string mapping = "mapping";
            int len = mapping.Length;
            string nameLower = "";
            object vObj = null;
            string fv = "";
            string fn = "";
            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.DeclaringType != meType) continue;
                nameLower = fieldInfo.Name.ToLower();
                if (len >= nameLower.Length) continue;
                if (!nameLower.Substring(nameLower.Length - len).Equals(mapping)) continue;
                vObj = fieldInfo.GetValue(this);
                fv = "";
                if (null != vObj) fv = vObj.ToString().Trim();
                if (string.IsNullOrEmpty(fv)) continue;
                fn = nameLower.Substring(0, nameLower.Length - len);
                action(fieldInfo.Name, fn, fv);
            }
        }

        /// <summary>
        /// 遍历当前类后缀为 Mapping 的所有字段
        /// </summary>
        /// <typeparam name="T">当前类</typeparam>
        /// <param name="action">string: 原字段名称, string: 去除后缀 Mapping 的字段名, object: 字段值</param>
        protected void ForeachFields<T>(Action<string, string, object> action)
        {
            Type type = typeof(T);
            ForeachFields(type, action);
        }

        protected static void SetIpToDic(string ip)
        {
            lock (_AbsActionFilterAttributeLock)
            {
                if (_ipDic.ContainsKey(ip)) return;
                _ipDic.Add(ip, ip);
            }
        }

        protected static bool IsExistIP(string ip)
        {
            lock (_AbsActionFilterAttributeLock)
            {
                return _ipDic.ContainsKey(ip);
            }
        }

        protected MethodInfo GetActionMethod(FilterContext context)
        {
            MethodInfo mi = null;
            Type controllerType = null;
            ActionDescriptor actionObj = null;
            if (null != (context as ActionExecutedContext))
            {
                controllerType = ((ActionExecutedContext)context).Controller.GetType();
                actionObj = ((ActionExecutedContext)context).ActionDescriptor;
            }
            else if (null != (context as ActionExecutingContext))
            {
                controllerType = ((ActionExecutingContext)context).Controller.GetType();
                actionObj = ((ActionExecutingContext)context).ActionDescriptor;
            }
            if (null == controllerType) return mi;

            string actionName = "";
            PropertyInfo pi = actionObj.GetType().GetProperty("ActionName");
            if (null != pi)
            {
                object piVal = pi.GetValue(actionObj);
                if (null != piVal) actionName = piVal.ToString();
            }

            string actionName1 = "";
            string s = actionObj.DisplayName;
            Regex rgAN = new Regex(@"(?<ActionName>[a-z0-9_]+)\s*\(", RegexOptions.IgnoreCase);
            if (rgAN.IsMatch(s))
            {
                actionName1 = rgAN.Match(s).Groups["ActionName"].Value;
            }

            if (!string.IsNullOrEmpty(actionName1))
            {
                mi = controllerType.GetMethod(actionName1);
            }

            if ((false == string.IsNullOrEmpty(actionName)) && (null == mi))
            {
                mi = controllerType.GetMethod(actionName);
            }
            return mi;
        }

        private static object _PrintIpToLogsLock = new object();
        private static Task ipLogTask = null;
        private static List<string> ipPools1 = new List<string>();
        private static List<string> ipPools2 = new List<string>();
        private static int collection_index = 0;
        public static void PrintIpToLogs(string ip)
        {
            lock (_PrintIpToLogsLock)
            {
                if (!ImplementAdapter.dbInfo1.IsPrintFilterIPToLogs) return;
                SetIpToList(ip);

                if (null != ipLogTask) return;
                ipLogTask = Task.Run(() =>
                {
                    List<string> ips = null;
                    while (true)
                    {
                        ips = SetIpToList(null);
                        SaveIpToLogs(ips);
                        Thread.Sleep(1000);
                    }
                });
            }
        }

        private static List<string> SetIpToList(string ip)
        {
            lock (_PrintIpToLogsLock)
            {
                if (!string.IsNullOrEmpty(ip))
                {
                    if (0 == collection_index)
                    {
                        ipPools1.Add(ip);
                    }
                    else
                    {
                        ipPools2.Add(ip);
                    }
                    return null;
                }

                if (0 == collection_index)
                {
                    collection_index = 1;
                    return ipPools1;
                }
                else
                {
                    collection_index = 0;
                    return ipPools2;
                }
            }
        }

        private static void SaveIpToLogs(List<string> ips)
        {
            lock (_PrintIpToLogsLock)
            {
                if (0 == ips.Count) return;
                DateTime dt = DateTime.Now;
                string fPath = Path.Combine(DJTools.RootPath, AutoCall.LogsDir);
                DJTools.InitDirectory(fPath, true);
                string fName = "IP-List-" + dt.ToString("yyyyMMddHH") + ".txt";
                fPath = Path.Combine(fPath, fName);

                string txt = "";
                foreach (string ip in ips)
                {
                    txt = "{0}\t{1}\r\n".ExtFormat(dt.ToString("yyyy-MM-dd HH:mm:ss"), ip);
                    File.AppendAllText(fPath, txt);
                }
                ips.Clear();
            }
        }
    }
}
