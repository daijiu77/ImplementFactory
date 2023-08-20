using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.DCache
{
    public class PersistenceCache
    {
        private const int flagSize = 20;
        public static Task task = null;
        static PersistenceCache()
        {
            task = Task.Run(() =>
            {
                if (null != ImplementAdapter.taskMultiTablesExec) ImplementAdapter.taskMultiTablesExec.Wait();
                if (null != ImplementAdapter.taskUpdateTableDesign) ImplementAdapter.taskUpdateTableDesign.Wait();
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
                List<Guid> ids = new List<Guid>();

                for (int i = 0; i < pageCount; i++)
                {
                    pageIndex = i + 1;
                    sqlScheme.dbSqlBody.Skip(pageIndex, pageSize);
                    list = sqlScheme.ToList<DataCacheTable>();
                    foreach (DataCacheTable tb in list)
                    {
                        if (tb.End < DateTime.Now)
                        {
                            ids.Add(tb.Id);
                            continue;
                        }
                        vObj = ByteArrayToObject(tb.Data, tb.DataType);
                        cachePool.Put(
                            tb.Id.ToString(),
                            tb.MethodPath,
                            tb.Key,
                            vObj,
                            tb.DataType,
                            tb.CycleTimeSecond,
                            tb.Start,
                            tb.End);
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(100);
                }

                PersistenceCache persistenceCache = new PersistenceCache();
                foreach (Guid guid in ids)
                {
                    persistenceCache.Remove(guid.ToString());
                }
                ids.Clear();
            });
        }

        private static object ByteArrayToObject(byte[] data, string dataType)
        {
            object vObj = null;
            if (null == data) return vObj;
            if (string.IsNullOrEmpty(dataType)) return vObj;

            RefOutParams refOutParams = null;
            if (flagSize < data.Length)
            {
                byte[] buffer = new byte[flagSize];
                Array.Copy(data, 0, buffer, 0, flagSize);
                string s = Encoding.UTF8.GetString(buffer).Trim();
                string s1 = "";
                if (0 < s.Length)
                {
                    s1 = s.Substring(0, 1);
                }

                if (DataCachePool.flag.Equals(s1))
                {
                    s1 = s.Substring(1);
                    int n = s1.IndexOf('\0');
                    if (-1 != n)
                    {
                        s1 = s1.Substring(0, n);
                    }
                    int pos = flagSize;
                    int size = 0;
                    int.TryParse(s1, out size);
                    buffer = new byte[size];
                    Array.Copy(data, pos, buffer, 0, size);
                    Type t = typeof(CKeyValue);
                    DataTable dataTable = buffer.ByteArrayToDataTable();
                    List<CKeyValue> list = dataTable.DataTableToList<CKeyValue>();

                    refOutParams = new RefOutParams();
                    foreach (CKeyValue item in list)
                    {
                        refOutParams.Add(item.Key, item);
                    }

                    pos += size;
                    size = data.Length - pos;
                    buffer = new byte[size];
                    Array.Copy(data, pos, buffer, 0, size);
                    data = buffer;
                }
            }
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

            if (null != refOutParams)
            {
                DataCacheVal dataCacheVal = new DataCacheVal();
                dataCacheVal.refOutParams = refOutParams;
                dataCacheVal.result = vObj;
                vObj = dataCacheVal;
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

        public Guid Set(string methodPath, string key, object value, RefOutParams refOutParams, Type dataType, int cacheTime, DateTime start, DateTime end)
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

            byte[] refOutDatas = null;
            if (null != refOutParams)
            {
                List<CKeyValue> list = new List<CKeyValue>();
                Type typeRefOut = typeof(List<CKeyValue>);
                refOutParams.Foreach(ckv =>
                {
                    list.Add(ckv);
                    return true;
                });
                refOutDatas = ObjectToByteArray(list, typeRefOut.TypeToString(true));
            }

            cacheTable.Data = ObjectToByteArray(value, dataType.FullName);
            if (null != refOutDatas)
            {
                int size = cacheTable.Data.Length;
                size += refOutDatas.Length;
                size += flagSize;
                byte[] datas = new byte[size];
                string s = DataCachePool.flag + refOutDatas.Length;
                byte[] buffer = Encoding.UTF8.GetBytes(s);
                int pos = 0;
                Array.Copy(datas, pos, buffer, 0, buffer.Length);
                pos += flagSize;

                Array.Copy(datas, pos, refOutDatas, 0, refOutDatas.Length);
                pos += refOutDatas.Length;

                Array.Copy(datas, pos, cacheTable.Data, 0, cacheTable.Data.Length);
                cacheTable.Data = datas;
            }
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
            scheme.dbSqlBody.Where(ConditionItem.Me.And("Id", ConditionRelation.Equals, id));
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
            scheme.dbSqlBody.Where(ConditionItem.Me.And("Id", ConditionRelation.Equals, id));
            scheme.dbSqlBody.DataOperateContains("Start", "End");
            scheme.Update();
        }
    }
}
