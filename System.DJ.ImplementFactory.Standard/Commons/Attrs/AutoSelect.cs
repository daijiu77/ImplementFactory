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
    public class AutoSelect: AbsDataInterface
    {

        public AutoSelect(string selectExpression) : base()
        {
            sql = selectExpression;
        }

        public AutoSelect(string selectExpression, bool isAsync) : base()
        {
            sql = selectExpression;
            this.IsAsync = isAsync;
        }

        public AutoSelect(string selectExpression, bool isAsync, int msInterval) : base()
        {
            sql = selectExpression;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        public AutoSelect(string dataProviderNamespace, string dataProviderClassName): base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, bool isAsync, int msInterval) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        public AutoSelect(string selectExpression, string[] ResultExecMethod) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
        }

        public AutoSelect(string selectExpression, string[] ResultExecMethod, bool isAsync) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

        public AutoSelect(string selectExpression, string[] ResultExecMethod, bool isAsync, int msInterval) : base()
        {
            sql = selectExpression;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
        }

        public AutoSelect(string dataProviderNamespace, string dataProviderClassName, string[] ResultExecMethod, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.ResultExecMethod = ResultExecMethod;
            this.IsAsync = isAsync;
        }

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
