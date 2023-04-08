using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    /// <summary>
    /// Data quantity statistics SQL query statement property identification.
    /// </summary>
    public class AutoCount: AbsDataInterface
    {
        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="countExpression">A valid data quantity statistics SQL query statement.</param>
        public AutoCount(string countExpression) : base()
        {
            sql = countExpression;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="countExpression">A valid data quantity statistics SQL query statement.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoCount(string countExpression, bool isAsync) : base()
        {
            sql = countExpression;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        public AutoCount(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoCount(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="countExpression">A valid data quantity statistics SQL query statement.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        public AutoCount(string countExpression, string[] ResultExecMethod) : base()
        {
            sql = countExpression;
            this.ResultExecMethod = ResultExecMethod;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="countExpression">A valid data quantity statistics SQL query statement.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoCount(string countExpression, string[] ResultExecMethod, bool isAsync) : base()
        {
            sql = countExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        public AutoCount(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
        }

        /// <summary>
        /// Data quantity statistics SQL query statement property identification.
        /// </summary>
        /// <param name="dataProviderNamespace">SQL query statements provide the namespace in which the class resides.</param>
        /// <param name="dataProviderClassName">The sql query statement provides the class name of the class, which must inherit the ISqlExpressionProvider interface class.</param>
        /// <param name="ResultExecMethod">The array must contain 2 or 3 array elements, 2 array elements: [0] the full path of the class, [2] method name; 3 array element cases: [0] namespace, [1] class name, [2] method name. The method parameter type must be a valid data query result type, and the method return value type must match the main method return value type.</param>
        /// <param name="isAsync">Whether to load data asynchronously, if data is loaded asynchronously, the last parameter type of the method must be the Action type, and the data type passed in matches the query result.</param>
        public AutoCount(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

    }
}
