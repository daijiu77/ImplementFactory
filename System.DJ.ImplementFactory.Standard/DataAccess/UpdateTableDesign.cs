﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class UpdateTableDesign
    {
        private static List<Type> absDataModels = new List<Type>();
        private static TableInfoDetail tableFieldInfos = new TableInfoDetail();

        private IDbTableScheme dbTableScheme;
        private AutoCall autoCall = new AutoCall();

        public UpdateTableDesign(IDbTableScheme dbTableScheme)
        {
            this.dbTableScheme = dbTableScheme;

            if (0 < absDataModels.Count) return;

            List<Assembly> assemblies = DJTools.GetAssemblyCollection(DJTools.RootPath, new string[] { "System.DJ.ImplementFactory" });
            Type[] types = null;
            foreach (Assembly item in assemblies)
            {
                types = item.GetTypes();
                foreach (Type tp in types)
                {
                    if (tp.IsAbstract || tp.IsInterface || tp.IsEnum) continue;
                    if (typeof(IEntityCopy).IsAssignableFrom(tp)) continue;
                    if (typeof(ImplementAdapter).IsAssignableFrom(tp)) continue;
                    if (!typeof(AbsDataModel).IsAssignableFrom(tp)) continue;
                    if (absDataModels.Contains(tp)) continue;
                    absDataModels.Add(tp);
                }
            }
        }

        private bool IsEnabledType(Type type, ref Type type1, ref int length)
        {
            bool mbool = false;
            type1 = null;
            length = 50;
            if (typeof(System.Collections.ICollection).IsAssignableFrom(type))
            {
                Type tp = null;
                if (typeof(byte[]) == type)
                {
                    tp = type;
                    type1 = type;
                    mbool = true;
                }
                else if (type.IsArray)
                {
                    tp = type.GetTypeForArrayElement();
                }
                else if (typeof(IList).IsAssignableFrom(type))
                {
                    Type[] types = type.GetGenericArguments();
                    if (null != types)
                    {
                        if (0 < types.Length) tp = types[0];
                    }
                }

                if (null == tp) return mbool;
                if (tp.IsBaseType())
                {
                    mbool = true;
                    type1 = typeof(string);
                    length = 500;
                }
            }
            else if (type.IsBaseType())
            {
                type1 = type;
                mbool = true;
            }
            else if (type.IsEnum)
            {
                type1 = typeof(int);
                mbool = true;
            }

            return mbool;
        }

        public UpdateTableDesign AddTable(Type tableType)
        {
            if (null == tableType) return this;
            if (!typeof(AbsDataModel).IsAssignableFrom(tableType)) return this;
            if (absDataModels.Contains(tableType)) return this;
            absDataModels.Add(tableType);
            return this;
        }

        public static void ForEach(Action<Type> action)
        {
            if (null == action) return;
            if (null == ImplementAdapter.taskUpdateTableDesign) return;
            ImplementAdapter.taskUpdateTableDesign.Wait();
            foreach (var item in absDataModels)
            {
                action(item);
            }
        }

        public UpdateTableDesign AddTable<T>() where T : AbsDataModel
        {
            return AddTable(typeof(T));
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
            Type type1 = null;
            int length = 0;
            object v = null;
            string tbName = "";
            string sql = "";
            string err = "";
            foreach (var item in absDataModels)
            {
                tbName = item.Name;
                DbAdapter.printSql(autoCall, "Table name: <" + tbName + ">");
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
                tbName = dbTableScheme.sqlAnalysis.GetLegalName(tbName);
                if (tableDic.ContainsKey(tbName.ToLower()))
                {
                    AddField(tbName, item);
                    continue;
                }
                fieldMappings.Clear();
                item.ForeachProperty((pi, tp, fn) =>
                {
                    fm = null;
                    type1 = null;
                    length = 0;
                    if (!IsEnabledType(tp, ref type1, ref length)) return;
                    att = pi.GetCustomAttribute(typeof(FieldMapping));
                    if (null != att) fm = (FieldMapping)att;
                    if (null == fm) fm = new FieldMapping(fn);
                    if (null == fm.FieldType) fm.FieldType = type1;
                    if (0 == fm.Length) fm.Length = length;
                    fieldMappings.Add(fm);
                    tableFieldInfos.Add(tbName, fm.FieldName, null, length, !fm.NoNull, fm.IsPrimaryKey);
                });
                DbAdapter.printSql(autoCall, "Table name: <" + tbName + ">, fields mapping count: " + fieldMappings.Count);
                if (0 == fieldMappings.Count) continue;
                MultiTablesExec.SetTable(tbName);
                sql = dbTableScheme.GetTableScheme(tbName, fieldMappings);
                dbHelper.update(autoCall, sql, null, false, null, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
        }

        public static TableInfoDetail GetTableInfoDetail()
        {
            if (null == ImplementAdapter.taskUpdateTableDesign) throw new Exception("Set the UpdateTableDesign node in the configuration file 'ImplementFactory.xml' to true");
            ImplementAdapter.taskUpdateTableDesign.Wait();
            return tableFieldInfos;
        }

        private Type getTypeByName(string typeName, int len)
        {
            Type type = typeof(string);
            if (null == type) return type;
            string tn = typeName.Trim().ToLower();
            if (-1 != tn.IndexOf("bit") || (-1 != tn.IndexOf("int") && 1 == len) || (-1 != tn.IndexOf("number") && 1 == len))
            {
                type = typeof(bool);
            }
            else if (-1 != tn.IndexOf("bigint"))
            {
                type = typeof(long);
            }
            else if ((-1 != tn.IndexOf("money")) || (-1 != tn.IndexOf("num")))
            {
                type = typeof(double);
            }
            else if ((-1 != tn.IndexOf("float")) || (-1 != tn.IndexOf("real")))
            {
                type = typeof(float);
            }
            else if (-1 != tn.IndexOf("int"))
            {
                type = typeof(int);
            }
            else if ((-1 != tn.IndexOf("binary")) || (-1 != tn.IndexOf("image")))
            {
                type = typeof(byte[]);
            }
            return type;
        }

        private void AddField(string tableName, Type type)
        {
            //select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='{0}';
            List<FieldInformation> list = dbTableScheme.GetFields(tableName);
            if (null == list) return;
            if (0 == list.Count) return;
            IDbHelper dbHelper = ImplementAdapter.DbHelper;
            Dictionary<string, string> fieldDic = new Dictionary<string, string>();
            string fn1 = "";
            string sql = "";
            string err = "";
            foreach (var item in list)
            {
                fn1 = item.Name.ToLower();
                if (fieldDic.ContainsKey(fn1)) continue;
                tableFieldInfos.Add(tableName, item.Name, item.ValueType, item.Length, item.IsNull, item.IsPrimaryKey);
                fieldDic.Add(fn1, item.Name);
            }

            FieldMapping fm = null;
            Attribute att = null;
            Type type1 = null;
            int length = 0;
            type.ForeachProperty((pi, tp, fn) =>
            {
                fm = null;
                type1 = null;
                length = 0;
                fn1 = dbTableScheme.sqlAnalysis.GetLegalName(fn);
                if (!IsEnabledType(tp, ref type1, ref length)) return;
                if (fieldDic.ContainsKey(fn1.ToLower())) return;
                att = pi.GetCustomAttribute(typeof(FieldMapping));
                if (null != att) fm = (FieldMapping)att;
                if (null == fm) fm = new FieldMapping(fn1);
                if (null == fm.FieldType) fm.FieldType = type1;
                if (0 == fm.Length) fm.Length = length;
                sql = dbTableScheme.GetAddFieldScheme(tableName, fm);
                dbHelper.update(autoCall, sql, null, false, null, ref err);
            });
            ImplementAdapter.Destroy(dbHelper);
        }


    }
}
