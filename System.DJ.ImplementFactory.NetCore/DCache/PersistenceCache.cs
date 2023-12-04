using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.DCache
{
    public class PersistenceCache
    {
        /// <summary>
        /// key: methodPath, value: key|Guid
        /// </summary>
        private static Dictionary<string, MethodItem> fDic = new Dictionary<string, MethodItem>();
        private static string fileDirPath = "";
        private static string fileDirName = "DataCaches";
        private const string _PersistenceSource = "file";
        private const string _dataExtName = "dc";
        private const string _propertyExtName = "pro";
        private const string _keyName = "key";
        private const string _methodName = "method";
        private const string _startName = "start";
        private const string _endName = "end";

        private const int flagSize = 20;
        public static Task task = null;
        static PersistenceCache()
        {
            fileDirPath = Path.Combine(DJTools.RootPath, fileDirName);
            if (!Directory.Exists(fileDirPath))
            {
                Directory.CreateDirectory(fileDirPath);
            }

            if (null == ImplementAdapter.dbInfo1.PersistenceSource) ImplementAdapter.dbInfo1.PersistenceSource = _PersistenceSource;
            ImplementAdapter.dbInfo1.PersistenceSource = ImplementAdapter.dbInfo1.PersistenceSource.ToLower();

            if (_PersistenceSource.Equals(ImplementAdapter.dbInfo1.PersistenceSource))
            {
                LoadFileCache();
                return;
            }

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

        private static void LoadFileCache()
        {
            task = Task.Run(() =>
            {                
                string[] fs = Directory.GetFiles(fileDirPath, "*." + _dataExtName);
                List<DataCacheTable> list = null;
                DataCacheTable tb = null;
                DataCachePool cachePool = new DataCachePool();
                List<Guid> ids = new List<Guid>();
                const int units = 1024 * 5;
                int size = 0;
                int pos = 0;
                byte[] buffer = null;
                byte[] data = null;
                object vObj = null;
                string fPath = "";
                string[] arr = null;
                DateTime start = DateTime.Now;
                DateTime end = DateTime.Now;
                foreach (string f in fs)
                {
                    FileStream fst = File.OpenRead(f);
                    size = (int)fst.Length;
                    data = new byte[size];
                    pos = 0;
                    while (pos < size)
                    {
                        if (units > (size - pos))
                        {
                            buffer = new byte[size - pos];
                        }
                        else
                        {
                            buffer = new byte[units];
                        }
                        fst.Read(buffer, 0, buffer.Length);
                        Array.Copy(buffer, 0, data, pos, buffer.Length);
                        pos += buffer.Length;
                    }
                    fst.Close();
                    fst.Dispose();

                    tb = null;
                    list = data.ByteArrayToList<DataCacheTable>();
                    if (null != list)
                    {
                        if (0 < list.Count) tb = list[0];
                    }

                    if (null != tb)
                    {
                        fPath = Path.Combine(fileDirPath, tb.Id + "." + _propertyExtName);
                        if (File.Exists(fPath))
                        {
                            tb.Start = GetValByKey<DateTime>(tb.Id, _startName);
                            tb.End = GetValByKey<DateTime>(tb.Id, _endName);
                        }

                        if (tb.End < DateTime.Now)
                        {
                            ids.Add(tb.Id);
                        }
                        else
                        {
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

                            MethodItem methodItem = null;
                            KeyItem keyItem = null;
                            fDic.TryGetValue(tb.MethodPath, out methodItem);
                            if (null == methodItem)
                            {
                                methodItem = new MethodItem();
                                fDic.Add(tb.MethodPath, methodItem);
                            }

                            if (null == methodItem[tb.Key])
                            {
                                keyItem = new KeyItem()
                                {
                                    Id = tb.Id.ToString(),
                                    key = tb.Key,
                                    methodPath = tb.MethodPath,
                                    start = tb.Start,
                                    end = tb.End,
                                };
                                methodItem[tb.Key] = keyItem;
                            }
                        }                                                
                    }
                    Thread.Sleep(10);
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
            if (_PersistenceSource.Equals(ImplementAdapter.dbInfo1.PersistenceSource))
            {
                return SetToFile(methodPath, key, value, refOutParams, dataType, cacheTime, start, end);
            }

            DbVisitor db = new DbVisitor();
            IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<DataCacheTable>());
            sqlScheme.dbSqlBody.Where(ConditionItem.Me.And("MethodPath", ConditionRelation.Equals, methodPath),
                ConditionItem.Me.And("Key", ConditionRelation.Equals, key));
            int count = sqlScheme.Count();
            if (0 < count) return Guid.Empty;

            DataCacheTable cacheTable = InitData(methodPath, key, value, refOutParams, dataType, cacheTime, start, end);
            sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From(cacheTable));
            sqlScheme.Insert();
            return cacheTable.Id;
        }

        private Guid SetToFile(string methodPath, string key, object value, RefOutParams refOutParams, Type dataType, int cacheTime, DateTime start, DateTime end)
        {
            lock (fDic)
            {
                Guid guid = Guid.Empty;
                MethodItem methodItem = null;
                KeyItem keyItem = null;
                fDic.TryGetValue(methodPath, out methodItem);
                if (null == methodItem)
                {
                    methodItem = new MethodItem();
                    fDic.Add(methodPath, methodItem);
                }

                if (null == methodItem[key])
                {
                    keyItem = new KeyItem()
                    {
                        key = key,
                        methodPath = methodPath,
                        start = start,
                        end = end,
                    };
                    methodItem[key] = keyItem;
                }

                keyItem = methodItem[key];
                Guid.TryParse(keyItem.Id, out guid);
                if (Guid.Empty != guid) return Guid.Empty;

                DataCacheTable cacheTable = InitData(methodPath, key, value, refOutParams, dataType, cacheTime, start, end);
                List<DataCacheTable> list = new List<DataCacheTable>();
                list.Add(cacheTable);
                byte[] data = list.ListToByteArray();
                int size = data.Length;
                int currentPos = 0;
                const int units = 1024 * 5;
                byte[] buffer = new byte[units];
                string fPath = Path.Combine(fileDirPath, cacheTable.Id + "." + _dataExtName);
                FileStream fs = File.Open(fPath, FileMode.OpenOrCreate, FileAccess.Write);
                while (currentPos < size)
                {
                    if (units > (size - currentPos))
                    {
                        buffer = new byte[size - currentPos];
                    }
                    Array.Copy(data, currentPos, buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, buffer.Length);
                    currentPos += buffer.Length;
                }
                fs.Close();
                fs.Dispose();

                SetProFile(cacheTable.Id, methodPath, key, start, end);

                keyItem.Id = cacheTable.Id.ToString();

                return cacheTable.Id;
            }
        }

        private void SetProFile(Guid id, string methodPath, string key, DateTime start, DateTime end)
        {
            string fPath = Path.Combine(fileDirPath, id + "." + _propertyExtName);
            string txt = "{0}\t{1}".ExtFormat(_startName, start.ToString("yyyy-MM-dd HH:mm:ss"));
            DJTools.append(ref txt, "{0}\t{1}", _endName, end.ToString("yyyy-MM-dd HH:mm:ss"));
            DJTools.append(ref txt, "{0}\t{1}", _keyName, key);
            DJTools.append(ref txt, "{0}\t{1}", _methodName, methodPath);
            File.WriteAllText(fPath, txt);
        }

        private static Dictionary<string, object> vkDic = new Dictionary<string, object>();
        private string GetKey(object id, string tag)
        {
            string key = "{0}@{1}".ExtFormat(id, tag);
            return key;
        }

        private static T GetValByKey<T>(Guid id, string tag, bool setBuffer)
        {
            lock (vkDic)
            {
                PersistenceCache cache = new PersistenceCache();
                string key = cache.GetKey(id, tag);  //id + "@" + tag;
                object vObj = null;
                vkDic.TryGetValue(key, out vObj);
                if (null != vObj)
                {
                    return (T)vObj;
                }

                T val = default(T);
                string fPath = Path.Combine(fileDirPath, id + "." + _propertyExtName);
                if (!File.Exists(fPath)) return val;
                string[] arr = File.ReadAllLines(fPath);
                string[] arr1 = null;
                string s = "";
                foreach (var item in arr)
                {
                    arr1 = item.Split('\t');
                    if (arr1[0].Trim().Equals(tag))
                    {
                        s = arr1[1].Trim();
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(s))
                {
                    val = DJTools.ConvertTo<T>(s);
                    if(setBuffer) vkDic.Add(key, val);
                }
                return val;
            }
        }

        private static T GetValByKey<T>(Guid id, string tag)
        {
            lock (vkDic)
            {
                return GetValByKey<T>(id, tag, false);
            }
        }

        private DataCacheTable InitData(string methodPath, string key, object value, RefOutParams refOutParams, Type dataType, int cacheTime, DateTime start, DateTime end)
        {
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
                refOutParams.Foreach(ckv =>
                {
                    list.Add(ckv);
                    return true;
                });
                refOutDatas = ObjectToByteArray(list, typeof(List<CKeyValue>).TypeToString(true));
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
                Array.Copy(buffer, pos, datas, 0, buffer.Length);
                pos += flagSize;

                Array.Copy(refOutDatas, 0, datas, pos, refOutDatas.Length);
                pos += refOutDatas.Length;

                Array.Copy(cacheTable.Data, 0, datas, pos, cacheTable.Data.Length);
                cacheTable.Data = datas;
            }
            return cacheTable;
        }

        public void Remove(string id)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(id, out guid);
            if (guid == Guid.Empty) return;

            if (_PersistenceSource.Equals(ImplementAdapter.dbInfo1.PersistenceSource))
            {
                RemoveFile(id);
                return;
            }

            DataCacheTable tb = new DataCacheTable();
            tb.Id = guid;

            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(tb));
            scheme.dbSqlBody.Where(ConditionItem.Me.And("Id", ConditionRelation.Equals, id));
            scheme.Delete();
        }

        private void RemoveFile(string id)
        {
            Guid guid = new Guid(id);
            string methodPath = GetValByKey<string>(guid, _methodName);
            string key = GetValByKey<string>(guid, _keyName);

            string tag = GetKey(id, _methodName); //id + "@" + _methodName;
            vkDic.Remove(tag);
            tag = GetKey(id, _keyName); //id + "@" + _keyName;
            vkDic.Remove(tag);

            MethodItem methodItem = null;
            fDic.TryGetValue(methodPath, out methodItem);
            if (null != methodItem)
            {
                methodItem.Remove(key);
                if (0 == methodItem.Count)
                {
                    fDic.Remove(methodPath);
                }
            }
            string fPath = Path.Combine(fileDirPath, id + "." + _propertyExtName);
            try
            {
                File.Delete(fPath);
            }
            catch (Exception)
            {

                //throw;
            }

            fPath = Path.Combine(fileDirPath, id + "." + _dataExtName);
            try
            {
                File.Delete(fPath);
            }
            catch (Exception)
            {

                //throw;
            }
        }

        public void UpdateTime(string id, DateTime start, DateTime end)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(id, out guid);
            if (guid == Guid.Empty) return;

            if (_PersistenceSource.Equals(ImplementAdapter.dbInfo1.PersistenceSource))
            {
                UpdateTimeToFile(id, start, end);
                return;
            }

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

        private void UpdateTimeToFile(string id, DateTime start, DateTime end)
        {
            Guid guid = new Guid(id);
            string methodPath = GetValByKey<string>(guid, _methodName, true);
            string key = GetValByKey<string>(guid, _keyName, true);
            SetProFile(guid, methodPath, key, start, end);
        }

        class MethodItem
        {
            /// <summary>
            /// key: key, value: methodPath|key|id|start|end
            /// </summary>
            private Dictionary<string, KeyItem> dic = new Dictionary<string, KeyItem>();
            public KeyItem this[string key]
            {
                get
                {
                    KeyItem keyItem = null;
                    dic.TryGetValue(key, out keyItem);
                    return keyItem;
                }
                set
                {
                    if (dic.ContainsKey(key)) return;
                    dic[key] = value;
                }
            }

            public void Add(string key, KeyItem keyItem)
            {
                if (dic.ContainsKey(key)) return;
                dic[key] = keyItem;
            }

            public void Remove(string key)
            {
                dic.Remove(key);
            }

            public int Count
            {
                get { return dic.Count; }
            }
        }

        class KeyItem
        {
            public string Id { get; set; }
            public string methodPath { get; set; }
            public string key { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
        }

    }
}
