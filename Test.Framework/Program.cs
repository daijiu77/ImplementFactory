using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Test.Framework.DataInterface;
using Test.Framework.Entities;
using static System.DJ.ImplementFactory.NetCore.Commons.Attrs.Condition;

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

        [Table("TestJson")]
        class testJson : AbsDataModel
        {
            public string key1 { get; set; }
            [Condition(LogicSign.and, WhereIgrons.igroneEmptyNull)]
            public virtual string key { get; set; }
            public virtual string val { get; set; }
            public virtual List<string> children { get; set; }

            public virtual Json2 json2 { get; set; }
        }

        class Json1: AbsDataModel
        {
            public string field1 { get; set; }
            public int field2 { get; set; }

            public testJson tjson { get; set; }
        }

        class Json2 : AbsDataModel
        {
            public string field3 { get; set; }
            public int field4 { get; set; }
            public Json1 json1 { get; set; }
        }

        class TTJson : testJson
        {

            public override string key { get => base.key; set => base.key = value; }
            public override string val { get => base.val; set => base.val = value; }
            public override List<string> children
            {
                get
                {
                    return base.children;
                }
                set
                {
                    base.children = value;
                }
            }
        }

        static void Main(string[] args)
        {
            Task task = Task.Run(() =>
              {
                  int n = 0;
                  while (n < 50)
                  {
                      n++;
                      Trace.WriteLine("Task print: " + n.ToString());
                      Thread.Sleep(500);
                  }
              });

            Task task1 = Task.Run(() =>
            {
                int n = 0;
                while (n < 10)
                {
                    n++;
                    Thread.Sleep(600);
                    Trace.WriteLine("Task-1 print: " + n.ToString());
                }
            });

            List<Task> tasks = new List<Task>();
            tasks.Add(task);
            tasks.Add(task1);
            Task.WaitAll(tasks.ToArray());
            Json1 json1 = new Json1()
            {
                field1 = "1",
                field2 = 2
            };

            testJson tjson = new testJson()
            {
                key = "key",
                val = "value",
                children = new List<string>()
            };
            json1.tjson = tjson;

            tjson.children.Add("a");
            tjson.children.Add("b");

            Json2 json2 = new Json2()
            {
                field3 = "f3",
                field4 = 4,
                json1 = json1
            };

            tjson.json2 = json2;

            string json = json1.ToJson();

            SetWindowPositionCenter();

            Plan plan = new Plan()
            {
                PName = "Go to play game.",
                Detail = "With my friend",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2)
            };

            DbVisitor db = new DbVisitor();
            IDbSqlScheme sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From(plan));
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("IsEnabled", false);
            sqlScheme.AppendInsert(dic); //当有有默认值的情况下，可以通过 AppendInsert 或 AppendUpdate 改变默认值
            IList<Plan> plans = sqlScheme.ToList<Plan>();

            sqlScheme = db.CreateSqlFrom(SqlFromUnit.Me.From<EquipmentInfo>("e",
                ConditionItem.Me.And("equipmentName", ConditionRelation.Equals, "868b5")));
            IList<EquipmentInfo> equipmentInfos = sqlScheme.ToList<EquipmentInfo>();

            //把 IsUseConstraintLoad 参数设置为 false 时，加载所有关联的子表数据，但不递归加载父级表数据；
            //该参数缺省时，默认为 true，采用数据懒加载
            IDbSqlScheme scheme = db.CreateSqlFrom(false, SqlFromUnit.New.From<WorkInfo>(dm => dm.CompanyName.Equals("HG")));
            scheme.dbSqlBody.Where(ConditionItem.Me.And("CompanyNameEn", ConditionRelation.Contain, "A"));

            IList<WorkInfo> list = scheme.ToList<WorkInfo>();

            //在懒加载的情况下，也就是 IsUseConstraintLoad=true 时，访问属性时加载数据
            EmployeeInfo employeeInfo1 = list[0].employeeInfo;
                        
            IList<WorkInfo> workInfos1 = employeeInfo1.WorkInfos;
            if(null != workInfos1)
            {
                //当 IsUseConstraintLoad 设置为 false 时，禁止递归加载数据，所以获取 WorkInfos 属性子元素的 employeeInfo 属性值时将采用懒加载
                EmployeeInfo employeeInfo2 = workInfos1[0].employeeInfo;
            }

            Console.WriteLine(employeeInfo1.ToJson((type, fn) =>
            {
                if (typeof(EmployeeInfo) == type) return false;
                return true;
            }));
            Console.WriteLine("Hello World!");
            Console.ReadKey(true);
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

        class TestObj : ImplementAdapter
        {
            [MyAutoCall]
            private IEquipmentInfoMapper equipmentInfoMapper;

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
                MethodInfo[] ms = typeof(testJson).GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodInfo item in ms)
                {
                    if (!item.IsGenericMethod) continue;
                    ParameterInfo[] ps = item.GetParameters();
                    ParameterInfo pi = ps[0];
                    Type tp = pi.ParameterType;
                    Type[] ts = tp.GetGenericArguments();
                    string ns = pi.ParameterType.Namespace;
                    bool mbool = item.ReturnType.IsGenericType;
                    string name = pi.Name;
                }

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

            ~TestObj()
            {
                Trace.WriteLine("TestObj distory");
            }
        }

    }
}
