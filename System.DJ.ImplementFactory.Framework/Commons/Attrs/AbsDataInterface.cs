using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-08-18
/// </summary>
namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public abstract class AbsDataInterface : AutoCall, IDataOperateAttribute
    {
        private string _sql = "";        
        public string[] fields = null;

        public string dataProviderNamespace = "";
        public string dataProviderClassName = "";

        public FieldsType fieldsType = FieldsType.Exclude;

        public bool EnabledBuffer = false;

        public bool IsAsync = false;

        public string[] ResultExecMethod = null;

        public DataOptType sqlExecType = DataOptType.none;

        public int MsInterval = 0;

        public AbsDataInterface() : base()
        {
            string fn = ""; // this.GetType().Name;
            Type[] types = new Type[]
            {
                typeof(AutoCount),
                typeof(AutoSelect),
                typeof(AutoInsert),
                typeof(AutoUpdate),
                typeof(AutoDelete),
                typeof(AutoProcedure)
            };

            int n = 0;
            Type typeObj = typeof(object);
            object obj = this;
            Type baseType = obj.GetType();
            Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
            dic.Add(baseType, baseType);

            while (5 > n)
            {
                baseType = baseType.BaseType;
                if (baseType.Equals(typeObj)) break;
                dic.Add(baseType, baseType);
                n++;
            }

            foreach (Type item in types)
            {
                typeObj = null;
                dic.TryGetValue(item, out typeObj);

                if (null != typeObj)
                {
                    fn = item.Name;
                    break;
                }
            }

            Regex rg = new Regex("^Auto(?<TagName>[a-z]+)", RegexOptions.IgnoreCase);
            if (rg.IsMatch(fn))
            {
                string TagName = rg.Match(fn).Groups["TagName"].Value.ToLower();
                DataOptType dataOptType = DataOptType.none;
                Enum.TryParse<DataOptType>(TagName, out dataOptType);
                ((IDataOperateAttribute)this).dataOptType = dataOptType;
            }
        }

        public override string ExecuteInterfaceMethodCodeString(MethodInformation method, ref string err)
        {
            string code = "";
            method.dataProviderNamespace = dataProviderNamespace;
            method.dataProviderClassName = dataProviderClassName;
            method.fields = fields;
            method.fieldsType = fieldsType;
            method.ResultExecMethod = ResultExecMethod;
            method.sqlExecType = sqlExecType;

            ExecInterfaceMethodOfCodeStr_DataOpt(method, ((IDataOperateAttribute)this).dataOptType, sql, ref code);

            return code;
        }

        public T ExecuteInterfaceMethod<T>(MethodInformation method, Action<T> action)
        {
            DataOptType dataOptType = ((IDataOperateAttribute)this).dataOptType;
            method.dataProviderNamespace = dataProviderNamespace;
            method.dataProviderClassName = dataProviderClassName;
            method.fields = fields;
            method.fieldsType = fieldsType;
            method.ResultExecMethod = ResultExecMethod;
            method.sqlExecType = sqlExecType;

            string sqlVarName = "sql";
            DynamicCodeAutoCall dynamicCodeAutoCall = new DynamicCodeAutoCall();

            _sql = dynamicCodeAutoCall.ExecReplaceForSqlByFieldName(_sql, sqlVarName, method);

            DynamicCodeChange dynamicCodeChange = new DynamicCodeChange();
            try
            {
                dynamicCodeChange.AnalyzeSql(method, dataOptType, sqlVarName, ref _sql);
            }
            catch (Exception ex)
            {
                e(ex.ToString(), ErrorLevels.severe);
                throw ex;
            }

            DynamicEntity dynamicEntity = new DynamicEntity();
            T result = dynamicEntity.Exec<T>(method, dataOptType, action, _sql);

            return result;
        }

        public string sql
        {
            get { return _sql; }
            set { _sql = value; }
        }

        DataOptType IDataOperateAttribute.dataOptType { get; set; } = DataOptType.none;

    }
}
