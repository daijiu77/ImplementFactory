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
    /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
    /// </summary>
    public class AutoSelect: AbsDataInterface
    {
        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        public AutoSelect(string selectExpression) : base()
        {
            sql = selectExpression;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoSelect(string selectExpression, bool isAsync) : base()
        {
            sql = selectExpression;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoSelect(string selectExpression, bool isAsync, int msInterval) : base()
        {
            sql = selectExpression;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName): base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, bool isAsync, int msInterval) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        public AutoSelect(string selectExpression, string[] ResultExecMethod) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoSelect(string selectExpression, string[] ResultExecMethod, bool isAsync) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="selectExpression">A valid SQL query statement expression</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoSelect(string selectExpression, string[] ResultExecMethod, bool isAsync, int msInterval) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// The expression attribute of the execution SQL query statement is identified, and the execution result type can be a basic data type, a data entity type, and a data entity collection type.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        /// <param name="msInterval"></param>
        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod, bool isAsync, int msInterval) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

    }
}
