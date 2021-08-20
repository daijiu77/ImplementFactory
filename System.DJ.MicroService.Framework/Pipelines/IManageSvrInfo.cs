using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.MicroService.Pipelines
{
    public interface IManageSvrInfo
    {
        string ipAddress { get; }
        int portNumber { get; }
        string key { get; }
    }
}
