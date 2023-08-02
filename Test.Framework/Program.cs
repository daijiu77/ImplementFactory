using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Test.Framework.DataInterface;
using Test.Framework.Entities;
using Test.Framework.InterfaceTest;
using Test.Framework.MSVisitor;

namespace Test.Framework
{
    class Program
    {
        #region 居中显示
        private struct RECT { public int left, top, right, bottom; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);


        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC  
        int nIndex // index of capability  
        );
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        const int HORZRES = 8;
        const int VERTRES = 10;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;
        #endregion

        static Size WorkingArea
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                Size size = new Size();
                size.Width = GetDeviceCaps(hdc, HORZRES);
                size.Height = GetDeviceCaps(hdc, VERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return size;
            }
        }

        /// <summary>
        /// 控制台窗体居中
        /// </summary>
        static void SetWindowPositionCenter()
        {
            IntPtr hWin = GetConsoleWindow();
            RECT rc;
            GetWindowRect(hWin, out rc);

            Size size = WorkingArea;
            Size winSize = new Size(rc.right - rc.left, rc.bottom - rc.top);

            int x = (size.Width - winSize.Width) / 2;
            int y = (size.Height - winSize.Height) / 2;

            MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, true);
        }

        class TestAbc
        {
            private MSVisitor.IUserInfo _mSUserInfo;
            private ICalculate _calculate;
            public TestAbc() { }
            public TestAbc(MSVisitor.IUserInfo mSUserInfo) { _mSUserInfo = mSUserInfo; }
            public TestAbc(MSVisitor.IUserInfo mSUserInfo, ICalculate calculate)
            {
                _mSUserInfo = mSUserInfo;
                _calculate = calculate;
            }

        }

        class ImplAdapter : ImplementAdapter
        {
            //
        }

        static void Main(string[] args)
        {
            ParameterInfo[] paras = null;
            int param_min_count = ImplementAdapter.GetConstructor(typeof(TestAbc), ref paras);
            ImplAdapter implAdapter = new ImplAdapter();
            object testAbs = implAdapter.GetInstanceByType(typeof(TestAbc));
            //Test1 test1 = new Test1();
            //test1.Test();
            //MService.Start();
            int n1 = 1 % 3; //n1=1
            int n2 = 2 % 3; //n2=2
            int n3 = 3 % 3; //n3=0
            //QueryData();
            TestObj testObj = new TestObj();
            //testObj.test_ToObjectFrom();
            //testObj.test_user();
            string un = testObj.VisitService("aa");
            //bool mbool = testObj.Compare();
            //TestDataTableByteArray();

            //testObj = new TestObj();
            //un = testObj.VisitService("bb");

            TableInfoDetail details = UpdateTableDesign.GetTableInfoDetail();
            foreach (TableDetail item in details)
            {
                Trace.WriteLine(item.Name);
                Console.WriteLine(item.Name);
                item.Foreach((fieldName, fieldType, length) =>
                {
                    Trace.WriteLine("\t" + fieldName);
                    Console.WriteLine("\t" + fieldName);
                });
            }

            SetWindowPositionCenter();



            Console.WriteLine("Hello World!");
            Console.ReadKey(true);
        }

        private static void DriverInfo()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            Task.Run(() =>
            {
                while (true)
                {
                    cpuCounter.NextValue();
                    Thread.Sleep(1000);
                    var cpuUsage = cpuCounter.NextValue();
                    string cpuUsageStr = string.Format("{0:f2} %", cpuUsage);
                    var ramAvailable = ramCounter.NextValue();
                    string ramAvaiableStr = string.Format("{0} MB", ramAvailable);
                    Console.WriteLine($"CPU:{cpuUsageStr}   RAM:{ramAvaiableStr}");
                }
            });
        }

        static void QueryData()
        {
            Plan plan = new Plan()
            {
                PName = "Go to play game.",
                Detail = "With my friend",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2)
            };

            DbVisitor db = new DbVisitor();
            IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<Plan>());
            sqlScheme.dbSqlBody.Where(ConditionItem.Me.And("PName", ConditionRelation.Contain, "play"));
            IList<Plan> plans = sqlScheme.ToList<Plan>();

            sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<EquipmentInfo>("e",
                ConditionItem.Me.And("equipmentName", ConditionRelation.Equals, "868b5")));
            IList<EquipmentInfo> equipmentInfos = sqlScheme.ToList<EquipmentInfo>();

            //把 IsUseConstraintLoad 参数设置为 false 时，加载所有关联的子表数据，但不递归加载父级表数据；
            //该参数缺省时，默认为 true，采用数据懒加载
            IDbSqlScheme scheme = db.CreateSqlFrom(false, SqlFromUnit.New.From<WorkInfo>(dm => dm.CompanyName.Equals("HG")));
            scheme.dbSqlBody.Where(ConditionItem.Me.And("CompanyNameEn", ConditionRelation.Contain, "A"));
            IList<WorkInfo> list = scheme.ToList<WorkInfo>();

            if (0 == list.Count) return;
            //在懒加载的情况下，也就是 IsUseConstraintLoad=true 时，访问属性时加载数据
            EmployeeInfo employeeInfo1 = list[0].employeeInfo;

            IList<WorkInfo> workInfos1 = employeeInfo1.WorkInfos;
            if (null != workInfos1)
            {
                //当 IsUseConstraintLoad 设置为 false 时，禁止递归加载数据，所以获取 WorkInfos 属性子元素的 employeeInfo 属性值时将采用懒加载
                EmployeeInfo employeeInfo2 = workInfos1[0].employeeInfo;
            }

            Console.WriteLine(employeeInfo1.ToJson((type, fn) =>
            {
                if (typeof(EmployeeInfo) == type) return false;
                return true;
            }));
        }

        void InsertData1(Plan plan)
        {
            DbVisitor db = new DbVisitor();
            IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From(plan));
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("IsEnabled", false);
            sqlScheme.AppendInsert(dic); //当有默认值的情况下，可以通过 AppendInsert 或 AppendUpdate 改变默认值
        }

        void InsertData(WorkInfo workInfo)
        {
            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(workInfo));
            scheme.dbSqlBody.DataOperateExcludes("id");//Insert操作时排除id字段
            scheme.Insert();
        }

        void UpdateData(WorkInfo workInfo)
        {
            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(workInfo));
            //Update操作时仅对CompanyName，CompanyNameEn字段操作
            scheme.dbSqlBody.DataOperateContains("CompanyName", "CompanyNameEn");
            scheme.Update();
        }

        void DeleteData(WorkInfo workInfo)
        {
            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(workInfo));
            scheme.dbSqlBody.Where(ConditionItem.Me.And("id", ConditionRelation.Equals, workInfo.id));
            scheme.Delete();
        }

        private static void TestDataTableByteArray()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("age", typeof(int));
            dt.Columns.Add("isGirl", typeof(bool));
            dt.Columns.Add("img", typeof(byte[]));
            dt.Columns.Add("phone", typeof(string));
            dt.Columns.Add("qq", typeof(string));
            dt.Columns.Add("email", typeof(string));
            dt.Columns.Add("address", typeof(string));

            DataRow dr = dt.NewRow();
            dr["name"] = "张三";
            dr["age"] = 23;
            dr["isGirl"] = false;
            dr["img"] = Encoding.UTF8.GetBytes("saewrsFgs43243yyrreVw");
            dr["address"] = "广东省";
            dr["qq"] = "565343112";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["name"] = "李丽";
            dr["age"] = 25;
            dr["isGirl"] = true;
            dr["img"] = Encoding.UTF8.GetBytes("sfsfdspp__sss");
            dr["address"] = "广东省123";
            dt.Rows.Add(dr);

            for (int i = 0; i < 20; i++)
            {
                if (0 == (i % 2))
                {
                    dr = dt.NewRow();
                    dr["name"] = "李丽_" + i;
                    dr["age"] = 25;
                    dr["isGirl"] = true;
                    dr["img"] = Encoding.UTF8.GetBytes("sfsfdspp__sss");
                    dr["address"] = "广东省123";
                    dt.Rows.Add(dr);
                }
                else
                {
                    dr = dt.NewRow();
                    dr["name"] = "张三_" + i;
                    dr["age"] = 23;
                    dr["isGirl"] = false;
                    dr["img"] = Encoding.UTF8.GetBytes("saewrsFgs43243yyrreVw");
                    dr["address"] = "广东省";
                    dr["qq"] = "565343112";
                    dt.Rows.Add(dr);
                }
            }

            byte[] data = dt.DataTableToByteArray();
            DataTable dataTable = data.ByteArrayToDataTable();
            int len = data.Length;
        }

        class Test1 : ImplementAdapter
        {
            [MyAutoCall]
            private ICalculate calculate;
            public Test1()
            {
                //
            }

            public void Test()
            {
                int a = 0;
            }
        }

        class TestObj : ImplementAdapter
        {
            [MyAutoCall]
            private IDb_Helper dbHelper1;

            [MyAutoCall]
            private IDb_Helper dbHelper2;

            [MyAutoCall]
            private ICalculate calculate;

            [MyAutoCall]
            private MSVisitor.IUserInfo userInfo2;

            [MyAutoCall]
            private DataInterface.IUserInfo userInfo3;

            [MyAutoCall]
            private IEquipmentInfoMapper equipmentInfoMapper;

            [MyAutoCall]
            private IHomeController homeController;

            public TestObj()
            {
                //
            }

            public bool Compare()
            {
                calculate.Division(1, 2);
                dbHelper1.ConStr = "1";
                dbHelper2.ConStr = "2";
                return dbHelper1.Equals(dbHelper2);
            }

            public void test20220313_1()
            {

                for (int i = 0; i < 50; i++)
                {
                    equipmentInfoMapper.insert(new EquipmentInfo()
                    {
                        height = i + 3,
                        width = i + 1,
                        code = Guid.NewGuid().ToString(),
                        equipmentName = Guid.NewGuid().ToString().Substring(0, 5)
                    });
                }
            }

            public void test123()
            {
                //数据实体属性加 IgnoreField 属性
                EquipmentInfo equipmentInfo = new EquipmentInfo()
                {
                    id = new Guid("5fb92c40-f815-464a-95b0-003740b225ad"),
                    height = 32,
                    width = 20,
                    equipmentName = "text",
                    code = "code123"
                };
                int num = equipmentInfoMapper.update(equipmentInfo);
            }

            public void test232()
            {
                EquipmentInfo equipmentInfo = equipmentInfoMapper.query("5fb92c40-f815-464a-95b0-003740b225ad");
                List<EquipmentInfo> list = equipmentInfoMapper.query(new EquipmentInfo()
                {
                    equipmentName = "868b5"
                });
                int n = list.Count;
                Console.WriteLine("RecordCount: " + n);
            }

            public void test20220313()
            {
                LogicCalculate logicCalculate = new LogicCalculate();
                Console.WriteLine("result: " + logicCalculate.testCalculate());
                Console.WriteLine("");

                UserInfoLogic userInfoLogic = new UserInfoLogic();
                UserInfo userInfo1 = new UserInfo();
                userInfo1.name = "X";
                userInfo1.age = 23;
                /**
                 * 根据属性值动态生成 where 条件获取数据
                 * where name like '%X%' and age=23
                 * **/
                List<UserInfo> ls = userInfoLogic.userInfos(userInfo1);
                Console.WriteLine("Create where of SQL:");
                foreach (UserInfo item in ls)
                {
                    Console.WriteLine("name: " + item.name + ", age: " + item.age + ", address: " + item.address);
                }

                List<UserInfo> userInfos = userInfoLogic.userInfos("53");
                UserInfo userInfo = userInfoLogic.GetLastUserInfo();
                Console.WriteLine("");
                userInfo.ForeachProperty((propertyInfo, propertyType, propertyName, propertyValue) =>
                {
                    propertyValue = null == propertyValue ? "" : propertyValue;
                    Console.WriteLine(propertyName + ": " + propertyValue.ToString());
                });
                Console.WriteLine("");

                Console.WriteLine("Dynamic field:");
                DataEntity<DataElement> dataElements = userInfoLogic.GetDynamicFieldData();
                object val = null;
                foreach (DataElement item in dataElements)
                {
                    val = item.value;
                    val = null == val ? "" : val;
                    Console.WriteLine(item.name + ": " + val.ToString());
                }
                Console.WriteLine("");
            }

            public string VisitService(string msg)
            {
                object vs = homeController.Test();

                string un = userInfo2.UserName("LiShi: " + msg).Result;
                Console.WriteLine("MicroService Visitor: " + un);

                un = userInfo2.UserName("LiShi-123: " + msg).Result;
                Console.WriteLine("MicroService Visitor: " + un);

                un = userInfo2.UserName("LiShi-321: " + msg).Result;
                Console.WriteLine("MicroService Visitor: " + un);
                return un;
            }

            private Task task1()
            {
                return Task.Run(() => { });
            }

            private async Task<Task> task2()
            {
                return Task.Run(() =>
                {
                    int i = 0;
                });
            }

            private async Task<int> task3()
            {
                //return await Task.Run(() => { return 2; });
                return await Task.FromResult(0);
            }

            private Task<int> task4()
            {
                return Task.Run(() => { return 2; });
            }

            public void test_user()
            {
                Type typeList = typeof(DOList<>);
                typeList = typeList.MakeGenericType(typeof(UserInfo));
                string ty = typeList.TypeToString(true);
                string s = "select * from (select top 1 * from UserInfo where name like '%abc%') tb order by age";
                Regex rg1 = new Regex(@"\sorder\s+by\s+((((?!\()(?!\))(?!\sfrom\s)(?!\swhere\s)(?!\sand\s)(?!\sor\s)(?!\slike\s)).)+)$", RegexOptions.IgnoreCase);
                if (rg1.IsMatch(s))
                {
                    s = "";
                }
                DbVisitor db = new DbVisitor();
                IDbSqlScheme scheme2 = db.CreateSqlFrom(SqlFromUnit.Me.From<UserInfo>("ui"),
                    LeftJoin.Me.From<Plan>("p",
                    ConditionItem.Me.And("p.UserInfoId", ConditionRelation.Equals, "ui.Id")));
                scheme2.dbSqlBody.Where(ConditionItem.Me.And("ui.name", ConditionRelation.Equals, "ff1"));
                //int num = scheme2.Delete(true); //当为 true 时，删除与之关联的表数据

                List<UserInfo> _userInfos = (List<UserInfo>)scheme2.ToList<UserInfo>();
                List<Plan> plans = _userInfos[0].Plans as List<Plan>;
                plans[0].num++;
                plans[0].PName = "TTTT-FFF";
                Plan plan = new Plan();
                plan.PName = "plan-123";
                plan.Detail = "Detail-123";
                plan.StartDate = DateTime.Now;
                plan.EndDate = DateTime.Now;
                plan.Id = Guid.NewGuid();
                plans.AddData(plan); //添加与 UserInfo 关联的 Plan 数据
                

                IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From<UserInfo>());
                IDbSqlScheme scheme1 = db.CreateSqlFrom(SqlFromUnit.Me.From<UserInfo>());
                scheme1.dbSqlBody.Where(ConditionItem.Me.And("Age", ConditionRelation.Equals, 21));
                scheme1.dbSqlBody.Select("Age", "age");

                scheme.dbSqlBody.Where(ConditionItem.Me.And("name", ConditionRelation.Contain, "abc"),
                    ConditionItem.Me.AndUnit("Age", ConditionRelation.NotIn, scheme1.dbSqlBody))
                    .Skip(1, 2).Orderby(OrderbyItem.Me.Set("cdatetime", OrderByRule.Asc));
                scheme.dbSqlBody.WhereIgnore("IsEnabled");
                List<UserInfo> users = (List<UserInfo>)scheme.ToList<UserInfo>();
                users[0].address = "test-123"; //向数据库更新 address 属性值

                UserInfo userInfo1 = new UserInfo();
                userInfo1.id = Guid.NewGuid();
                userInfo1.cdatetime = DateTime.Now;
                userInfo1.name = "dj-123";
                userInfo1.address = "SZ-GDong";
                users.AddData(userInfo1); //向数据库新增一条数据
                IList<UserInfo> children = users[1].children;

                IList<UserInfo> userInfos1 = users.ToListFrom<UserInfo, UserInfo>((tg, src, fn, fv) =>
                {
                    return fv;
                });
                int ncount = scheme.Count();
                int recordCount = scheme.RecordCount;
                int pageCount = scheme.PageCount;

                Console.WriteLine("Please input [ok]:");
                string msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) msg = "";
                if (!msg.ToLower().Equals("ok")) return;

                List<UserInfo> uiList = userInfo3.query<UserInfo>("abc", 1, 5, 1).Result;
                uiList = userInfo3.query<UserInfo>("abc", 1, 5, 1).Result;
                uiList = userInfo3.query<UserInfo>("abc", 1, 5, 1).Result;
                uiList = userInfo3.query<UserInfo>("abc", 1, 5, 1).Result;

                char[] arr = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
                int size = arr.Length - 1;
                Random rnd = new Random();
                UserInfo userInfo = null;
                for (int i = 0; i < 5; i++)
                {
                    userInfo = new UserInfo()
                    {
                        id = Guid.NewGuid(),
                        name = "LS-abc-" + arr[rnd.Next(0, size)],
                        age = 13,
                        address = "Szsf_" + arr[rnd.Next(0, size)],
                        userType = UserType.TOP,
                        cdatetime = DateTime.Now
                    };
                    userInfo3.insert(userInfo);
                }

                List<UserInfo> userInfos = userInfo3.query(new UserInfo()
                {
                    name = "SZ"
                });
            }

            public void test_ToObjectFrom()
            {
                Plan plan = new Plan()
                {
                    PName = "abc",
                    plan = new Plan() { PName = "abc-123" },
                    plans = new List<Plan>()
                    {
                        new Plan()
                        {
                            PName="abc-321",
                            plan=new Plan(){PName="abc-321-01"}
                        },
                        new Plan()
                        {
                            PName="abc-567",
                            plan=new Plan(){PName="abc-567-01"}
                        }
                    }
                };

                PlanCopy plan1 = plan.ToObjectFrom<PlanCopy>();
                string pName = plan1.PName;
            }

            public class PlanCopy
            {
                public string PName { get; set; }
                public string Detail { get; set; }
                public DateTime StartDate { get; set; }
                public DateTime EndDate { get; set; }

                public PlanCopy plan { get; set; }
                public List<PlanCopy> plans { get; set; }
            }
        }

    }
}
