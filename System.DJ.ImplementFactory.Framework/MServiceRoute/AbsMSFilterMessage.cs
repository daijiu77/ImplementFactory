﻿using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public delegate void ReceiveIP(string ip, object controller, MethodInfo actionMethod, bool isFinished);
    public abstract class AbsMSFilterMessage : IMSFilterMessage
    {
        private ReceiveIP _receiveIP = null;

        void IMSFilterMessage.TokenKilled(string token, string clientIP)
        {
            TokenKilled(token, clientIP);
        }

        void IMSFilterMessage.TokenUsed(string token, string clientIP)
        {
            TokenUsed(token, clientIP);
        }

        bool IMSFilterMessage.TokenValidating(string token, string clientIP, object controller, MethodInfo actionMethod)
        {
            return TokenValidating(token, clientIP, controller, actionMethod);
        }

        void IMSFilterMessage.ClientIP(string ip, object controller, MethodInfo actionMethod)
        {
            if (null != _receiveIP)
            {
                lock (this)
                {
                    _receiveIP(ip, controller, actionMethod, false);
                }
            }
            ClientIP(ip, controller, actionMethod);
        }

        void IMSFilterMessage.MethodExecuted(string ip, object controller, MethodInfo actionMethod)
        {
            if (null != _receiveIP)
            {
                lock (this)
                {
                    _receiveIP(ip, controller, actionMethod, true);
                }
            }
            MethodExecuted(ip, controller, actionMethod);
        }

        public void SetIPReceiver(ReceiveIP receiveIP)
        {
            _receiveIP = receiveIP;
        }

        /// <summary>
        /// The interface method is triggered when the Token string in the MSFilter gateway reaches the end of life
        /// </summary>
        /// <param name="token">A valid token string</param>
        /// <param name="clientIP">Client IP address</param>
        public virtual void TokenKilled(string token, string clientIP) { }

        /// <summary>
        /// The interface method is triggered when the Token string is enabled in the MSFilter gateway
        /// </summary>
        /// <param name="token">A valid token string</param>
        /// <param name="clientIP">Client IP address</param>
        public virtual void TokenUsed(string token, string clientIP) { }

        /// <summary>
        /// Verify the legitimacy of tokens and clientIPs
        /// </summary>
        /// <param name="token">The token value passed in by the client</param>
        /// <param name="clientIP">Client IP address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        /// <returns>A return value of true indicates that the token value is legal</returns>
        public virtual bool TokenValidating(string token, string clientIP, object controller, MethodInfo actionMethod) { return false; }

        /// <summary>
        /// Get client ip address
        /// </summary>
        /// <param name="ip">Client ip address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        public virtual void ClientIP(string ip, object controller, MethodInfo actionMethod) { }

        /// <summary>
        /// The called method has finished executing.
        /// </summary>
        /// <param name="ip">Client ip address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        public virtual void MethodExecuted(string ip, object controller, MethodInfo actionMethod) { }
    }
}
