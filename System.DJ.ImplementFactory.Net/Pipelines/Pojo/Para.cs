using System.Reflection;
/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines.Pojo
{
    public class Para
    {
        private Type _ParaType = null;
        private string _ParaName = null;
        private string _IsGenericParameter = null;
        private Guid _ID = Guid.Empty;

        public Para()
        {
            _ID = Guid.NewGuid();
        }

        public Para(Guid guid)
        {
            _ID = guid;
        }

        /// <summary>
        /// 获取\设置接口方法参数类型
        /// </summary>
        public Type ParaType
        {
            get { return _ParaType; }
            set
            {
                if (null == _ParaType) _ParaType = value;
            }
        }

        /// <summary>
        /// 获取\设置接口方法参数类型全名
        /// </summary>
        public string ParaTypeName { get; set; }

        /// <summary>
        /// 获取\设置接口方法参数名称
        /// </summary>
        public string ParaName
        {
            get { return _ParaName; }
            set
            {
                if (string.IsNullOrEmpty(_ParaName)) _ParaName = value;
            }
        }

        /// <summary>
        /// 获取\设置接口方法参数值
        /// </summary>
        public object ParaValue { get; set; }

        /// <summary>
        /// 是否是泛型参数
        /// </summary>
        public bool IsGenericParameter { get; set; }

        /// <summary>
        /// 获取Para对象ID
        /// </summary>
        public Guid ID { get { return _ID; } }

        public override string ToString()
        {
            return null == ParaValue ? "" : ParaValue.ToString();
        }

        public int TryInt()
        {
            string s = ToString();
            int n = 0;
            int.TryParse(s, out n);
            return n;
        }

        public float TryFloat()
        {
            string s = ToString();
            float n = 0;
            float.TryParse(s, out n);
            return n;
        }

        public double TryDouble()
        {
            string s = ToString();
            double n = 0;
            double.TryParse(s, out n);
            return n;
        }

        public Guid TryGuid()
        {
            string s = ToString();
            Guid n = Guid.Empty;
            Guid.TryParse(s, out n);
            return n;
        }

        public bool TryBool()
        {
            string s = ToString();
            return s.Trim().ToLower().Equals("true");
        }

        public DateTime TryDateTime()
        {
            string s = ToString();
            DateTime n = DateTime.Now;
            DateTime.TryParse(s, out n);
            return n;
        }

        public decimal TryDecimal()
        {
            string s = ToString();
            decimal n = 0;
            decimal.TryParse(s, out n);
            return n;
        }

        public T TryObject<T>()
        {
            string value = ToString();
            object obj = null;
            T v = default(T);
            if (typeof(T).IsValueType)
            {
                Type type = typeof(T);
                string s = type.ToString();
                string typeName = s.Substring(s.LastIndexOf(".") + 1);
                typeName = typeName.Replace("]", "");
                typeName = typeName.Replace("&", "");
                string methodName = "To" + typeName;
                try
                {
                    Type t = Type.GetType("System.Convert");
                    //执行Convert的静态方法
                    obj = t.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { value });
                    v = (T)obj;
                }
                catch (Exception ex)
                {
                    //throw;
                }
            }
            else if (typeof(string) == typeof(T))
            {
                obj = value;
                v = (T)obj;
            }
            else if (null != ParaValue)
            {
                v = (T)ParaValue;
            }
            return v;
        }

    }
}
