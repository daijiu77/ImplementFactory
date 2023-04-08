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
    /// The identifier of the SQL statement expression property that performs the Update data operation.
    /// </summary>
    public class AutoUpdate: AbsDataInterface
    {

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        public AutoUpdate(string updateExpression) : base()
        {
            sql = updateExpression;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoUpdate(string updateExpression, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoUpdate(string updateExpression, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoUpdate(string updateExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        public AutoUpdate(string updateExpression, string[] fields) : base()
        {
            sql = updateExpression;
            this.fields = fields;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoUpdate(string updateExpression, string[] fields, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="updateExpression">A valid SQL statement expression that performs an update data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoUpdate(string updateExpression, string[] fields, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Update data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
