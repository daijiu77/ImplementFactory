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
    /// The identifier of the SQL statement expression property that performs the Insert data operation.
    /// </summary>
    public class AutoInsert: AbsDataInterface
    {
        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        public AutoInsert(string insertExpression): base()
        {
            sql = insertExpression;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoInsert(string insertExpression, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoInsert(string insertExpression, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoInsert(string insertExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation.default is exclude.</param>
        public AutoInsert(string insertExpression, string[] fields) : base()
        {
            sql = insertExpression;
            this.fields = fields;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation,default is exclude.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoInsert(string insertExpression, string[] fields, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="insertExpression">A valid SQL statement expression that performs an insert data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the insert data operation,default is exclude.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoInsert(string insertExpression, string[] fields, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="sqlExecType">SQL expression execution type.</param>
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Insert data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="EnabledBuffer">Whether to enable caching, default is false (not enabled)</param>
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
