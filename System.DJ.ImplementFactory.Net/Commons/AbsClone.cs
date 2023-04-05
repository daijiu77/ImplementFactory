using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.Exts;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.Commons
{
    public abstract class AbsClone : IClone
    {
        static object _clone_Obj1 = new object();
        static DicEle dicEle;
        static Dictionary<object, object> _clone_dic = new Dictionary<object, object>();
        public static T Clone<T>(T t)
        {
            lock (_clone_Obj1)
            {
                _clone_dic.Clear();
                object o = getClone(null, t, 1, 0);
                _clone_dic.Clear();
                return (T)o;
            }            
        }

        static object getClone(object srcEntity, object entity, int clonePara, int while_num)
        {
            while_num++;
            object o = srcEntity;
            if (null == entity) return o;
            Type t = entity.GetType();
            if (DJTools.IsBaseType(t))
            {
                return DJTools.ConvertTo(entity, t);
            }

            Action<object,object> add_dic = (newObj, cloneObj) =>
            {
                object _o = null;
                _clone_dic.TryGetValue(newObj, out _o);
                if (null == _o) _clone_dic.Add(newObj, cloneObj);
            };

            if (null == o)
            {
                Type t1 = entity.GetType();
                if (0 == clonePara && null != srcEntity) t1 = srcEntity.GetType();

                try
                {
                    o = Activator.CreateInstance(t1);
                    if (0 == clonePara && null != srcEntity)
                    {
                        add_dic(srcEntity, o);
                    }
                    else
                    {
                        add_dic(entity, o);
                    }
                }
                catch { }
            }
            if (null == o) return o;

            if (99 < while_num) return o;
                        
            Func<object, DicEle> SameFunc = (newObj) =>
             {
                 object _oo = null;
                 _clone_dic.TryGetValue(newObj, out _oo);                 
                 dicEle.isSame = null != _oo;
                 dicEle.nObj = _oo;
                 return dicEle;
             };

            string fName = "";
            object fv = null;
            DicEle dic_Ele;
            PropertyInfo pi = null;
            Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] arr = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo item in arr)
            {
                fv = null;
                if (item.CanRead)
                {
                    fv = item.GetValue(o, null);
                    if (null == item.PropertyType.GetInterface("IEnumerable")) fv = null;
                    if (typeof(string) == item.PropertyType) fv = null;
                }

                if (!item.CanWrite && null == fv) continue;
                fName = item.Name.ToLower();
                pi = null;
                dic.TryGetValue(fName, out pi);
                if (null != pi) continue;
                dic.Add(fName, item);
            }

            arr = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo item in arr)
            {
                if (!item.CanRead) continue;
                fName = item.Name.ToLower();
                pi = null;
                dic.TryGetValue(fName, out pi);
                if (null == pi) continue;

                fv = item.GetValue(entity, null);
                if (null == fv) continue;

                if (null != item.PropertyType.GetInterface("IEnumerable"))
                {
                    if (item.PropertyType.IsArray)
                    {
                        Array array = fv as Array;
                        if (null == array) continue;
                        int len = array.Length;
                        object arr1 = null;
                        
                        if (pi.CanWrite)
                        {
                            arr1 = ExtCollection.createArrayByType(fv.GetType(), len);
                        }
                        else
                        {
                            arr1 = pi.GetValue(o, null);
                        }
                        if (null == arr1) continue;

                        if (0 < len)
                        {
                            object ele = null;
                            int n = 0;
                            foreach (var a in array)
                            {
                                dic_Ele = SameFunc(a);
                                if (dic_Ele.isSame)
                                {
                                    ele = dic_Ele.nObj;
                                }
                                else
                                {
                                    ele = getClone(null, a, 1, while_num);
                                }
                                ExtCollection.arrayAdd(arr1, ele, n);
                                n++;
                            }
                        }

                        fv = arr1;
                    }
                    else if (null != item.PropertyType.GetInterface("IList"))
                    {
                        ICollection collection = fv as ICollection;
                        if (null == collection) continue;
                        object list = null;
                        if (pi.CanWrite)
                        {
                            list = ExtCollection.createListByType(fv.GetType());
                        }
                        else
                        {
                            list = pi.GetValue(o, null);
                        }
                        if (null == list) continue;

                        if (0 < collection.Count)
                        {
                            object ele = null;
                            foreach (var a in collection)
                            {
                                dic_Ele = SameFunc(a);
                                if (dic_Ele.isSame)
                                {
                                    ele = dic_Ele.nObj;
                                }
                                else
                                {
                                    ele = getClone(null, a, 1, while_num);
                                }
                                ExtCollection.listAdd(list, ele);
                            }
                        }

                        fv = list;
                    }
                    else if (null != item.PropertyType.GetInterface("IDictionary"))
                    {
                        IDictionary collection = fv as IDictionary;
                        if (null == collection) continue;
                        object dic1 = null;
                        if (pi.CanWrite)
                        {
                            dic1 = ExtCollection.createDictionaryByType(fv.GetType());
                        }
                        else
                        {
                            dic1 = pi.GetValue(o, null);
                        }
                        if (null == dic1) continue;

                        if (0 < collection.Count)
                        {
                            string key = "";
                            object val = null;
                            object ele = null;
                            foreach (var a in collection)
                            {
                                key = a.GetType().GetProperty("Key").GetValue(item, null).ToString();
                                val = a.GetType().GetProperty("Value").GetValue(item, null);
                                if (null != val)
                                {
                                    dic_Ele = SameFunc(val);
                                    if (dic_Ele.isSame)
                                    {
                                        ele = dic_Ele.nObj;
                                    }
                                    else
                                    {
                                        ele = getClone(null, val, 1, while_num);
                                    }
                                }
                                ExtCollection.dictionaryAdd(dic1, key, val);
                            }
                        }

                        fv = dic1;
                    }
                }
                else if (item.PropertyType.IsClass)
                {
                    dic_Ele = SameFunc(fv);
                    if (dic_Ele.isSame)
                    {
                        fv = dic_Ele.nObj;
                    }
                    else
                    {
                        fv = getClone(null, fv, 1, while_num);
                    }
                }
                else if (DJTools.IsBaseType(item.PropertyType))
                {
                    fv = DJTools.ConvertTo(fv, item.PropertyType);
                }

                if (pi.CanWrite)
                {
                    try
                    {
                        pi.SetValue(o, fv, null);
                    }
                    catch { }
                }
            }

            return o;
        }

        struct DicEle
        {
            public bool isSame;
            public object nObj;
        }

        //static object _clone_Obj3 = new object();
        T IClone.Clone<T>(object t)
        {
            lock (_clone_Obj1)
            {
                _clone_dic.Clear();
                object o = getClone(this, t, 0, 0);
                _clone_dic.Clear();
                return (T)o;
            }            
        }

        //static object _clone_Obj4 = new object();
        T IClone.Clone<T>()
        {
            lock (_clone_Obj1)
            {
                _clone_dic.Clear();
                object o = getClone(null, this, 1, 0);
                _clone_dic.Clear();
                return (T)o;
            }            
        }
    }
}
