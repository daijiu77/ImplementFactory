using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public abstract class AbsMSFilterMessage : IMSFilterMessage
    {
        void IMSFilterMessage.TokenKilled(string token, string clientIP)
        {
            TokenKilled(token, clientIP);
        }

        void IMSFilterMessage.TokenUsed(string token, string clientIP)
        {
            TokenUsed(token, clientIP);
        }

        bool IMSFilterMessage.TokenValidating(string token, string clientIP)
        {
            return TokenValidating(token, clientIP);
        }

        public virtual void TokenKilled(string token, string clientIP) { }

        public virtual void TokenUsed(string token, string clientIP) { }

        public virtual bool TokenValidating(string token, string clientIP) { return false; }
    }
}
