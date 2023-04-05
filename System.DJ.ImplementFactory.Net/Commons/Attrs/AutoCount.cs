using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class AutoCount: AbsDataInterface
    {
        public AutoCount(string countExpression) : base()
        {
            sql = countExpression;
        }

        public AutoCount(string countExpression, bool isAsync) : base()
        {
            sql = countExpression;
            this.IsAsync = isAsync;
        }

        public AutoCount(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        public AutoCount(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        public AutoCount(string countExpression, string[] ResultExecMethod) : base()
        {
            sql = countExpression;
            this.ResultExecMethod = ResultExecMethod;
        }

        public AutoCount(string countExpression, string[] ResultExecMethod, bool isAsync) : base()
        {
            sql = countExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

        public AutoCount(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
        }

        public AutoCount(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

    }
}
