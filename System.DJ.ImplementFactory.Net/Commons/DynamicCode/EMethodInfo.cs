using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    internal class EMethodInfo : MethodInfo
    {
        private Type _declaringType = null;
        private Type _returnType = null;
        private bool _IsGenericMethod = false;
        private bool _isAsyncReturn = false;
        private bool _isTaskReturn = false;
        private string _name = "";
        private List<Attribute> _attributes = new List<Attribute>();
        private List<ParameterInfo> _parameterInfos = new List<ParameterInfo>();
        private List<CustomAttributeData> _CustomAttributeDatas = new List<CustomAttributeData>();
        private List<Type> _GenericArguments = new List<Type>();

        public override Type DeclaringType => _declaringType;

        public override string Name => _name;

        public override Type ReturnType => _returnType;

        public override bool IsGenericMethod => _IsGenericMethod;

        public override IEnumerable<CustomAttributeData> CustomAttributes => _CustomAttributeDatas;

        public bool IsAsyncReturn { get { return _isAsyncReturn; } }

        public bool IsTaskReturn { get { return _isTaskReturn; } }

        public EMethodInfo SetCustomAttributeDatas(IEnumerable<CustomAttributeData> customAttributeDatas)
        {
            if (null == customAttributeDatas) return this;
            foreach (CustomAttributeData customData in customAttributeDatas)
            {
                _CustomAttributeDatas.Add(customData);
            }

            if (null != _returnType)
            {
                Type rtnType = null;
                JudgeTaskMethod(this, ref _isAsyncReturn, ref _isTaskReturn, ref rtnType);
                _returnType = rtnType;
            }
            return this;
        }

        public EMethodInfo SetIsGenericMethod(bool isGenericMethod)
        {
            _IsGenericMethod = isGenericMethod;
            return this;
        }

        public EMethodInfo SetReturnType(Type returnType)
        {
            _returnType = returnType;
            Type rtnType = null;
            JudgeTaskMethod(this, ref _isAsyncReturn, ref _isTaskReturn, ref rtnType);
            _returnType = rtnType;
            return this;
        }

        public EMethodInfo SetDeclaringType(Type declaringType)
        {
            _declaringType = declaringType;
            return this;
        }

        public EMethodInfo SetName(string name)
        {
            _name = name;
            return this;
        }

        public override Type[] GetGenericArguments()
        {
            return _GenericArguments.ToArray();
        }

        public EMethodInfo SetGenericArguments(Type[] genericArguments)
        {
            if (null == genericArguments) return this;
            foreach (var item in genericArguments)
            {
                _GenericArguments.Add(item);
            }
            return this;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _attributes.ToArray();
            //throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            List<object> attributeList = new List<object>();
            foreach (var item in _attributes)
            {
                if (attributeType.IsAssignableFrom(item.GetType()) || attributeType == item.GetType())
                {
                    attributeList.Add(item);
                }
            }
            return attributeList.ToArray();
        }

        public EMethodInfo SetCustomAttributes(object[] attributes)
        {
            if (null == attributes) return this;
            foreach (var item in attributes)
            {
                this._attributes.Add((Attribute)item);
            }
            return this;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _parameterInfos.ToArray();
        }

        public EMethodInfo SetParameters(ParameterInfo[] parameters)
        {
            if (null == parameters) return this;
            foreach (var item in parameters)
            {
                _parameterInfos.Add(item);
            }
            return this;
        }

        /// <summary>
        /// 判断是否是 async Task 方法或 Task 方法，及返回方法值类型
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="isAsyncReturn">Task 方法中是否含有 async 标识</param>
        /// <param name="isTaskReturn">是否是 Task 方法</param>
        /// <param name="return_type">方法返回值类型</param>
        private void JudgeTaskMethod(MethodInfo mi, ref bool isAsyncReturn, ref bool isTaskReturn, ref Type return_type)
        {
            isAsyncReturn = false;
            isTaskReturn = false;
            return_type = typeof(void);
            if (0 < mi.CustomAttributes.Count())
            {
                /**
                 * 判断方法是否是 async Task 方法:
                 * public async Task UpdateInfo(Guid Id, CustomerInfo) { }
                 * **/
                #region 判断方法是否是 async Task 方法
                IEnumerable<CustomAttributeData> attrs = mi.CustomAttributes;
                Type attrType = null;
                List<Type> listTypes = new List<Type>();
                listTypes.Add(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute));
                listTypes.Add(typeof(System.Diagnostics.DebuggerStepThroughAttribute));
                int n = 0;
                foreach (CustomAttributeData item in attrs)
                {
                    attrType = item.AttributeType;
                    if (listTypes.Contains(attrType)) n++;
                }

                if (2 == n)
                {
                    isAsyncReturn = true;
                    isTaskReturn = true;
                }
                #endregion
            }

            Type rtnType = mi.ReturnType;
            if (false == isTaskReturn) isTaskReturn = -1 != rtnType.Name.ToLower().IndexOf("task");
            if (isTaskReturn)
            {
                /**
                 * 如果是 Task 方法, 判断 Task 是否带有参数:
                 * public Task<bool> UpdateInfo(Guid Id, CustomerInfo) { }
                 * return_type = bool类型
                 * **/
                Type[] tys = rtnType.GetGenericArguments();
                if (0 < tys.Length) return_type = tys[0];
            }
            else
            {
                return_type = mi.ReturnType;
            }
        }

        #region throw new NotImplementedException();
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

        public override MethodAttributes Attributes => throw new NotImplementedException();

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
