using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IExecuteSql: IDisposable
    {
        string Key { get; set; }
        string connectString { get; set; }

        bool disposableAndClose { get; set; }

        IDataServerProvider dataServerProvider { get; set; }

        IDbConnectionState dbConnectionState { get; set; }

        void Add(object tempData);
    }
}
