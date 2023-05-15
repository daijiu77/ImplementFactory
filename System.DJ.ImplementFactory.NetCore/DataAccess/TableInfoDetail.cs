using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Entities;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class TableInfoDetail : IEnumerable<TableDetail>
    {
        private IEnumerableItem enumerableItem = new IEnumerableItem();

        public TableDetail this[string tableName]
        {
            get { return enumerableItem[tableName]; }
        }

        public TableDetail this[int index]
        {
            get { return enumerableItem[index]; }
        }

        public TableInfoDetail Add(string tableName, string fieldName, string fieldType, int valueLength, bool isNull, bool isPrimaryKey)
        {
            enumerableItem.Add(tableName, fieldName, fieldType, valueLength, isNull, isPrimaryKey);
            return this;
        }

        IEnumerator<TableDetail> IEnumerable<TableDetail>.GetEnumerator()
        {
            return enumerableItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumerableItem;
        }

        public class IEnumerableItem : IEnumerator<TableDetail>, IEnumerator
        {
            private Dictionary<string, TableDetail> _fields = new Dictionary<string, TableDetail>();
            private List<string> _names = new List<string>();
            private List<string> _tableNames = new List<string>();
            private TableDetail _tableDetail = null;
            private int _index = 0;

            public TableDetail this[string tableName]
            {
                get
                {
                    TableDetail list = null;
                    string key = tableName.Trim().ToLower();
                    _fields.TryGetValue(key, out list);
                    return list;
                }
            }

            public TableDetail this[int index]
            {
                get
                {
                    TableDetail list = null;
                    if (index < _names.Count)
                    {
                        string key = _names[_index];
                        list = _fields[key];
                    }
                    return list;
                }
            }

            public void Add(string tableName, string fieldName, string fieldType, int valueLength, bool isNull, bool isPrimaryKey)
            {
                string key = tableName.ToLower();
                TableDetail tableDetail = null;
                if (_fields.ContainsKey(key))
                {
                    tableDetail = _fields[key];
                }
                else
                {
                    tableDetail = new TableDetail();
                    tableDetail.SetName(tableName);
                    _fields.Add(key, tableDetail);
                    _names.Add(key);
                    _tableNames.Add(tableName);
                }
                tableDetail.Add(fieldName, fieldType, valueLength, isNull, isPrimaryKey);
            }

            public int Count => _names.Count;

            public string[] TableNams { get { return _tableNames.ToArray(); } }

            public void Clear()
            {
                _names.Clear();
                _fields.Clear();
                _index = 0;
                _tableDetail = null;
            }

            public void Remove(TableFieldInfo tableFieldInfo)
            {
                string tKey = tableFieldInfo.TableName.ToLower();
                if (_fields.ContainsKey(tKey))
                {
                    if (_tableDetail == _fields[tKey]) _tableDetail = null;
                }
                _names.Remove(tKey);
                _fields.Remove(tKey);
                _index = 0;
            }

            TableDetail IEnumerator<TableDetail>.Current => _tableDetail;

            object IEnumerator.Current => _tableDetail;

            void IDisposable.Dispose()
            {
                _index = 0;
            }

            bool IEnumerator.MoveNext()
            {
                if (_index >= _names.Count) return false;
                string key = _names[_index];
                _tableDetail = _fields[key];
                _index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                _index = 0;
            }
        }
    }
}
