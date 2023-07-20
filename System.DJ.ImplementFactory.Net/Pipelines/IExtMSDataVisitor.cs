using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public delegate object WillExecute(params object[] args);
    public interface IExtMSDataVisitor
    {
        event WillExecute OnWillExecute;

        IMSAllot mSAllot { get; set; }
    }
}
