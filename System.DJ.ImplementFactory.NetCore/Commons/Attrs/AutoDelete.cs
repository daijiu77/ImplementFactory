using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class AutoDelete: AbsDataInterface
    {
        /// <summary>
        /// 执行 delete 操作
        /// </summary>
        /// <param name="deleteExpression">delete 语句表达式</param>
        public AutoDelete(string deleteExpression): base()
        {
            sql = deleteExpression;
        }

        /// <summary>
        /// 执行 delete 操作
        /// </summary>
        /// <param name="deleteExpression">delete 语句表达式</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoDelete(string deleteExpression, DataOptType sqlExecType) : base()
        {
            sql = deleteExpression;
            this.sqlExecType = sqlExecType;
        }

        public AutoDelete(string deleteExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = deleteExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 delete 操作
        /// </summary>
        /// <param name="deleteExpression">delete 语句表达式</param>
        /// <param name="EnabledBuffer">是否启用缓存机制, 默认为 false(不启用)</param>
        public AutoDelete(string deleteExpression, bool EnabledBuffer) : base()
        {
            sql = deleteExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 指定一个生成sql语句的提供者类, 需要指定该所在的命名空间及类名
        /// </summary>
        /// <param name="dataProviderNamespace">提供sql语句类所在的命名空间</param>
        /// <param name="dataProviderClassName">提供sql语句类的类名</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// 指定一个生成sql语句的提供者类, 需要指定该所在的命名空间及类名
        /// </summary>
        /// <param name="dataProviderNamespace">提供sql语句类所在的命名空间</param>
        /// <param name="dataProviderClassName">提供sql语句类的类名</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// 指定一个生成sql语句的提供者类, 需要指定该所在的命名空间及类名
        /// </summary>
        /// <param name="dataProviderNamespace">提供sql语句类所在的命名空间</param>
        /// <param name="dataProviderClassName">提供sql语句类的类名</param>
        /// <param name="EnabledBuffer">是否启用缓存机制, 默认为 false(不启用)</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
