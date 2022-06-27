using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlStructure : DbBody
    {
        public DbSqlStructure() { }

        public int Count()
        {
            return 0;
        }

        public IList<T> ToList<T>()
        {
            return null;
        }

        public T DefaultFrist<T>()
        {
            return default(T);
        }

    }
}
