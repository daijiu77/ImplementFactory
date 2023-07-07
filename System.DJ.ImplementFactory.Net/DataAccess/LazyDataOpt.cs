using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class LazyDataOpt
    {
        public void SetValue(object currentModel, Constraint constraint, Type propertyType, string propertyName, object currentPropertyValue, object newPropertyValue)
        {
            if (currentPropertyValue == newPropertyValue) return;
            if (propertyType.IsBaseType())
            {
                //Base value                
                DbVisitor db = new DbVisitor();
                try
                {
                    IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(currentModel));
                    List<ConditionItem> conditions = getConditionItems(currentModel, (DbSqlScheme)scheme);
                    if (0 == conditions.Count) return;
                    scheme.dbSqlBody.Where(conditions.ToArray());
                    scheme.dbSqlBody.DataOperateContains(propertyName);

                    string field = getPropertyName(currentModel, propertyName);
                    Dictionary<string, object> kvDic = new Dictionary<string, object>();
                    kvDic.Add(field, newPropertyValue);
                    scheme.AppendUpdate(kvDic);
                }
                catch (Exception ex)
                {
                    ImplementAdapter.autoCall.e(ex.ToString());
                    //throw;
                }
                finally
                {
                    ((IDisposable)db).Dispose();
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propertyType) && (typeof(string) != propertyType))
            {
                //Collection
                Type eleType = null;
                if (propertyType.IsList())
                {
                    Type[] ts = propertyType.GetGenericArguments();
                    eleType = ts[0];
                }
                else if (propertyType.IsArray)
                {
                    eleType = propertyType.GetTypeForArrayElement();
                }

                if (null == eleType) return;
                if (null != currentPropertyValue)
                {
                    //delete currentPropertyValue
                    DeleteData(currentModel, (IEnumerable)currentPropertyValue);
                }

                if (null != newPropertyValue)
                {
                    //add newPropertyValue
                    AddData(currentModel, propertyName, (IEnumerable)newPropertyValue);
                }
            }
            else
            {
                //object
                if (null != currentPropertyValue)
                {
                    //delete currentPropertyValue
                    List<object> list = new List<object>();
                    list.Add(currentPropertyValue);
                    DeleteData(currentModel, list);
                }

                if (null != newPropertyValue)
                {
                    //add newPropertyValue
                    List<object> list = new List<object>();
                    list.Add(newPropertyValue);
                    AddData(currentModel, propertyName, list);
                }
            }
        }

        public int AddData(object currentModel, string propertyName, IEnumerable collection)
        {
            int num = 0;
            Dictionary<string, object> kvDic = getRelationData(currentModel, propertyName);
            bool mbool = false;
            DbVisitor db = new DbVisitor();
            foreach (var item in collection)
            {
                if (null == item) continue;
                mbool = true;
                foreach (var kv in kvDic)
                {
                    if (null != (item as IEntityCopy))
                    {
                        ((IEntityCopy)item).AssignmentNo = true;
                    }
                    mbool = item.SetPropertyValue(kv.Key, kv.Value);
                    if (!mbool) break;
                }
                if (!mbool) break;
                IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(item));
                num += scheme.Insert();
            }
            ((IDisposable)db).Dispose();
            return num;
        }

        private Dictionary<string, object> getRelationData(object currentModel, string propertyName)
        {
            Dictionary<string, object> kvDic = new Dictionary<string, object>();
            if (null == currentModel) return kvDic;
            if (string.IsNullOrEmpty(propertyName)) return kvDic;
            string field = propertyName.ToLower();
            currentModel.GetType().ForeachProperty((pi, pt, fn) =>
            {
                if (!field.Equals(fn.ToLower())) return;
                Commons.Attrs.Constraint constraint = pi.GetCustomAttribute<Commons.Attrs.Constraint>();
                if (null == constraint) return;
                if (string.IsNullOrEmpty(constraint.ForeignKey) || string.IsNullOrEmpty(constraint.RefrenceKey)) return;
                PropertyInfo propertyInfo = getPropertyInfo(currentModel, constraint.ForeignKey);
                object v = currentModel.GetPropertyValue(propertyInfo.PropertyType, constraint.ForeignKey);
                if (null == v) return;
                if (v.ToString().Length == 0) return;
                kvDic[constraint.RefrenceKey] = v;

                if (null != constraint.Foreign_refrenceKeys)
                {
                    bool mbool = true;
                    bool success = true;
                    foreach (var item in constraint.Foreign_refrenceKeys)
                    {
                        if (mbool)
                        {
                            mbool = false;
                            propertyInfo = getPropertyInfo(currentModel, item);
                            v = currentModel.GetPropertyValue(propertyInfo.PropertyType, item);
                            if (null == v)
                            {
                                success = false;
                                break;
                            }

                            if (v.ToString().Length == 0)
                            {
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            mbool = true;
                            kvDic[item] = v;
                        }
                    }

                    if (!success)
                    {
                        kvDic.Clear();
                    }
                }
            });
            return kvDic;
        }

        private PropertyInfo getPropertyInfo(object currentModel, string propertyName)
        {
            PropertyInfo pi = null;
            string field = propertyName.ToLower();
            currentModel.GetType().ForeachProperty((ppi, pt, fn) =>
            {
                if (!field.Equals(fn.ToLower())) return;
                pi = ppi;
            });
            return pi;
        }

        public int DeleteData(object currentModel, IEnumerable collection)
        {
            int num = 0;
            DbVisitor db = new DbVisitor();
            List<ConditionItem> conditions = new List<ConditionItem>();
            bool isDeleteRelation = getDeleteRelation(currentModel);
            foreach (var item in collection)
            {
                if (null == item) continue;
                IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(item));
                if (0 == conditions.Count)
                {
                    conditions = getConditionItems(item, (DbSqlScheme)scheme);
                }
                if (0 == conditions.Count) break;
                scheme.dbSqlBody.Where(conditions.ToArray());
                num += scheme.Delete(isDeleteRelation);
            }
            ((IDisposable)db).Dispose();
            return num;
        }

        private bool getDeleteRelation(object currentModel)
        {
            bool isDel = false;
            string field = OverrideModel.DeleteRelation.ToLower();
            currentModel.GetType().ForeachProperty((pi, pt, fn) =>
            {
                if (!field.Equals(fn.ToLower())) return;
                object v = pi.GetValue(currentModel, null);
                if (null != v)
                {
                    isDel = Convert.ToBoolean(v);
                }
            });
            return isDel;
        }

        private string getPropertyName(object currentModel, string propertyName)
        {
            string field = propertyName;
            string fLower = propertyName.ToLower();
            currentModel.GetType().ForeachProperty((pi, pt, fn) =>
            {
                if (!fLower.Equals(fn.ToLower())) return;
                FieldMapping fm = pi.GetCustomAttribute<FieldMapping>();
                if (null == fm) return;
                field = fm.FieldName;
            });
            return field;
        }

        private List<ConditionItem> getConditionItems(object model, DbSqlScheme scheme)
        {
            List<ConditionItem> conditionItems = new List<ConditionItem>();
            Dictionary<string, PropertyInfo> keyDic = scheme.GetPrimaryKey(model.GetType());
            if (null == keyDic) return conditionItems;
            if (0 == keyDic.Count) return conditionItems;
            object vObj = null;
            bool mbool = true;
            foreach (var item in keyDic)
            {
                vObj = model.GetPropertyValue(item.Value.PropertyType, item.Value.Name);
                if (null == vObj)
                {
                    mbool = false;
                    break;
                }
                conditionItems.Add(ConditionItem.Me.And(item.Value.Name, ConditionRelation.Equals, vObj));
            }
            if (false == mbool) conditionItems.Clear();
            return conditionItems;
        }
    }
}
