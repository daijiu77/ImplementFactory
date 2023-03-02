using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class PersistenceCache
    {
        static PersistenceCache()
        {
            Task.Run(() =>
            {
                ImplementAdapter.task.Wait();
                if (null != ImplementAdapter.task1) ImplementAdapter.task1.Wait();
                DbVisitor db = new DbVisitor();
                IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<DataCacheTable>());
                int recordCount = sqlScheme.Count();
                if (0 == recordCount) return;

                const int pageSize = 50;
                int pageIndex = 1;
                int pageCount = recordCount / pageSize;
                if (0 < (recordCount % pageSize)) pageCount++;
                object vObj = null;
                sqlScheme.dbSqlBody.Orderby(OrderbyItem.Me.Set("MethodPath", OrderByRule.Asc));
                IList<DataCacheTable> list = null;
                DataCachePool cachePool = new DataCachePool();
                for (int i = 0; i < pageCount; i++)
                {
                    pageIndex = i + 1;
                    sqlScheme.dbSqlBody.Skip(pageIndex, pageSize);
                    list = sqlScheme.ToList<DataCacheTable>();
                    foreach (DataCacheTable tb in list)
                    {
                        vObj = ByteArrayToObject(tb.Data, tb.DataType);
                        cachePool.Put(tb.MethodPath, tb.Key, vObj, tb.DataType, tb.CycleTimeSecond, tb.Start, tb.End);
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(100);
                }
            });
        }

        private static object ByteArrayToObject(byte[] data, string dataType)
        {
            object vObj = null;
            if (string.IsNullOrEmpty(dataType)) return vObj;
            if (null == data) return vObj;
            Type type = Type.GetType(dataType);
            if (null == type)
            {
                type = DJTools.GetTypeByFullName(dataType);
            }
            if (null == type) return vObj;
            if (type.IsBaseType())
            {
                string s = Encoding.UTF8.GetString(data);
                vObj = DJTools.ConvertTo(s, type);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                DataTable dt = data.ByteArrayToDataTable();
                Type[] tps = type.GetGenericArguments();
                vObj = dt.DataTableToList(tps[0]);
            }
            else
            {
                DataTable dt = data.ByteArrayToDataTable();
                List<object> list = dt.DataTableToList(type);
                if (0 < list.Count) vObj = list[0];
            }
            return vObj;
        }

        private byte[] ObjectToByteArray(object vObj, string dataType)
        {
            byte[] bts = null;
            if (null == vObj) return bts;
            if (string.IsNullOrEmpty(dataType)) return bts;
            Type type = Type.GetType(dataType);
            if (null == type)
            {
                type = DJTools.GetTypeByFullName(dataType);
            }
            if (null == type) return bts;

            if (type.IsBaseType())
            {
                bts = Encoding.UTF8.GetBytes(vObj.ToString());
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                List<object> list = new List<object>();
                IEnumerable enums = vObj as IEnumerable;
                foreach (var item in enums)
                {
                    list.Add(item);
                }
                bts = list.ListToByteArray();
            }
            else
            {
                bts = vObj.ObjectToByteArray();
            }
            return bts;
        }

        public Guid Set(string methodPath, string key, object value, Type dataType, int cacheTime, DateTime start, DateTime end)
        {
            DbVisitor db = new DbVisitor();
            IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<DataCacheTable>());
            sqlScheme.dbSqlBody.Where(ConditionItem.Me.And("MethodPath", ConditionRelation.Equals, methodPath),
                ConditionItem.Me.And("Key", ConditionRelation.Equals, key));
            int count = sqlScheme.Count();
            if (0 < count) return Guid.Empty;

            DataCacheTable cacheTable = new DataCacheTable()
            {
                Id = Guid.NewGuid(),
                MethodPath = methodPath,
                Key = key,
                DataType = dataType.FullName,
                CycleTimeSecond = cacheTime,
                Start = start,
                End = end
            };

            cacheTable.Data = ObjectToByteArray(value, dataType.FullName);
            sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From(cacheTable));
            sqlScheme.Insert();
            return cacheTable.Id;
        }

        public void Remove(string id)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(id, out guid);
            if (guid == Guid.Empty) return;

            DataCacheTable tb = new DataCacheTable();
            tb.Id = guid;

            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(tb));
            scheme.Delete();
        }

        public void UpdateTime(string id, DateTime start, DateTime end)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(id, out guid);
            if (guid == Guid.Empty) return;

            DataCacheTable tb = new DataCacheTable();
            tb.Id = guid;
            tb.Start = start;
            tb.End = end;

            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(tb));
            scheme.dbSqlBody.DataOperateContains("Start", "End");
            scheme.Update();
        }
    }
}
