# ImplementFactory
System.DJ.ImplementFactory

In c# program development, the ImplementFactory component provides a good solution for decoupling between business levels. You simply
The implementation of the code can release the close association of the code between the modules. The ImplementFactory component emphasizes that each business module is a unique
As an individual, the loading of interface instances is done by the ImplementFactory component.

Using ImplementFactory component can easily realize the automatic assembly of interface instances, and query, add,
Modify and delete operations.

In the process of using the ImplementFactory component, create a subclass that inherits the AutoCall class, which can be convenient for each interface and interface
The mouth method performs effective control (AOP) and exception interception processing.

    public class myAutoCall: AutoCall
    {

        //Called before executing the interface instance method
        public override bool ExecuteBeforeFilter(Type interfaceType, object implement, string methodName,
          PList<Para> paras)
        {
	 //If it returns false, the interface method is not executed
 	return true;
        }
        //Called after executing the interface instance method
        public override bool ExecuteAfterFilter(Type interfaceType, object implement, string methodName,
         PList<Para> paras, object result)
        {
 	//If it returns false, the result of executing the interface method will not be returned to the caller
 	return true;
        }
        //Intercept all exceptions that occur on all interface instances
        public override void ExecuteExcption(Type interfaceType, object implement, string methodName,
         PList<Para> paras, Exception ex)
        {
 	base.ExecuteExcption(interfaceType, implement, methodName, paras, ex);
        }
    }

How to use myAutoCall?

    class TestUnit : ImplementAdapter
    {

        [myAutoCall]
        IMixedJson mixedJson;
        public DJsonItem GetStudentInfoByName(string name)
        {
 	return mixedJson.StudentInfoJsonItem(name);
       }
    }

By inheriting the AutoCall class, it is very convenient to implement the AOP mechanism, and use the AOP mechanism to intercept any relevant interface instance and interface
method.

At the same time, for data operations, you can choose the data adapter provided by ImplementFactory, if your business has special
Requirements, you can also choose to provide an effective data source adapter yourself, and you only need to implement the IDataServerProvider interface to
ImplementFactory provides a valid data adapter.This instance has the highest priority and it will be automatically loaded by the system.

    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.DJ.ImplementFactory.Pipelines;
    using System.Text;

    public class MySqlDataServerProvider : IDataServerProvider
    {
        DataAdapter IDataServerProvider.CreateDataAdapter(DbCommand dbCommand)
        {
            return new MySqlDataAdapter((MySqlCommand)dbCommand);
        }

        DbCommand IDataServerProvider.CreateDbCommand(string sql, DbConnection connection)
        {
            return new MySqlCommand(sql, (MySqlConnection)connection);
        }

        DbCommand IDataServerProvider.CreateDbCommand()
        {
            return new MySqlCommand();
        }

        DbConnection IDataServerProvider.CreateDbConnection(string connectString)
        {
            return new MySqlConnection(connectString);
        }

        DbParameter IDataServerProvider.CreateDbParameter(string fieldName, object fieldValue)
        {
            return new MySqlParameter(fieldName, fieldValue);
        }
    }

After declaring the implementation class of the IDataServerProvider interface as public, the component will automatically load and enable the interface instance.

