using System;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class UpdateTableDesign
    {
        private List<Type> absDataModels = new List<Type>();

        private IDbTableScheme dbTableScheme;
        private AutoCall autoCall = new AutoCall();

        public UpdateTableDesign(IDbTableScheme dbTableScheme)
        {
            this.dbTableScheme = dbTableScheme;

            List<Assembly> assemblies = DJTools.GetAssemblyCollection(DJTools.RootPath, new string[] { "System.DJ.ImplementFactory" });
            Type[] types = null;
            foreach (Assembly item in assemblies)
            {
                types = item.GetTypes();
                foreach (Type tp in types)
                {
                    if (tp.IsAbstract || tp.IsInterface || tp.IsEnum) continue;
                    if (typeof(AbsDataModel).IsAssignableFrom(tp))
                    {
                        absDataModels.Add(tp);
                    }
                }
            }

        }

        public void TableScheme()
        {
            Dictionary<string, string> tableDic = MultiTablesExec.Tables;
            IDbHelper dbHelper = ImplementAdapter.DbHelper;
            List<FieldMapping> fieldMappings = new List<FieldMapping>();
            IEnumerable<Attribute> atts = null;
            Attribute att = null;
            PropertyInfo ppi = null;
            FieldMapping fm = null;
            object v = null;
            string tbName = "";
            string sql = "";
            string err = "";
            foreach (var item in absDataModels)
            {
                tbName = item.Name;
                atts = item.GetCustomAttributes();
                if (null != atts)
                {
                    foreach (Attribute at in atts)
                    {
                        if (-1 != at.GetType().Name.ToLower().IndexOf("table"))
                        {
                            ppi = at.GetType().GetProperty("Name");
                            if (null != ppi)
                            {
                                v = ppi.GetValue(at);
                                if (null == v) v = "";
                                tbName = v.ToString();
                                break;
                            }
                        }
                    }
                }
                if (tableDic.ContainsKey(tbName.ToLower()))
                {
                    AddField(tbName, item);
                    continue;
                }
                fieldMappings.Clear();
                item.ForeachProperty((pi, tp, fn) =>
                {
                    fm = null;
                    if (!tp.IsBaseType()) return;
                    att = pi.GetCustomAttribute(typeof(FieldMapping));
                    if (null != att) fm = (FieldMapping)att;
                    if (null == fm) fm = new FieldMapping(fn);
                    if (null == fm.FieldType) fm.FieldType = tp;
                    fieldMappings.Add(fm);
                });
                if (0 == fieldMappings.Count) continue;
                sql = dbTableScheme.GetTableScheme(tbName, fieldMappings);
                dbHelper.update(autoCall, sql, null, false, null, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
        }

        private void AddField(string tableName, Type type)
        {
            //select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='{0}';
            List<string> list = dbTableScheme.GetFields(tableName);
            if (null == list) return;
            if (0 == list.Count) return;
            IDbHelper dbHelper = ImplementAdapter.DbHelper;
            Dictionary<string, string> fieldDic = new Dictionary<string, string>();
            string fn1 = "";
            string sql = "";
            string err = "";
            foreach (var item in list)
            {
                fn1 = item.ToLower();
                if (fieldDic.ContainsKey(fn1)) continue;
                fieldDic.Add(fn1, item);
            }

            FieldMapping fm = null;
            Attribute att = null;
            type.ForeachProperty((pi, tp, fn) =>
            {
                fm = null;
                if (!tp.IsBaseType()) return;
                if (fieldDic.ContainsKey(fn.ToLower())) return;
                att = pi.GetCustomAttribute(typeof(FieldMapping));
                if (null != att) fm = (FieldMapping)att;
                if (null == fm) fm = new FieldMapping(fn);
                if (null == fm.FieldType) fm.FieldType = tp;
                sql = dbTableScheme.GetAddFieldScheme(tableName, fm);
                dbHelper.update(autoCall, sql, null, false, null, ref err);
            });
            ImplementAdapter.Destroy(dbHelper);
        }
    }
}
