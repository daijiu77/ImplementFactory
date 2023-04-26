using System;
using System.Collections.Generic;
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
    }
}
