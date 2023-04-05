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
    public class AutoInsert: AbsDataInterface
    {        
        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        public AutoInsert(string insertExpression): base()
        {
            sql = insertExpression;
        }

        public AutoInsert(string insertExpression, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoInsert(string insertExpression, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.sqlExecType = sqlExecType;
        }

        public AutoInsert(string insertExpression, DataOptType sqlExecType, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.sqlExecType = sqlExecType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="fields">执行 insert 操作时必须排除\包含的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        /// <param name="EnabledBuffer">是否启用缓存机制, 默认为 false(不启用)</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="fields">执行 insert 操作时必须排除\包含的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="fields">执行 insert 操作时必须排除的字段名称</param>
        /// <param name="fieldsType">指示字段名称集合类型是 Exclude(排除) 或 Contain(包含), 默认为 Exclude</param>
        public AutoInsert(string insertExpression, string[] fields, FieldsType fieldsType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.fieldsType = fieldsType;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="fields">执行 insert 操作时必须排除的字段名称</param>
        public AutoInsert(string insertExpression, string[] fields) : base()
        {
            sql = insertExpression;
            this.fields = fields;
        }

        public AutoInsert(string insertExpression, string[] fields, bool EnabledBuffer) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.EnabledBuffer = EnabledBuffer;
        }

        /// <summary>
        /// 执行 insert 操作
        /// </summary>
        /// <param name="insertExpression">执行 insert 操作的sql语句</param>
        /// <param name="fields">执行 insert 操作时必须排除的字段名称</param>
        /// <param name="sqlExecType">sql 表达式执行类型</param>
        public AutoInsert(string insertExpression, string[] fields, DataOptType sqlExecType) : base()
        {
            sql = insertExpression;
            this.fields = fields;
            this.sqlExecType = sqlExecType;
        }

        /// <summary>
        /// 指定一个生成sql语句的提供者类, 需要指定该所在的命名空间及类名
        /// </summary>
        /// <param name="dataProviderNamespace">提供sql语句类所在的命名空间</param>
        /// <param name="dataProviderClassName">提供sql语句类的类名</param>
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName) : base()
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
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName, DataOptType sqlExecType) : base()
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
        public AutoInsert(string dataProviderNamespace, string dataProviderClassName, bool EnabledBuffer) : base()
        {
            this.dataProviderNamespace = dataProviderNamespace;
            this.dataProviderClassName = dataProviderClassName;
            this.EnabledBuffer = EnabledBuffer;
        }

    }
}
