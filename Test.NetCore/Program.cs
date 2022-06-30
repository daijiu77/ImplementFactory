using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using Test.NetCore.DataInterface;
using Test.NetCore.Entities;
using static System.DJ.ImplementFactory.NetCore.Commons.Attrs.Condition;

namespace Test.NetCore
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
        public class testJson : AbsDataModel
        {
            [Condition(LogicSign.and, WhereIgrons.igroneEmptyNull)]
            public int key { get; set; }
            public string val { get; set; }
            public void sum<T>(T t)
            {
                //
            }
            public T Generic<T>(List<T> data, T[] arr,  int n)
            {
                return data[n];
            }
        }

        static Func<AbsDataModel, bool> funcCondition = null;
        static void test(AbsDataModel data, Func<AbsDataModel, bool> funcCondition)
        {
            
        }

        static void Main(string[] args)
        {
            SetWindowPositionCenter();

            //TestObj testObj = new TestObj();
            //testObj.test123();
            
            DbVisitor.sqlAnalysis = new SqlServerAnalysis();
            
            testJson tt = new testJson();
            test(tt, dm =>
            {
                Type[] types = dm.GetType().GetGenericArguments();
                Type t = types[0];
                int n = 0;
                return true;
            });
            DbVisitor db = new DbVisitor();
            IDbSqlScheme scheme = db.CreateSqlFrom(LeftJoin.Me.From(tt, "t",
            ConditionItem.Me.And("t.key", ConditionRelation.Equals, new string[] { "a", "b" }),
            ConditionItem.Me.And("t.val",ConditionRelation.Contain,"abc")),
            RightJoin.Me.From(tt, "t1", ConditionItem.Me.And("t.key", ConditionRelation.Equals, "t1.key")));
            scheme.dbSqlBody.Where(ConditionItem.Me.And(ConditionItem.Me.Or("t1.key", ConditionRelation.Equals, "'a1'"),
                ConditionItem.Me.Or("t1.key", ConditionRelation.Equals, "'a2'")));
            string sql = scheme.dbSqlBody.GetSql();
            /**
             * select * from TestJson, Right join TestJson t1 on t.key = t1.key where t.key in ('a', 'b')
             * and t.val like '%abc%' and (t1.key = 'a1' or t1.key = 'a2')
             * **/
            Console.WriteLine("Hello World!");
            Console.ReadKey(true);
        }

        class TestObj: ImplementAdapter
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