The interface-oriented operation data source mechanism of the ImplementFactory component:

    public interface IUserInfoMapper
    {

        [AutoSelect("select * from UserInfo where name like '%{name}%'")]
        List<UserInfo> GetUserInfosByName(UserInfo userInfo);

        [AutoSelect("select * from UserInfo where name=@name and age=@age")]
        UserInfo GetUserInfoByNameAndAge(UserInfo userInfo);

        [AutoSelect(dataProviderNamespace: "DySqlProvider",
            dataProviderClassName: "UserInfoSqlProvider")]
        List<UserInfo> GetUserInfosBySqlProvider(UserInfo userInfo);

        [AutoSelect("select * from UserInfo")]
        DataTable GetUserInfos();

        [AutoSelect("select * from UserInfo")]
        List<UserInfo> GetUserInfos1();

        [AutoInsert(insertExpression: "if not exists(select * from UserInfo where name=@name and age=@age) begin insert into UserInfo values({userInfos}) end",
        fields: new string[] { "id", "createdate" }, fieldsType: FieldsType.Exclude)]
        int InsertUserInfo(List<UserInfo> userInfos);

        [AutoInsert("if not exists(select * from UserInfo where name=@name and age=@age) begin insert into UserInfo(name,age,address) values(@name,@age,@address) end")]
        int InsertUserInfo(DataTable dataTable);

        [AutoUpdate(updateExpression: "update UserInfo set {userInfos} where id=@id",
            fields: new string[] { "address" }, fieldsType: FieldsType.Contain)]
        int UpdateUserInfo(List<UserInfo> userInfos);

        [AutoDelete("delete from UserInfo where id=@id")]
        int DeleteUserInfo(List<UserInfo> userInfos);
    }

public class TestUnit : ImplementAdapter, ITestUnit
{

    [myAutoCall]
    IUserInfoMapper UserInfoMapper;

    List<UserInfo> ITestUnit.GetUserInfosByName()
    {
        List<UserInfo> userInfos = UserInfoMapper.GetUserInfosByName(new UserInfo() { name = "Jim" });
        return userInfos;
    }

    int ITestUnit.InsertUserInfo()
    {
        List<UserInfo> userInfos = new List<UserInfo>();
        UserInfo userInfo = new UserInfo();
        userInfo.name = "Wang";
        userInfo.age = 21;
        userInfo.address = "China";
        userInfos.Add(userInfo);

        userInfo = new UserInfo();
        userInfo.name = "Yang";
        userInfo.age = 21;
        userInfo.address = "China";
        userInfos.Add(userInfo);

        return UserInfoMapper.InsertUserInfo(userInfos);
    }

    Random rnd = new Random();
    int ITestUnit.UpdateUserInfo()
    {
        List<UserInfo> userInfos = new List<UserInfo>();
        UserInfo userInfo = new UserInfo();
        userInfo.id = 1;
        userInfo.name = "Wang";
        userInfo.age = 21;
        userInfo.address = "China-" + rnd.Next(1, 10).ToString("D2");
        userInfos.Add(userInfo);

        int num = UserInfoMapper.UpdateUserInfo(userInfos);
        return num;
    }

    int ITestUnit.DeleteUserInfo()
    {
        List<UserInfo> userInfos = new List<UserInfo>();
        UserInfo userInfo = new UserInfo();
        userInfo.id = 2;
        userInfos.Add(userInfo);

        userInfo = new UserInfo();
        userInfo.id = 3;
        userInfos.Add(userInfo);

        int num = UserInfoMapper.DeleteUserInfo(userInfos);
        return num;
    }
}

By default, AutoCall is a mechanism that uses instance references.

namespace TestModule01

       public class TestCls: ImplementFactory
              [AutoCall]
              ILogin login;

namespace TestModule02

       public class TestCls: ImplementFactory
              [AutoCall]
              ILogin login;

TestModule01.TestCls.login == TestModule02.TestCls.login    ----> true

According to actual needs, you can also use the instance non-reference mechanism, and this only needs to add SingleCall on the basis of AutoCall:

namespace TestModule01

       public class TestCls: ImplementFactory
              [AutoCall, SingleCall]
              ILogin login;

namespace TestModule02

       public class TestCls: ImplementFactory
              [AutoCall, SingleCall]
              ILogin login;

TestModule01.TestCls.login == TestModule02.TestCls.login    ----> false

ImplementFactory components are automatically scanned for interface type members, automatically assemble interface instances for interface type members, and face interface operation data Source, implementation interface instance loading interception, interface method call interception, and corresponding exception interception provide an effective solution.
