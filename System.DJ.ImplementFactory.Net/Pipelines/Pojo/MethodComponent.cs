/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines.Pojo
{
    public class MethodComponent
    {
        /// <summary>
        /// 获取\设置接口方法返回结果变量名称
        /// </summary>
        public string ResultVariantName { get; set; }

        /// <summary>
        /// 获取\设置接口方法返回值类型全名
        /// </summary>
        public string ResultTypeName { get; set; }

        /// <summary>
        /// 获取\设置接口方法返回值类型
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        /// 获取\设置接口实例变量名称
        /// </summary>
        public string InstanceVariantName { get; set; }

        /// <summary>
        /// 获取\设置接口方法名称
        /// </summary>
        public string InterfaceMethodName { get; set; }

        /// <summary>
        /// 获取\设置接口方法参数变量名称字符串系列,多个用英文状态逗号相隔
        /// </summary>
        public string MethodParas { get; set; }

        /// <summary>
        /// 泛型参数
        /// </summary>
        public string GenericityParas { get; set; }

        /// <summary>
        /// 在数据操作时, 是否启用缓存机制
        /// </summary>
        public bool EnabledBuffer { get; set; } = false;

        /// <summary>
        /// 是否需要执行异步加载数据
        /// </summary>
        public bool IsAsync { get; set; } = false;

        /// <summary>
        /// action 参数名称
        /// </summary>
        public string ActionParaName { get; set; }

        /// <summary>
        /// action 参数类型
        /// </summary>
        public Type ActionType { get; set; }

        public object djNormal { get; set; }

        public int Interval { get; set; } = 0;
    }
}
