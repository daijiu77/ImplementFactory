using System.DJ.ImplementFactory.Commons.Attrs;
using System.Reflection;

namespace System.DJ.ImplementFactory.Commons
{
    public class ForeachExtends
    {
        public void ForeachProperty(object obj, bool isAll, Func<PropertyInfo, Type, string, object, bool> func)
        {
            if (null == obj) return;
            Type type = obj.GetType();
            PropertyInfo[] piArr = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            object v = null;
            bool mbool = false;
            Attribute att = null;
            foreach (PropertyInfo item in piArr)
            {
                att = item.GetCustomAttribute(typeof(IgnoreForeachProp));
                if (null != att) continue;
                if (!isAll)
                {
                    if (item.DeclaringType != type) continue;
                }
                v = item.GetValue(obj);

                try
                {
                    mbool = func(item, item.PropertyType, item.Name, v);
                    if (false == mbool) break;
                }
                catch (Exception)
                {

                    //throw;
                }

            }
        }

        public void ForeachProperty(object obj, Func<PropertyInfo, Type, string, object, bool> func)
        {
            bool isAll = true;
            ForeachProperty(obj, isAll, func);
        }

        public void ForeachProperty(object obj, Action<PropertyInfo, Type, string, object> action)
        {
            ForeachProperty(obj, (pi, fieldType, fName, fValue) =>
            {
                action(pi, fieldType, fName, fValue);
                return true;
            });
        }

        public void ForeachProperty(object obj, bool isAll, Action<PropertyInfo, Type, string, object> action)
        {
            ForeachProperty(obj, isAll, (pi, fieldType, fName, fValue) =>
            {
                action(pi, fieldType, fName, fValue);
                return true;
            });
        }

        public void ForeachProperty(Type objType, bool isAll, Func<PropertyInfo, Type, string, bool> func)
        {
            if (null == objType) return;
            PropertyInfo[] piArr = objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            bool mbool = false;
            Attribute att = null;
            foreach (var item in piArr)
            {
                att = item.GetCustomAttribute(typeof(IgnoreForeachProp));
                if (null != att) continue;
                if (!isAll)
                {
                    if (item.DeclaringType != objType) continue;
                }

                try
                {
                    mbool = func(item, item.PropertyType, item.Name);
                    if (false == mbool) break;
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }

        public void ForeachProperty(Type objType, Func<PropertyInfo, Type, string, bool> func)
        {
            bool isAll = true;
            ForeachProperty(objType, isAll, func);
        }

        public void ForeachProperty(Type objType, Action<PropertyInfo, Type, string> action)
        {
            ForeachProperty(objType, (pi, fieldType, fName) =>
            {
                action(pi, fieldType, fName);
                return true;
            });
        }

        public void ForeachProperty(Type objType, bool isAll, Action<PropertyInfo, Type, string> action)
        {
            ForeachProperty(objType, isAll, (pi, fieldType, fName) =>
            {
                action(pi, fieldType, fName);
                return true;
            });
        }

    }
}
