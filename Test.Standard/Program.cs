using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Reflection;
using Test.Standard.DataInterface;
using Test.Standard.Entities;
using static System.DJ.ImplementFactory.Commons.Attrs.Condition;

namespace Test.Standard
{
    class Program
    {
        static void Main(string[] args)
        {
            TestObj testObj = new TestObj();
            testObj.test20220313();
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }

        public class testJson
        {
            [Condition(LogicSign.and, WhereIgrons.igroneEmptyNull)]
            public int key { get; set; }
            public string val { get; set; }
            public void sum<T>(T t)
            {
                //
            }
            public T Generic<T>(List<T> data, T[] arr, int n)
            {
                return data[n];
            }
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
