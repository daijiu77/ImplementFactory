using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines.Pojo
{
    public class PList<T> : List<T> where T : Para
    {
        public Para this[string paraName]
        {
            get
            {                
                Para para = null;
                if (string.IsNullOrEmpty(paraName)) return para;
                string pn = paraName.ToLower();
                IList list = this;
                foreach (var item in list)
                {
                    if (null == ((Para)item).ParaName) continue;
                    if (((Para)item).ParaName.ToLower().Equals(pn))
                    {
                        para = (Para)item;
                        break;
                    }
                }
                return para;
            }
        }

        public Para this[string paraName, bool ignoreNull]
        {
            get
            {
                Para p = this[paraName];
                if (ignoreNull)
                {
                    p = null == p ? new Para() : p;
                }
                return p;
            }
        }
    }
}
