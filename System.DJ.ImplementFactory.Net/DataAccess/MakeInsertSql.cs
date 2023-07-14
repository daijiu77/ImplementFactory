using MySqlX.XDevAPI.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    internal class MakeInsertSql
    {
        public void Execute()
        {
            if (null == ImplementAdapter.dbInfo1) return;
            if (null == ImplementAdapter.taskMultiTablesExec) return;
            if (string.IsNullOrEmpty(ImplementAdapter.dbInfo1.MakeInsertSqlLocalPath)) return;
            string path = ImplementAdapter.dbInfo1.MakeInsertSqlLocalPath;
            if (!Directory.Exists(path)) return;
            string dirName = DateTime.Now.ToString("yyyy-MM-dd_HH");
            path = Path.Combine(path, dirName);
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return;
                //throw;
            }

            if (!Directory.Exists(path)) return;

            string dts = DateTime.Now.ToString("yyyyMMddHH");
            int size = ImplementAdapter.dbInfo1.MakeInsertSqlMaxRecordSize;
            if (0 >= size) size = 1000;
            ImplementAdapter.taskMultiTablesExec.Wait();
            using (DbVisitor db = new DbVisitor())
            {
                UpdateTableDesign.ForEach(tp =>
                {
                    if (typeof(DataCacheTable) == tp) return;
                    IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(tp));
                    scheme.dbSqlBody.WhereIgnoreAll();
                    scheme.dbSqlBody.Skip(1, size);
                    IList<object> list = scheme.ToList(tp);
                    if (null == list) return;
                    CreateInsertSQL((IList)list, (tbName, sql) =>
                    {
                        string f = Path.Combine(path, tbName + "_" + dts + ".sql");
                        try
                        {
                            File.AppendAllLines(f, new string[] { sql });
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                    });
                });
            }
        }

        private void CreateInsertSQL(IList values, Action<string, string> insertSqlAction)
        {
            if (null == values) return;
            if (0 == values.Count) return;

            Type[] ts = values.GetType().GetGenericArguments();
            if (0 == ts.Length) return;
            if (ts[0].IsBaseType()) return;
            if (!ts[0].IsClass) return;

            string sql = "";
            string tbName = ts[0].Name;
            string srcTbName = tbName;
            string fields = "";
            string fieldVals = "";
            bool isCopy = false;
            if (typeof(IEntityCopy).IsAssignableFrom(values[0].GetType()))
            {
                tbName = ((IEntityCopy)values[0]).CopyParentModelType.Name;
                srcTbName = tbName;
                isCopy = true;
            }

            if (null != DbVisitor.sqlAnalysis)
            {
                tbName = DbVisitor.sqlAnalysis.GetTableName(tbName);
            }

            foreach (var item in values)
            {
                fields = "";
                fieldVals = "";
                item.ForeachProperty(true, (ppi, ppt, ffn) =>
                {
                    if (isCopy)
                    {
                        if (-1 != ffn.IndexOf(OverrideModel._AssignmentNo)) return false;
                        if (-1 != ffn.IndexOf(OverrideModel.CopyParentModel)) return false;
                        if (ffn.Equals(OverrideModel.DeleteRelation)) return false;
                        if (ffn.Equals(MultiTablesExec.RecordQuantityFN)) return false;
                    }
                    return true;
                }, (pi, pt, fn, fv) =>
                {
                    if (null == fv) return;
                    if (pt == typeof(byte[])) return;
                    if (!pt.IsBaseType())
                    {
                        if (typeof(IList).IsAssignableFrom(pt) || typeof(IList) == pt)
                        {
                            CreateInsertSQL((IList)fv, insertSqlAction);
                        }
                        return;
                    }

                    if (pt == typeof(DateTime) || pt == typeof(DateTime?))
                    {
                        if ((((DateTime)fv) == DateTime.MinValue) || (((DateTime)fv) == DateTime.MaxValue)) return;
                    }

                    if (null != DbVisitor.sqlAnalysis)
                    {
                        fields += "," + DbVisitor.sqlAnalysis.GetFieldName(fn);
                    }
                    else
                    {
                        fields += "," + fn;
                    }

                    if (pt == typeof(string) || pt == typeof(Guid) || pt == typeof(Guid?))
                    {
                        fieldVals += ",'" + fv.ToString() + "'";
                    }
                    else if (pt == typeof(DateTime) || pt == typeof(DateTime?))
                    {
                        DateTime dt = Convert.ToDateTime(fv);
                        fieldVals += ",'" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    }
                    else if (pt == typeof(bool) || pt == typeof(bool?))
                    {
                        bool mbool = Convert.ToBoolean(fv);
                        fieldVals += "," + (mbool ? 1 : 0);
                    }
                    else if (pt.IsEnum)
                    {
                        fieldVals += "," + (int)fv;
                    }
                    else
                    {
                        fieldVals += "," + fv;
                    }
                });

                if (!string.IsNullOrEmpty(fields))
                {
                    fields = fields.Substring(1);
                    fieldVals = fieldVals.Substring(1);
                    sql = "insert into " + tbName + "(";
                    sql += fields + ") values(" + fieldVals + ");";
                    if (null != insertSqlAction) insertSqlAction(srcTbName, sql);
                }
            }
        }

    }
}
