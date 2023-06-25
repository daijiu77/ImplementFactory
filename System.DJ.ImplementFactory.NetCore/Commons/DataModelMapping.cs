using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public class DataModelMapping
    {

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public T ToObjectFrom<T>(object srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, object, string, object, object> funcVal)
        {
            T tObj = default(T);
            if (srcObj.GetType().IsBaseType()) return tObj;
            Type tp = typeof(T);
            try
            {
                tObj = (T)Activator.CreateInstance(tp);
            }
            catch (Exception)
            {
                return tObj;
                //throw;
            }

            Dictionary<string, PropertyInfo> tgPiDic = new Dictionary<string, PropertyInfo>();
            tp.ForeachProperty((pi, pt, fn) =>
            {
                tgPiDic.Add(fn.ToLower(), pi);
            });

            PropertyInfo tgPropertyInfo = null;
            Type piType = null;
            Type srcType = srcObj.GetType();
            object fv = null;
            object vObj = null;
            srcType.ForeachProperty((pi, pt, fn) =>
            {
                tgPropertyInfo = tgPiDic[fn.ToLower()];
                if (null == tgPropertyInfo) return true;
                if (null != funcAssign)
                {
                    if (!funcAssign(pi, fn)) return true;
                }
                fv = pi.GetValue(srcObj, null);
                vObj = fv;
                if (null != funcVal)
                {
                    vObj = funcVal(tObj, srcObj, fn, fv);
                }

                if (null != vObj)
                {
                    piType = tgPropertyInfo.PropertyType;
                    if (typeof(IList).IsAssignableFrom(pt) && typeof(IList).IsAssignableFrom(piType))
                    {
                        Type srcParaType = pt.GetGenericArguments()[0];
                        Type tegartParaType = tgPropertyInfo.PropertyType.GetGenericArguments()[0];
                        vObj = ExecuteStaticMethod("ToListData", new Type[] { tegartParaType, srcParaType }, new object[] { vObj, isTrySetVal, funcAssign, funcVal });
                    }
                    else if (pt.IsClass
                        && (false == pt.IsBaseType())
                        && (false == pt.IsAbstract)
                        && (false == pt.IsInterface)
                        && piType.IsClass
                        && (false == piType.IsBaseType())
                        && (false == piType.IsAbstract)
                        && (false == piType.IsInterface))
                    {
                        vObj = ExecuteStaticMethod("ToObjectData", new Type[] { piType }, new object[] { vObj, isTrySetVal, funcAssign, funcVal });
                    }
                }

                try
                {
                    tgPropertyInfo.SetValue(tObj, vObj);
                }
                catch (Exception)
                {
                    if (isTrySetVal) tObj.SetMethodValue(fn, fv, vObj);
                    //throw;
                }
                return true;
            });
            return tObj;
        }

        /// <summary>
        /// Reflection calls and executes generic methods
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="genericTypes">An array of generic types for the method</param>
        /// <param name="methodParameters">An array of parameters for the method</param>
        /// <returns>Returns the result of the method execution</returns>
        private object ExecuteStaticMethod(string methodName, Type[] genericTypes, object[] methodParameters)
        {
            object vObj = null;
            MethodInfo mi = typeof(DataModelMapping).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (null == mi) return vObj;
            mi = mi.MakeGenericMethod(genericTypes);
            if (null == mi) return vObj;
            if (null != mi)
            {
                try
                {
                    vObj = mi.Invoke(this, methodParameters);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            return vObj;
        }

        #region 'Reflection calling' is prohibited for deletion
        /// <summary>
        /// 'Reflection calling' is prohibited for deletion
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcM">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns></returns>
        private T ToObjectData<T>(object srcM, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, object, string, object, object> funcVal)
        {
            return ToObjectFrom<T>(srcM, isTrySetVal, funcAssign, funcVal);
        }

        /// <summary>
        /// 'Reflection calling' is prohibited for deletion
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns></returns>
        private List<T> ToListData<T, TT>(IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            return (List<T>)ToListFrom<T, TT>(srcList, isTrySetVal, funcAssign, funcVal);
        }
        #endregion

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public IList<T> ToListFrom<T, TT>(IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            if (false == typeof(IList).IsAssignableFrom(srcList.GetType())) return null;
            IList<T> list = new List<T>();
            if (null == srcList) return list;
            if (0 == srcList.Count()) return list;
            T t = default(T);
            foreach (TT item in srcList)
            {
                t = ToObjectFrom<T>(item, isTrySetVal, funcAssign, (targetEle, srcEle, fieldName, fieldValue) =>
                {
                    TT tt = default(TT);
                    if (typeof(TT) == srcEle.GetType()) tt = (TT)item;
                    return funcVal(targetEle, tt, fieldName, fieldValue);
                });
                list.Add(t);
            }
            return list;
        }
    }
}
