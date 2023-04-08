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
    /// <summary>
    /// Removes the data SQL statement expression attribute identification.
    /// </summary>
    public class AutoDelete: AbsDataInterface
    {
        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="deleteExpression">A valid delete statement expression</param>
        public AutoDelete(string deleteExpression): base()
        {
            sql = deleteExpression;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="deleteExpression">A valid delete statement expression</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoDelete(string deleteExpression, DataOptType sqlExecType) : base()
        {
            sql = deleteExpression;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="deleteExpression">A valid delete statement expression</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoDelete(string deleteExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = deleteExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="deleteExpression">delete 语句表达式</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoDelete(string deleteExpression, bool EnabledBuffer) : base()
        {
            sql = deleteExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// Removes the data SQL statement expression attribute identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoDelete(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
