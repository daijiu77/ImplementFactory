using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
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
                string ip = mSFilter.GetIP(context);
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

        public static void RemoveToken(string token)
        {
            lock (mSFilter)
            {
                if (!tokenKV.ContainsKey(token)) return;
                TokenObj token1 = tokenKV[token];
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

        private static string GetIP_Token(string token)
        {
            lock (mSFilter)
            {
                return GetIP_Token(token, null);
            }
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
                        ip2 = GetIP_Token(token);
                        mbool = ip2.Equals(ip1);
                    }
                    string msg = "Original ip: {0}, Current ip: {1}, Token: {2}".ExtFormat(ip2, ip1, token);
                    PrintIpToLogs(msg);
                }
            }

            if (!mbool)
            {
                string err = "Illegal access";
                List<string> list = new List<string>() { MServiceConst.contractKey };
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
                string contractVal2 = MSServiceImpl.GetContractValue();
                if (!contractVal1.Equals(contractVal2))
                {
                    throw new Exception(err);
                }
                                
                PrintIpToLogs("IP: " + clientIP);
                attr = mi.GetCustomAttribute(typeof(MSClientRegisterAction), true);
                if ((false == IsExistIP(clientIP)) && (null == attr))
                {
                    throw new Exception(err);
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
