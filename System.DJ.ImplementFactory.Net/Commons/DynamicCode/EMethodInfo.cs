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
        private MethodInfo _mi = null;
        private Type _declaringType = null;
        private Type _returnType = null;
        private Type _implementType = null;
        private bool _IsGenericMethod = false;
        private bool _isAsyncReturn = false;
        private bool _isTaskReturn = false;
        private bool _isJudgeExc = false;
        private string _name = "";
        private ParameterInfo[] _parameterInfos = null;
        private List<CustomAttributeData> _CustomAttributeDatas = null;
        private Type[] _GenericArguments = null;

        public EMethodInfo(MethodInfo interfaceMethod)
        {
            _isJudgeExc = false;
            _mi = interfaceMethod;
            _name = interfaceMethod.Name;
            _declaringType = interfaceMethod.DeclaringType;
            _returnType = interfaceMethod.ReturnType;
            _IsGenericMethod = interfaceMethod.IsGenericMethod;
            _parameterInfos = interfaceMethod.GetParameters();
            _GenericArguments = interfaceMethod.GetGenericArguments();
        }

        public override Type DeclaringType => _declaringType;

        public override string Name => _name;

        public override Type ReturnType
        {
            get
            {
                JudgeTaskMethod(_mi, ref _isAsyncReturn, ref _isTaskReturn, ref _returnType);
                return _returnType;
            }
        }

        public override bool IsGenericMethod => _IsGenericMethod;

        public override IEnumerable<CustomAttributeData> CustomAttributes
        {
            get
            {
                if (null == _CustomAttributeDatas)
                {
                    _CustomAttributeDatas = new List<CustomAttributeData>();
                    var attrDatas = _mi.CustomAttributes;
                    if (null != attrDatas)
                    {
                        foreach (var attrData in attrDatas)
                        {
                            _CustomAttributeDatas.Add(attrData);
                        }
                    }

                    MethodInfo implMethod = GetImplementMethodBy(_mi, _implementType);
                    if (null != implMethod)
                    {
                        attrDatas = implMethod.CustomAttributes;
                        if (null != attrDatas)
                        {
                            foreach (var attrData in attrDatas)
                            {
                                _CustomAttributeDatas.Add(attrData);
                            }
                        }
                    }
                }
                return _CustomAttributeDatas;
            }
        }

        public bool IsAsyncReturn
        {
            get
            {
                JudgeTaskMethod(_mi, ref _isAsyncReturn, ref _isTaskReturn, ref _returnType);
                return _isAsyncReturn;
            }
        }

        public bool IsTaskReturn
        {
            get
            {
                JudgeTaskMethod(_mi, ref _isAsyncReturn, ref _isTaskReturn, ref _returnType);
                return _isTaskReturn;
            }
        }

        public Type ImplementType { get { return _implementType; } }

        public EMethodInfo SetImplementType(Type implType)
        {
            _implementType = implType;
            return this;
        }

        public override Type[] GetGenericArguments()
        {
            return _GenericArguments;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return WholeAttributes(null, inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return WholeAttributes(attributeType, inherit);
        }

        public object[] WholeAttributes(Type attributeType, bool inherit)
        {
            object[] attributes = _mi.GetCustomAttributes(attributeType, inherit);
            MethodInfo implMethod = GetImplementMethodBy(_mi, _implementType);
            if (null != implMethod)
            {
                Dictionary<string, object> attrsDic = new Dictionary<string, object>();
                string key = "";
                if (null != attributes)
                {
                    foreach (var attr in attributes)
                    {
                        key = attr.GetType().FullName;
                        if (attrsDic.ContainsKey(key)) attrsDic.Remove(key);
                        attrsDic.Add(key, attr);
                    }
                }

                object[] attrs = implMethod.GetCustomAttributes(attributeType, inherit);
                if (null != attrs)
                {
                    foreach (var attr in attrs)
                    {
                        key = attr.GetType().FullName;
                        if (attrsDic.ContainsKey(key)) attrsDic.Remove(key);
                        attrsDic.Add(key, attr);
                    }
                }

                if (0 < attrsDic.Count)
                {
                    attributes = new object[attrsDic.Count];
                    int n = 0;
                    foreach (var attr in attrsDic)
                    {
                        attributes[n] = attr.Value;
                        n++;
                    }
                }
            }
            return attributes;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _parameterInfos;
        }

        public List<CustomAttributeData> CustomAttributeDatas { get; private set; }

        public MethodInfo GetImplementMethodBy(MethodInfo interfaceMethod, Type implementClass)
        {
            MethodInfo implMethod = null;
            if ((null == interfaceMethod) || (null == implementClass)) return implMethod;
            Type interfaceClassType = interfaceMethod.DeclaringType;
            if (!interfaceClassType.IsAssignableFrom(implementClass))
            {
                string err = "Type: <{0}> is not an implementation class for interface type: <{1}>".ExtFormat(implementClass.FullName, interfaceClassType.FullName);
                throw new Exception(err);
            }
            ParameterInfo[] paras = interfaceMethod.GetParameters();
            int paraSize = paras.Length;

            string methodName = interfaceMethod.Name;
            string methodName1 = interfaceMethod.DeclaringType.FullName + "." + methodName;
            bool mbool = false;
            int n = 0;
            string mName = "";
            ParameterInfo[] paras1 = null;
            MethodInfo[] implMs = implementClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo impl in implMs)
            {
                mbool = false;
                mName = impl.Name;
                if (mName.Equals(methodName) || mName.Equals(methodName1))
                {
                    paras1 = impl.GetParameters();
                    if (paras1.Length == paraSize)
                    {
                        mbool = true;
                        n = 0;
                        foreach (ParameterInfo item in paras1)
                        {
                            if (item.ParameterType != paras[n].ParameterType)
                            {
                                mbool = false;
                                break;
                            }
                            n++;
                        }
                    }
                }
                if (mbool)
                {
                    implMethod = impl;
                    break;
                }
            }
            return implMethod;
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
            if (_isJudgeExc) return;
            _isJudgeExc = true;
            isAsyncReturn = false;
            isTaskReturn = false;
            return_type = typeof(void);
            /**
                * 判断方法是否是 async Task 方法:
                * public async Task UpdateInfo(Guid Id, CustomerInfo) { }
                * **/
            #region 判断方法是否是 async Task 方法
            IEnumerable<CustomAttributeData> attrs = null;
            if (0 < mi.CustomAttributes.Count())
            {
                attrs = mi.CustomAttributes;
            }

            MethodInfo implM = GetImplementMethodBy(_mi, _implementType);
            if (null != implM)
            {
                var attrs1 = implM.CustomAttributes;
                if (null == attrs)
                {
                    attrs = attrs1;
                }
                else
                {
                    string attType = "";
                    Dictionary<string, CustomAttributeData> attrDic = new Dictionary<string, CustomAttributeData>();
                    List<CustomAttributeData> list = new List<CustomAttributeData>();
                    foreach (CustomAttributeData item in attrs)
                    {
                        attType = item.AttributeType.Name;
                        if (attrDic.ContainsKey(attType)) continue;
                        attrDic[attType] = item;
                        list.Add(item);
                    }

                    foreach (CustomAttributeData item in attrs1)
                    {
                        attType = item.AttributeType.Name;
                        if (attrDic.ContainsKey(attType)) continue;
                        attrDic[attType] = item;
                        list.Add(item);
                    }

                    attrs = list;
                }
            }

            if (null != attrs)
            {
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
            }
            #endregion

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
