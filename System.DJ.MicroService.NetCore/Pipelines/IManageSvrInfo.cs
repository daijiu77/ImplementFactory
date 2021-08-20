using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.MicroService.Pipelines
{
    public interface IManageSvrInfo
    {
        string ipAddress { get; set; }
        int portNumber { get; set; }
        string key { get; set; }
    }
}
