using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines.Pojo
{
    public class DbList<T> : List<T> where T : DbParameter
    {
        public static IDataServerProvider dataServerProvider { get; set; }

        public T this[string name]
        {
            get
            {
                T dbParameter = null;
                List<T> ts = this;
                if (null == ts) return dbParameter;
                foreach (T item in ts)
                {
                    if (((DbParameter)item).ParameterName.Equals(name))
                    {
                        dbParameter = item;
                        break;
                    }
                }
                return dbParameter;
            }
        }

        public void Add(string name, object value)
        {
            if (null == dataServerProvider)
            {
                throw new Exception("ImplementAdapter.dataServerProvider 不能为空");
            }
            T dbP = this[name];
            object v = null == value ? DBNull.Value : value;
            if (null != dbP)
            {
                dbP.Value = v;
                return;
            }
            dbP = (T)dataServerProvider.CreateDbParameter(name, v);
            this.Add(dbP);
        }

    }
}
