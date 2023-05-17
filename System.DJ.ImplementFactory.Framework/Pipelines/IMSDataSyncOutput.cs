using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSDataSyncOutput
    {
        object Insert();
        object Update();
        object Delete();
    }
}
