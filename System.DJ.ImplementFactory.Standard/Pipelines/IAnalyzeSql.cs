using System.DJ.ImplementFactory.Pipelines.Pojo;
/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IAnalyzeSql
    {
        /// <summary>
        /// 执行方法前
        /// </summary>
        /// <param name="method"></param>
        /// <param name="dataOptType"></param>
        /// <param name="sqlVarName"></param>
        /// <param name="sql"></param>
        void ExecuteMethodBefore(MethodInformation method, DataOptType dataOptType, string sqlVarName, ref string sql);

        /// <summary>
        /// 正在执行接口方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paraList"></param>
        /// <param name="dataOptType"></param>
        /// <param name="sqlVarName"></param>
        /// <param name="sql"></param>
        void ExecutingMethod(MethodInformation method, PList<Para> paraList, DataOptType dataOptType, string sqlVarName, ref string sql);
    }
}
