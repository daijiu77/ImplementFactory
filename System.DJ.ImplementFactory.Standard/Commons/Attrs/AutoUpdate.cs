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
    public class AutoUpdate: AbsDataInterface
    {

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        public AutoUpdate(string updateExpression) : base()
        {
            sql = updateExpression;
        }

        public AutoUpdate(string updateExpression, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoUpdate(string updateExpression, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.sqlExecType = sqlExecType;
        }

        public AutoUpdate(string updateExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="fields">执行 update 操作时必须排除的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        /// <param name="EnabledBuffer">是否启用缓存机制, 默认为 false(不启用)</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="fields">执行 update 操作时必须排除的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="fields">执行 update 操作时必须排除的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoUpdate(string updateExpression, string[] fields, FieldsType fieldsType, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="fields">执行 update 操作时必须排除的字段名称</param>
        public AutoUpdate(string updateExpression, string[] fields) : base()
        {
            sql = updateExpression;
            this.fields = fields;
        }

        public AutoUpdate(string updateExpression, string[] fields, bool EnabledBuffer) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 update 操作
        /// </summary>
        /// <param name="updateExpression">执行 update 操作的sql语句</param>
        /// <param name="fields">执行 update 操作时必须排除的字段名称</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoUpdate(string updateExpression, string[] fields, DataOptType sqlExecType) : base()
        {
            sql = updateExpression;
            this.fields = fields;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// 指定一个生成sql语句的提供者类, 需要指定该所在的命名空间及类名
        /// </summary>
        /// <param name="dataProviderNamespace">提供sql语句类所在的命名空间</param>
        /// <param name="dataProviderClassName">提供sql语句类的类名</param>
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName) : base()
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
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
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
        public AutoUpdate(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
