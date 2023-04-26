using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSFilterMessage
    {
        void KillToken(string token, string clientIP);
        void EnabledToken(string token, string clientIP);
    }
}
