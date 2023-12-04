namespace System.DJ.ImplementFactory.Entities
{
    public enum RuleType
    {
        none,
        DbInfo,
        MatchRule,
        LogsRange,
        SysConfig
    }

    public enum DbConnectionFreeStrategy
    {
        onlyDispose,
        disposeAndClose
    }

    public enum InsertBatchStrategy
    {
        /// <summary>
        /// 正常的批量 insert into tablename(fieldList) values(valueList1),(valueList2),(valueList3)
        /// </summary>
        normalBatch,
        /// <summary>
        /// 独立的列表 insert into tablename(fieldList) values(valueList1);insert into tablename(fieldList) values(valueList2)
        /// </summary>
        singleList
    }

    public class DbInfo
    {
        private string _ConnectionString = "数据库连接字符串";
        public string ConnectionString { get; set; } = "Data Source=(local);Initial Catalog=DatabaseName;User Id=sa;Password=sa;Encrypt=True;TrustServerCertificate=True;";

        private string _DatabaseType = "数据库类型:sqlserver,oracle,mysql";
        public string DatabaseType { get; set; } = "sqlserver";

        private string _SqlProviderRelativePathOfDll = "动态 sql 提供者所在的dll文件程序集的相对路径,注：该提供者必须继承 System.DJ.ImplementFactory.Pipelines.ISqlExpressionProvider 接口";
        public string SqlProviderRelativePathOfDll { get; set; }

        private string _optByBatchMaxNumber = "insert/update/delete 批量操作最大数量, 默认100条数据";
        /// <summary>
        /// insert/update/delete 批量操作最大数量, 默认100条数据
        /// </summary>
        public int optByBatchMaxNumber { get; set; } = 100;

        private string _optByBatchWaitSecond = "insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒";
        /// <summary>
        /// insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒
        /// </summary>
        public int optByBatchWaitSecond { get; set; } = 3;

        private string _sqlMaxLengthForBatch = "insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000";
        /// <summary>
        /// insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000
        /// </summary>
        public int sqlMaxLengthForBatch { get; set; } = 50000;

        private string _close = "释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接)";
        /// <summary>
        /// 释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接)
        /// </summary>
        public bool close { get; set; } = false;

        private string _dbConnectionFreeStrategy = "数据库连接释放策略，onlyDispose/disposeAndClose 或整数值: 0/1, close 属性的补充，此属性优先级高于 close 属性";
        /// <summary>
        /// 数据库连接释放策略
        /// </summary>
        public DbConnectionFreeStrategy dbConnectionFreeStrategy { get; set; } = DbConnectionFreeStrategy.onlyDispose;

        private string _insertBatchStrategy = "缓存时批量插入策略，normalBatch/singleList 或整数值: 0/1, 默认采用通用批量插入";
        /// <summary>
        /// 批量插入策略
        /// </summary>
        public InsertBatchStrategy insertBatchStrategy { get; set; } = InsertBatchStrategy.normalBatch;

        private string _IsShowCode = "是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]";
        /// <summary>
        /// 是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]
        /// </summary>
        public bool IsShowCode { get; set; }

        private string _IsPrintSQLToTrace = "输出sql语句到Visual Studio输出台";
        /// <summary>
        /// 输出sql语句到Visual Studio输出台
        /// </summary>
        public bool IsPrintSQLToTrace { get; set; }

        private string _IsPrintSqlToLog = "输出sql语句到日志";
        /// <summary>
        /// 输出sql语句到日志
        /// </summary>
        public bool IsPrintSqlToLog { get; set; }

        private string _MakeInsertSqlMaxRecordSize = "把数据生成 insert-sql 时，规定的最大数据量，默认为 1000";
        public int MakeInsertSqlMaxRecordSize { get; set; } = 1000;

        private string _MakeInsertSqlLocalPath = "把数据生成 insert-sql 并保存到指定目录下";
        public string MakeInsertSqlLocalPath { get; set; }

        /// <summary>
        /// 分表存储及查询
        /// </summary>
        public SplitTable splitTable { get; set; } = new SplitTable();

        private string _IsDbUsed = "当设置为 true 时，每次使用完 DbConnection 都会被释放";
        public bool IsDbUsed { get; set; }

        private string _UpdateTableDesign = "是否开启更新数据表设计";
        public bool UpdateTableDesign { get; set; }

        private string _CacheTime_Second = "数据缓存生命周期, 默认为1小时,即: 3600秒";
        public int CacheTime_Second { get; set; } = 3600;

        private string _PersistenceCylceSync_Second = "缓存持久化周期同步间隔，默认10秒";
        public int PersistenceCylceSync_Second { get; set; } = 10;

        private string _PersistenceSource = "数据持久化源，可以是：db 或 file";
        public string PersistenceSource { get; set; } = "file";

        private string _IsPrintFilterIPToLogs = "把网关拦截的 IP 输出到 Logs 文件夹";
        public bool IsPrintFilterIPToLogs { get; set; }

        private string _TryTimeServiceRegister = "服务注册尝试次数，默认50次，当值为0(零)时执行无限次数尝试";
        public int TryTimeServiceRegister { get; set; } = 50;

        private string _HttpTimeout_Second = "Http请求等待时长, 默认30秒";
        public int HttpTimeout_Second { get; set; } = 30;
    }
}
