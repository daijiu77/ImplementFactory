using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public enum DataOptType
    {
        select = 0,
        insert,
        update,
        delete,
        count,
        procedure,
        none
    }
}
