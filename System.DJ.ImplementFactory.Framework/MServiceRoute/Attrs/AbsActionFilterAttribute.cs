﻿using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.DJ.ImplementFactory.Commons;

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

        protected Dictionary<string, object> GetKVListFromBody(HttpContextBase context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = null;
            string txt = "";
            MemoryStream ms = new MemoryStream();
            try
            {
                Stream stream = context.Request.InputStream;
                byte[] buffer = new byte[1024];
                int size = 0;
                while (0 < (size = stream.ReadAsync(buffer, 0, buffer.Length).Result))
                {
                    ms.Write(buffer, 0, size);
                }
                txt = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception)
            {
                //throw;
            }
            finally
            {
                ms.Dispose();
            }

            dic = getDic(txt, keys, contain);
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromForm(HttpContextBase context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                NameValueCollection forms = context.Request.Form;
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in forms.AllKeys)
                {
                    kn = item.ToLower();
                    kv = forms.Get(item);
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
                    if (mbool) dic[item] = kv;
                }
            }
            catch (Exception)
            {

                //throw;
            }
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromQuery(HttpContextBase context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                NameValueCollection querys = context.Request.QueryString;
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in querys.AllKeys)
                {
                    kn = item.ToLower();
                    kv = querys.Get(item);
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
                    if (mbool) dic[item] = kv;
                }
            }
            catch (Exception)
            {

                //throw;
            }
            return dic;
        }

        protected Dictionary<string, object> GetKVListFromHead(HttpContextBase context, List<string> keys, bool contain)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (null == keys) keys = new List<string>();
            try
            {
                NameValueCollection heads = context.Request.Headers;                
                string kn = "", kv = "";
                bool mbool = false;
                foreach (var item in heads.AllKeys)
                {
                    kn = item.ToLower();
                    kv = heads.Get(item);
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
                    if (mbool) dic[item] = kv;
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
                FKey = item.Groups["FKey"].Value;
                FValue = item.Groups["FValue"].Value;
                dic[FKey] = FValue;
            }
            return dic;
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