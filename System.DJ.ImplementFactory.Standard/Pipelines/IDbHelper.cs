using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Entities;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDbHelper: IUnSingleInstance
    {
        string connectString { get; set; }

        IDbConnectionState dbConnectionState { get; set; }

        IDataServerProvider dataServerProvider { get; set; }

        /// <summary>
        /// insert/update/delete 批量操作最大数量, 默认100条数据
        /// </summary>
        int optByBatchMaxNumber { get; set; }

        /// <summary>
        /// insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒
        /// </summary>
        int optByBatchWaitSecond { get; set; }

        /// <summary>
        /// insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000
        /// </summary>
        int sqlMaxLengthForBatch { get; set; }

        /// <summary>
        /// 释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接)
        /// </summary>
        bool disposableAndClose { get; set; }

        /// <summary>
        /// 是否是常规的批量插入
        /// </summary>
        bool isNormalBatchInsert { get; set; }

        /// <summary>
        /// 分表匹配规则
        /// </summary>
        string splitTablesRule { get; set; }

        /// <summary>
        /// 分表时单表最大记录量
        /// </summary>
        long splitTablesRecordQuantity { get; set; }
        
        /// <summary>
        /// 测试数据库连接状态
        /// </summary>
        /// <param name="dataServerProvider"></param>
        /// <param name="connectionString"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        bool TestDbConnectionState(IDataServerProvider dataServerProvider, string connectionString, ref string err);

        DataTable query(object autoCall, string sql, DataPage dataPage, bool isDataPage, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);
        DataTable query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);
        DataTable query(object autoCall, string sql, DataPage dataPage, bool isDataPage, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);
        DataTable query(object autoCall, string sql, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);
        int insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);
        int update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);
        int delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);
    }
}
