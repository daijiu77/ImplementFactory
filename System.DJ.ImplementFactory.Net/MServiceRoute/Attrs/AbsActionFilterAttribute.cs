﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.ComponentModel;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Linq;
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

        public static void SetMSServiceInstance(IMSService mSService)
        {
            _mSService = mSService;
            List<string> ips = _mSService.IPAddrSources();
            if (null == ips) return;
            foreach (var item in ips)
            {
                _ipDic[item] = item;
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

        private static object _PrintIpToLogsLock = new object();
        private static Task ipLogTask = null;
        private static List<string> ipPools1 = new List<string>();
        private static List<string> ipPools2 = new List<string>();
        private static int collection_index = 0;
        protected void PrintIpToLogs(string ip)
        {
            lock (_PrintIpToLogsLock)
            {
                if (!ImplementAdapter.dbInfo1.PrintFilterIPToLogs) return;
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

        private List<string> SetIpToList(string ip)
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

        private void SaveIpToLogs(List<string> ips)
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
                txt = "{0}: {1}\r\n".ExtFormat(dt.ToString("yyyy-MM-dd HH:mm:ss"), ip);
                File.AppendText(txt);
            }
        }
    }
}
