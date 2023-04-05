using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    internal class NormalEntityPara
    {
        public AutoCall autoCall { get; set; }
        public AutoCall autoCall_Impl { get; set; }
        public Type implementType { get; set; }
        public Type objType { get; set; }
        public Type actionType { get; set; }
        public PList<Para> paraList { get; set; }
        public bool EnabledBuffer { get; set; }
        public bool isNotInheritInterface { get; set; }
        public string autocall_name { get; set; }
        public string paraListVarName { get; set; }
        public string impl_name { get; set; }
        public string interfaceName { get; set; }
        public string methodName { get; set; }
        public string plist { get; set; }
        public bool isAsync { get; set; }
        public int msInterval { get; set; }
        public string actionParaName { get; set; }
        public string return_type { get; set; }
        public string genericity { get; set; }
    }
}
