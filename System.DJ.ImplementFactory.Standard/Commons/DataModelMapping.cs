using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Exts;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public class DataModelMapping
    {
        private const string _ToListData = "ToListData";
        private const string _ToObjectData = "ToObjectData";

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public T ToObjectFrom<T>(object srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
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
                //tgPropertyInfo = tgPiDic[fn.ToLower()];
                tgPiDic.TryGetValue(fn.ToLower(), out tgPropertyInfo);
                if (null == tgPropertyInfo) return true;
                if (null != funcAssign)
                {
                    if (!funcAssign(pi, fn)) return true;
                }

                fv = pi.GetValue(srcObj, null);
                if (null == fv) return true;
                vObj = fv;

                if (null != funcVal)
                {
                    vObj = funcVal(tObj, srcObj, fn, fv);
                }

                if (null != vObj)
                {
                    piType = tgPropertyInfo.PropertyType;

                    bool ptList = pt.IsList();
                    bool piTypeList = piType.IsList();
                    bool ptBaseType = pt.IsBaseType();
                    bool piBaseType = piType.IsBaseType();
                    //if (typeof(IList).IsAssignableFrom(pt) && typeof(IList).IsAssignableFrom(piType))
                    if (ptList && piTypeList)
                    {
                        Type srcParaType = pt.GetGenericArguments()[0];
                        Type tegartParaType = tgPropertyInfo.PropertyType.GetGenericArguments()[0];
                        vObj = ExecuteStaticMethod(_ToListData, new Type[] { tegartParaType, srcParaType }, new object[] { vObj, isTrySetVal, funcAssign, funcVal });
                    }
                    else if (pt.IsClass
                        && (false == ptBaseType)
                        && (false == pt.IsAbstract)
                        && (false == pt.IsInterface)
                        && piType.IsClass
                        && (false == piBaseType)
                        && (false == piType.IsAbstract)
                        && (false == piType.IsInterface))
                    {
                        vObj = ExecuteStaticMethod(_ToObjectData, new Type[] { piType },
                            new object[] { vObj, isTrySetVal, funcAssign, funcVal });
                    }
                }

                if (null == vObj) return true;

                try
                {
                    if (pt.IsList())
                    {
                        object tgVal = tgPropertyInfo.GetValue(tObj, null);
                        if (null != tgVal)
                        {
                            IEnumerable ienum = (IEnumerable)vObj;
                            IList list = tgVal as IList;
                            if (null == list) return true;
                            foreach (var item in ienum)
                            {
                                list.Add(item);
                            }
                            return true;
                        }
                    }
                    else if (pt.IsArray)
                    {
                        object tgVal = tgPropertyInfo.GetValue(tObj, null);
                        if (null != tgVal)
                        {
                            Type eleTp = pt.GetTypeForArrayElement();

                            if (null != eleTp)
                            {
                                object list = ExtCollection.createListByType(eleTp);
                                IEnumerable ienum = (IEnumerable)tgVal;
                                foreach (var item in ienum)
                                {
                                    ExtCollection.listAdd(list, item);
                                }

                                ienum = (IEnumerable)vObj;
                                foreach (var item in ienum)
                                {
                                    ExtCollection.listAdd(list, item);
                                }

                                vObj = ExtCollection.listToArray(list);
                            }
                        }
                    }
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
        private T ToObjectData<T>(object srcM, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
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
        private List<T> ToListData<T, TT>(IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            return (List<T>)ToListFrom<T, TT>(srcList, isTrySetVal, funcAssign, (tg, src, fn, fv) =>
            {
                if (null != funcVal)
                {
                    fv = funcVal(tg, src, fn, fv);
                }
                return fv;
            });
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
            foreach (object item in srcList)
            {
                t = ToObjectFrom<T>(item, isTrySetVal, funcAssign, (targetEle, srcEle, fieldName, fieldValue) =>
                {
                    return Call_back1<T, TT>(targetEle, srcEle, fieldName, fieldValue, funcVal, null, null);
                });
                list.Add(t);
            }
            return list;
        }

        public IList<T> ToListWhithChildModel<T, TT>(IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            if (false == typeof(IList).IsAssignableFrom(srcList.GetType())) return null;
            IList<T> list = new List<T>();
            if (null == srcList) return list;
            if (0 == srcList.Count()) return list;
            T t = default(T);
            foreach (object item in srcList)
            {
                t = ToObjectFrom<T>(item, isTrySetVal, funcAssign, (targetEle, srcEle, fieldName, fieldValue) =>
                {
                    return Call_back1<T, TT>(targetEle, srcEle, fieldName, fieldValue, null, null, funcVal);
                });
                list.Add(t);
            }
            return list;
        }

        public object Call_back1<T, TT>(object targetEle, object srcEle, string fieldName, object fieldValue,
            Func<T, TT, string, object, object> callback1,
            Func<T, object, string, object, object> callback2,
            Func<object, object, string, object, object> callback3)
        {
            TT tt = default(TT);
            Type ttType = typeof(TT);
            Type srcType = srcEle.GetType();
            if ((ttType == srcType) || ttType.IsAssignableFrom(srcType)) tt = (TT)srcEle;

            T t = default(T);
            Type tType = typeof(T);
            Type tgType = targetEle.GetType();
            if ((tType == tgType) || tType.IsAssignableFrom(tgType)) t = (T)targetEle;

            if (null != callback1)
            {
                return callback1(t, tt, fieldName, fieldValue);
            }
            else if (null != callback2)
            {
                return callback2(t, srcEle, fieldName, fieldValue);
            }
            else if (null != callback3)
            {
                return callback3(targetEle, srcEle, fieldName, fieldValue);
            }
            return fieldValue;
        }

        public object Call_back2<T, TT>(T targetEle, object srcEle, string fieldName, object fieldValue,
            Func<T, TT, string, object, object> callback1,
            Func<T, object, string, object, object> callback2,
            Func<object, object, string, object, object> callback3)
        {
            TT tt = default(TT);
            Type ttType = typeof(TT);
            Type srcType = srcEle.GetType();
            if ((ttType == srcType) || ttType.IsAssignableFrom(srcType)) tt = (TT)srcEle;

            if (null != callback1)
            {
                return callback1(targetEle, tt, fieldName, fieldValue);
            }
            else if (null != callback2)
            {
                return callback2(targetEle, srcEle, fieldName, fieldValue);
            }
            else if (null != callback3)
            {
                return callback3(targetEle, srcEle, fieldName, fieldValue);
            }
            return fieldValue;
        }

        public object Call_back3<T, TT>(T targetEle, TT srcEle, string fieldName, object fieldValue,
            Func<T, TT, string, object, object> callback1,
            Func<T, object, string, object, object> callback2,
            Func<object, object, string, object, object> callback3)
        {
            if (null != callback1)
            {
                return callback1(targetEle, srcEle, fieldName, fieldValue);
            }
            else if (null != callback2)
            {
                return callback2(targetEle, srcEle, fieldName, fieldValue);
            }
            else if (null != callback3)
            {
                return callback3(targetEle, srcEle, fieldName, fieldValue);
            }
            return fieldValue;
        }

        public object Call_back4<T>(object targetEle, object srcEle, string fieldName, object fieldValue,
            Func<T, object, string, object, object> callback1,
            Func<object, object, string, object, object> callback2)
        {
            T t = default(T);
            Type tType = typeof(T);
            Type tgType = targetEle.GetType();
            if ((tType == tgType) || tType.IsAssignableFrom(tgType)) t = (T)targetEle;

            if (null != callback1)
            {
                return callback1(t, srcEle, fieldName, fieldValue);
            }
            else if (null != callback2)
            {
                return callback2(targetEle, srcEle, fieldName, fieldValue);
            }
            return fieldValue;
        }
    }
}
