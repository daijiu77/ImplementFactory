﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Controllers;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
    /// </summary>
    public class MSFilter : AbsActionFilterAttribute
    {
        private static string tokenKeyName = "token";

        private static Dictionary<string, TokenObj> tokenKV = new Dictionary<string, TokenObj>();
        /// <summary>
        /// key: clientIP@contractKey, value: svrKey
        /// </summary>
        private static Dictionary<string, string> svrKeyDic = new Dictionary<string, string>();
        private static MSFilter mSFilter = new MSFilter();

        static MSFilter()
        {
            Task.Run(() =>
            {
                const int sleepNum = 5000;
                while (true)
                {
                    ConfirmTokenEnabled();
                    Thread.Sleep(sleepNum);
                }
            });
        }

        /// <summary>
        /// The service gateway filter requires the use of the 'MSClientRegisterAction' attribute to specify the interface method used to accept client registration
        /// </summary>
        public MSFilter()
        {
            //
        }

        private static void ConfirmTokenEnabled()
        {
            lock (mSFilter)
            {
                DateTime dt = DateTime.Now;
                List<TokenObj> tokens = new List<TokenObj>();
                foreach (var item in tokenKV)
                {
                    //Trace.WriteLine("Start: {0}, End: {1}".ExtFormat(item.Value.startTime.ToString("yyyy-MM-dd HH:mm:ss"), item.Value.endTime.ToString("yyyy-MM-dd HH:mm:ss")));
                    if (item.Value.endTime <= dt)
                    {
                        tokens.Add(item.Value);
                    }
                }

                foreach (var item in tokens)
                {
                    if (null != ImplementAdapter.mSFilterMessage)
                    {
                        try
                        {
                            ImplementAdapter.mSFilterMessage.TokenKilled(item.token, item.ip);
                        }
                        catch (Exception)
                        {
                            //throw;
                        }
                    }
                    PrintIpToLogs("The ip: {0} and the token: {1} has been removed when it was time out.".ExtFormat(item.ip, item.token));
                    tokenKV.Remove(item.token);
                }
                tokens.Clear();
            }
        }

        private static string LegalSvrKey(string clientIP, string contractKey, string svrKey, bool isSave)
        {
            lock (mSFilter)
            {
                string svr_key = "";
                if (string.IsNullOrEmpty(clientIP)
                    || string.IsNullOrEmpty(contractKey)
                    || string.IsNullOrEmpty(svrKey)) return svr_key;

                string key = clientIP + MSConst.keySplit + contractKey;                
                svrKeyDic.TryGetValue(key, out svr_key);
                if (!svrKey.Equals(svr_key))
                {
                    svr_key = "";
                }

                if (isSave)
                {
                    if (string.IsNullOrEmpty(svr_key) && (false == string.IsNullOrEmpty(svrKey)))
                    {
                        svrKeyDic[key] = svrKey;
                    }
                }
                
                return svr_key;
            }
        }

        /// <summary>
        /// Set up token filtering (ignore IP registration)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenKeyName">Set the keyName of the key-value pair Token in the parameters of the front-end HTTP request</param>
        /// <param name="token">The value of the token is set in the background for gateway filtering</param>
        /// <param name="liveCycle_Second">Set the lifetime of the token in seconds, and the default value is 3600 seconds, that is: 1 hour</param>
        public static void SetToken(HttpContext context, string tokenKeyName, string token, int liveCycle_Second)
        {
            lock (mSFilter)
            {
                MSFilter.tokenKeyName = tokenKeyName;
                string ip = MSFilter.GetIP(context);
                SetToken(ip, tokenKeyName, token, liveCycle_Second);
            }
        }

        /// <summary>
        /// Set up token filtering (ignore IP registration)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenKeyName">Set the keyName of the key-value pair Token in the parameters of the front-end HTTP request</param>
        /// <param name="token">The value of the token is set in the background for gateway filtering</param>
        public static void SetToken(HttpContext context, string tokenKeyName, string token)
        {
            lock (mSFilter)
            {
                SetToken(context, tokenKeyName, token, 0);
            }
        }

        /// <summary>
        /// Set up token filtering (ignore IP registration)
        /// </summary>
        /// <param name="clientIP">Client ip address</param>
        /// <param name="tokenKeyName">Set the keyName of the key-value pair Token in the parameters of the front-end HTTP request</param>
        /// <param name="token">The value of the token is set in the background for gateway filtering</param>
        /// <param name="liveCycle_Second">Set the lifetime of the token in seconds, and the default value is 3600 seconds, that is: 1 hour</param>
        public static void SetToken(string clientIP, string tokenKeyName, string token, int liveCycle_Second)
        {
            lock (mSFilter)
            {
                TokenObj tokenObj = null;
                tokenKV.TryGetValue(token, out tokenObj);
                if (null == tokenObj)
                {
                    tokenObj = new TokenObj(liveCycle_Second);
                    tokenObj.SetToken(token)
                        .SetIp(clientIP)
                        .SetStartTime(DateTime.Now);
                    tokenKV.Add(token, tokenObj);
                }

                if (null != ImplementAdapter.mSFilterMessage)
                {
                    try
                    {
                        ImplementAdapter.mSFilterMessage.TokenUsed(token, clientIP);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Determine the validity of the client token
        /// </summary>
        /// <param name="token">A valid token of client</param>
        /// <param name="clientIP">Client ip address</param>
        /// <returns></returns>
        public static bool IsEnabledToken(string token, string clientIP)
        {
            lock (mSFilter)
            {
                bool mbool = false;
                GetIP_Token(token, tokenObj =>
                {
                    if (null != tokenObj)
                    {
                        mbool = clientIP.Equals(tokenObj.ip);
                        if (mbool) tokenObj.SetStartTime(DateTime.Now);
                    }
                });
                return mbool;
            }
        }

        /// <summary>
        /// Check whether the token is in the lifetime
        /// </summary>
        /// <param name="token"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static bool CheckTokenLiveCylce(string token, string clientIP)
        {
            lock (mSFilter)
            {
                bool mbool = false;
                GetIP_Token(token, tokenObj =>
                {
                    if (null != tokenObj)
                    {
                        mbool = clientIP.Equals(tokenObj.ip);
                    }
                });
                return mbool;
            }
        }

        public static void RemoveToken(string token)
        {
            lock (mSFilter)
            {
                if (!tokenKV.ContainsKey(token)) return;
                TokenObj token1 = tokenKV[token];
                if (null == token1) return;
                string ip = token1.ip;
                tokenKV.Remove(token);
                if (null != ImplementAdapter.mSFilterMessage)
                {
                    try
                    {
                        ImplementAdapter.mSFilterMessage.TokenKilled(token, ip);
                    }
                    catch { }
                }
            }
        }

        private static string GetIP_Token(string token, Action<TokenObj> action)
        {
            lock (mSFilter)
            {
                string ip = "";
                TokenObj token1 = null;
                if (tokenKV.ContainsKey(token))
                {
                    token1 = tokenKV[token];
                    if (null == action) token1.SetStartTime(DateTime.Now);
                    ip = token1.ip;
                }
                if (null != action) action(token1);
                return ip;
            }
        }

        private bool AuthenticateKey(string key)
        {
            bool mbool = false;
            if (string.IsNullOrEmpty(key)) return mbool;
            MServiceManager sm = MicroServiceRoute.ServiceManager;
            string svrUrl = sm.Uri;
            svrUrl = svrUrl.Replace("\\", "/");
            string s1 = svrUrl.Substring(svrUrl.Length - 1);
            if (s1.Equals("\\") || s1.Equals("/"))
            {
                svrUrl = svrUrl.Substring(0, svrUrl.Length - 1);
            }

            Regex rg = new Regex(@"\/(?<areaName>[a-z0-9_\-]+)$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(svrUrl))
            {
                svrUrl = rg.Replace(svrUrl, "");
            }

            svrUrl += "/{0}/{1}?key={2}".ExtFormat(MSConst.MSCommunication, MSConst.AuthenticateKey, key);
            Dictionary<string, string> heads = new Dictionary<string, string>();
            heads.Add(MSConst.contractKey, sm.ContractKey);

            object data = new { key = key };

            IHttpHelper httpHelper = new HttpHelper();
            httpHelper.SendData(svrUrl, heads, data, true, (resultData, err) =>
            {
                if (!string.IsNullOrEmpty(err)) return;
                if (null == resultData) return;
                bool.TryParse(resultData.ToString().ToLower(), out mbool);
            });
            return mbool;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string clientIP = GetIP(context.HttpContext);
            object controller = context.Controller;
            MethodInfo mi = GetActionMethod(context);
            if (null != ImplementAdapter.mSFilterMessage)
            {
                try
                {
                    ImplementAdapter.mSFilterMessage.ClientIP(clientIP, controller, mi);
                }
                catch (Exception)
                {

                    //throw;
                }
            }

            bool mbool = false;
            Type type = context.Controller.GetType();
            Attribute attr = type.GetCustomAttribute(typeof(MSUnlimited), true);
            if (null != attr)
            {
                base.OnActionExecuting(context);
                return;
            }

            if (null != mi)
            {
                Attribute atr1 = null; // mi.GetCustomAttribute(typeof(MSClientRegisterAction), true);
                Attribute atr2 = null; // mi.GetCustomAttribute(typeof(MSConfiguratorAction), true);
                Attribute atr3 = mi.GetCustomAttribute(typeof(MSUnlimited), true);
                if (null != atr1 || null != atr2 || null != atr3) mbool = true;
            }

            if (!mbool)
            {
                List<string> keys = new List<string>() { tokenKeyName };
                Dictionary<string, object> kvDic = GetKVListFromHeader(context.HttpContext, keys, false);
                if (0 == kvDic.Count)
                {
                    kvDic = GetKVListFromQuery(context.HttpContext, keys, false);
                }

                if (0 < kvDic.Count)
                {
                    string token = kvDic[tokenKeyName.ToLower()].ToString();

                    string ip1 = GetIP(context.HttpContext);
                    string ip2 = "";
                    if (null != ImplementAdapter.mSFilterMessage)
                    {
                        ip2 = ip1;
                        try
                        {
                            mbool = ImplementAdapter.mSFilterMessage.TokenValidating(token, ip1, controller, mi);
                        }
                        catch (Exception)
                        {
                            mbool = false;
                            //throw;
                        }
                    }

                    if (!mbool)
                    {
                        GetIP_Token(token, token1 =>
                        {
                            if (null == token1) return;
                            mbool = token1.ip.Equals(ip1);
                            if (mbool) token1.SetStartTime(DateTime.Now);
                        });
                    }
                    string msg = "Original ip: {0}, Current ip: {1}, Token: {2}".ExtFormat(ip2, ip1, token);
                    PrintIpToLogs(msg);
                }
            }

            if (!mbool)
            {
                string err = "Illegal access";
                List<string> list = new List<string>() { MSConst.contractKey };
                Dictionary<string, object> ckDic = GetKVListFromHeader(context.HttpContext, list, false);
                if (0 == ckDic.Count)
                {
                    ckDic = GetKVListFromQuery(context.HttpContext, list, false);
                }

                if (0 == ckDic.Count)
                {
                    ckDic = GetKVListFromBody(context.HttpContext, list, false);
                }

                if (0 == ckDic.Count)
                {
                    ckDic = GetKVListFromForm(context.HttpContext, list, false);
                }

                if (0 == ckDic.Count)
                {
                    throw new Exception(err);
                }

                string[] keys = ckDic.Keys.ToArray();
                string contractVal1 = ckDic[keys[0]].ToString();
                string svrKey = "";
                if (-1 != contractVal1.IndexOf(MSConst.keySplit))
                {
                    int n = contractVal1.LastIndexOf(MSConst.keySplit);
                    string svrKey1 = contractVal1.Substring(n + MSConst.keySplit.Length);
                    string contractVal3 = contractVal1.Substring(0, n);
                    svrKey = LegalSvrKey(clientIP, contractVal3, svrKey1, false);
                    if(string.IsNullOrEmpty(svrKey))
                    {
                        mbool = AuthenticateKey(svrKey1);
                        if (mbool)
                        {
                            contractVal1 = contractVal3;
                            svrKey = svrKey1;
                        }
                        else
                        {
                            svrKey = "";
                        }
                    }
                    else
                    {
                        contractVal1 = contractVal3;
                    }
                }

                string contractVal2 = MSServiceImpl.GetContractValue();
                if (!contractVal1.Equals(contractVal2))
                {
                    throw new Exception(err);
                }

                PrintIpToLogs("IP: " + clientIP);                
                if (string.IsNullOrEmpty(svrKey))
                {
                    attr = mi.GetCustomAttribute(typeof(MSClientRegisterAction), true);
                    if ((false == IsExistIP(clientIP)) && (null == attr))
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    LegalSvrKey(clientIP, contractVal1, svrKey, true);
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (null != ImplementAdapter.mSFilterMessage)
            {
                string clientIP = GetIP(context.HttpContext);
                object controller = context.Controller;
                MethodInfo mi = GetActionMethod(context);
                try
                {
                    ImplementAdapter.mSFilterMessage.MethodExecuted(clientIP, controller, mi);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            base.OnActionExecuted(context);
        }

        class TokenObj
        {
            public TokenObj(int liveCycle_Second)
            {
                if (0 < liveCycle_Second) this.liveCycle_Second = liveCycle_Second;
            }

            public TokenObj() { }

            public string token { get; private set; }
            public string ip { get; private set; }

            private DateTime _startTime = DateTime.Now;
            public DateTime startTime
            {
                get { return _startTime; }
            }

            public DateTime endTime { get; private set; }
            public int liveCycle_Second { get; private set; } = 3600;

            public TokenObj SetToken(string token)
            {
                this.token = token;
                return this;
            }

            public TokenObj SetIp(string ip)
            {
                this.ip = ip;
                return this;
            }

            public TokenObj SetStartTime(DateTime startTime)
            {
                this._startTime = startTime;
                endTime = _startTime.AddSeconds(liveCycle_Second);
                return this;
            }
        }
    }
}
