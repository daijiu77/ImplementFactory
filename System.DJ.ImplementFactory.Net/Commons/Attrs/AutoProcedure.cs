using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class AutoProcedure : AbsDataInterface
    {
        public AutoProcedure(string sqlExpression): base()
        {
            sql = sqlExpression;
        }

        public AutoProcedure(string sqlExpression, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.IsAsync = isAsync;
        }

        public AutoProcedure(string sqlExpression, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        /// <summary>
        /// 执行 procedure 操作
        /// </summary>
        /// <param name="sqlExpression">执行 procedure 操作的sql语句</param>
        /// <param name="fields">执行 procedure 操作时必须排除的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.IsAsync = isAsync;
        }

        public AutoProcedure(string sqlExpression, string[] fields, FieldsType fieldsType, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        public AutoProcedure(string sqlExpression, string[] fields) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
        }

        public AutoProcedure(string sqlExpression, string[] fields, bool isAsync) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.IsAsync = isAsync;
        }

        public AutoProcedure(string sqlExpression, string[] fields, bool isAsync, int msInterval) : base()
        {
            sql = sqlExpression;
            this.fields = fields;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
        }

        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName, bool isAsync) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
        }

        public AutoProcedure(string dataProviderNamespace, string dataProviderClassName, bool isAsync, int msInterval) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.IsAsync = isAsync;
            MsInterval = msInterval;
        }

    }
}
