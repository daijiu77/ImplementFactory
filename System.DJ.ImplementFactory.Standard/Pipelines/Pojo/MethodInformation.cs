using System.Reflection;
using System.DJ.ImplementFactory.Commons;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines.Pojo
{
    public enum LeftSpaceLevel
    {
        one = 0,
        two, three, four, five, six, seven, eight, nine, ten
    }

    public enum FieldsType
    {
        /// <summary>
        /// 排除
        /// </summary>
        Exclude,
        /// <summary>
        /// 包含
        /// </summary>
        Contain
    }

    public class MethodInformation
    {
        /// <summary>
        /// 接口类型
        /// </summary>
        public Type ofInterfaceType { get; set; }

        /// <summary>
        /// 实例类型
        /// </summary>
        public Type ofInstanceType { get; set; }

        /// <summary>
        /// 接口的系统方法信息对象
        /// </summary>
        public MethodInfo methodInfo { get; set; }

        MethodComponent mc = new MethodComponent();
        /// <summary>
        /// 接口方法组件信息对象
        /// </summary>
        public MethodComponent methodComponent { get { return mc; } }

        /// <summary>
        /// 设置\获取默认左缩进空间(空字符串)
        /// </summary>
        public string StartSpace { get; set; }

        /// <summary>
        /// 设置\获取AutoCall对象变量名称
        /// </summary>
        public string AutoCallVarName { get; set; }

        /// <summary>
        /// 设置\获取接口方法参数集合变量名称
        /// </summary>
        public string ParaListVarName { get; set; }

        /// <summary>
        /// 设置\获取动态sql提供者实例所在名称空间
        /// </summary>
        public string dataProviderNamespace { get; set; }

        /// <summary>
        /// 设置\获取动态sql提供者实例类名称
        /// </summary>
        public string dataProviderClassName { get; set; }

        /// <summary>
        /// 执行 select\count 查询后，结果执行方法，new string[] { nameSpace, className, methodName },
        /// 该参数所指向的方法参数类型及返回值类型必须与数据接口方法返回值类型一至
        /// </summary>
        public string[] ResultExecMethod { get; set; }

        /// <summary>
        /// 根据指定的tab数量获取左边代码空格空字符串
        /// </summary>
        /// <param name="tabNum">左边缩进tab数量</param>
        /// <returns></returns>
        public string getSpace(int tabNum)
        {
            string s = "";
            for (int i = 0; i < tabNum; i++)
            {
                s += "    ";
            }
            return s;
        }

        /// <summary>
        /// 把需格式化的字符串附加到code字符串后，附加前在code字符串后加回车换行
        /// </summary>
        /// <param name="code">返回字符串附加后的结果</param>
        /// <param name="level">左边tab层级量(空字符串)</param>
        /// <param name="s">要附加的字符串</param>
        /// <param name="arr">格式化{0}、{1}..字符串待的替换字符系列</param>
        public void append(ref string code, LeftSpaceLevel level, string s, params string[] arr)
        {
            string space1 = null == StartSpace ? "" : StartSpace;
            space1 += getSpace((int)level);

            string s2 = null == s ? "" : s;
            if (null != arr)
            {
                if (0 < arr.Length)
                {
                    int n = 0;
                    foreach (string item in arr)
                    {
                        s2 = s2.Replace("{" + n + "}", item);
                        n++;
                    }
                }
            }

            s2 = space1 + s2;

            if (string.IsNullOrEmpty(code))
            {
                code = s2;
            }
            else
            {
                code += "\r\n" + s2;
            }
        }

        /// <summary>
        /// 把普通字符串附加到code字符串后，附加前在code字符串后加回车换行
        /// </summary>
        /// <param name="code">返回字符串附加后的结果</param>
        /// <param name="level">左边tab层级量(空字符串)</param>
        /// <param name="s">要附加的字符串</param>
        public void append(ref string code, LeftSpaceLevel level, string s)
        {
            append(ref code, level, s, null);
        }

        /// <summary>
        /// 左边默认按照0个tab的缩进把普通字符串附加到code字符串后，附加前在code字符串后加回车换行
        /// </summary>
        /// <param name="code">返回字符串附加后的结果</param>
        /// <param name="s">要附加的字符串</param>
        public void append(ref string code, string s)
        {
            append(ref code, LeftSpaceLevel.one, s, null);
        }

        PList<Para> _paraList = new PList<Para>();
        /// <summary>
        /// 设置\获取接口方法参数集合
        /// </summary>
        public PList<Para> paraList
        {
            get { return _paraList; }
            set
            {
                if (null == value) return;
                PList<Para> dic = value;
                _paraList.Clear();
                foreach (Para item in dic)
                {
                    Para p = new Para();
                    p.SetPropertyFrom(item, (pi) =>
                    {
                        if (pi.Name.ToLower().Equals("id")) return false;
                        return true;
                    });
                    _paraList.Add(p);
                }
            }
        }

        /// <summary>
        /// 字段名称集合
        /// </summary>
        public string[] fields { get; set; }

        /// <summary>
        /// 指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude
        /// </summary>
        public FieldsType fieldsType { get; set; } = FieldsType.Exclude;

        /// <summary>
        /// sql 表达式执行类型
        /// </summary>
        public DataOptType sqlExecType { get; set; } = DataOptType.none;

        public DataOptType dataOptType { get; set; }

        public object AutoCall { get; set; }

        public string autoCallParaValue { get; set; }
    }
}
