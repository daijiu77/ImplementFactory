using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    /// <summary>
    /// The identifier of the SQL statement expression property that performs the Procedure data operation.
    /// </summary>
    public class AutoProcedure : AbsDataInterface
    {
        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        public AutoProcedure(string sqlExpression): base()
        {
            sql = sqlExpression;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoProcedure(string sqlExpression, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoProcedure(string sqlExpression, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation.</param>
        /// <param name="fieldsType">Indicates whether each data element (field name) contained in the field name array is Exclude or Contain when performing data operations, which defaults to Exclude.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        public AutoProcedure(string sqlExpression, string[] fields) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoProcedure(string sqlExpression, string[] fields, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="sqlExpression">A valid SQL statement expression that performs an procedure data operation.</param>
        /// <param name="fields">An array of valid field name strings. Field names that must be excludedincluded when performing the update data operation,default is exclude.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoProcedure(string sqlExpression, string[] fields, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The identifier of the SQL statement expression property that performs the Procedure data operation.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName, bool isAsync, int msInterval) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

    }
}
