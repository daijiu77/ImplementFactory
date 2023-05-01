using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSFilterMessage
    {
        /// <summary>
        /// The interface method is triggered when the Token string in the MSFilter gateway reaches the end of life
        /// </summary>
        /// <param name="token">A valid token string</param>
        /// <param name="clientIP">Client IP address</param>
        void TokenKilled(string token, string clientIP);

        /// <summary>
        /// The interface method is triggered when the Token string is enabled in the MSFilter gateway
        /// </summary>
        /// <param name="token">A valid token string</param>
        /// <param name="clientIP">Client IP address</param>
        void TokenUsed(string token, string clientIP);

        /// <summary>
        /// Verify the legitimacy of tokens and clientIPs
        /// </summary>
        /// <param name="token">The token value passed in by the client</param>
        /// <param name="clientIP">Client IP address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        /// <returns>A return value of true indicates that the token value is legal</returns>
        bool TokenValidating(string token, string clientIP, object controller, MethodInfo actionMethod);

        /// <summary>
        /// Get client ip address
        /// </summary>
        /// <param name="ip">Client ip address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        void ClientIP(string ip, object controller, MethodInfo actionMethod);

        /// <summary>
        /// The called method has finished executing.
        /// </summary>
        /// <param name="ip">Client ip address</param>
        /// <param name="controller">The controller class object being called</param>
        /// <param name="actionMethod">The method being called</param>
        void MethodExecuted(string ip, object controller, MethodInfo actionMethod);
    }
}
