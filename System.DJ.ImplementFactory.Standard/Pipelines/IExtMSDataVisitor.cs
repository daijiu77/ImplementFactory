using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public delegate object WillExecute(params object[] args);
    public interface IExtMSDataVisitor
    {
        WillExecute willExecute { get; set; }

        IMSAllot mSAllot { get; set; }
    }
}
