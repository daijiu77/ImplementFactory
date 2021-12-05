using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.NetCore.Commons.Attrs
{
    /// <summary>
    /// where 条件解析属性,默认情况下采用对象属性做为字段名,采用等号(=)做为比较符,采用and做为逻辑连接符
    /// </summary>
    public class Condition : Attribute
    {
        private string fieldMapping = null;
        private string compareSign = "=";
        private LogicSign logicSign = LogicSign.and;
        private WhereIgrons whereIgrons = WhereIgrons.none;

        /// <summary>
        /// 设置/获取表名或表名标识
        /// </summary>
        public string TableSign { get; set; }

        public string FieldMapping
        {
            get { return fieldMapping; }
            set { fieldMapping = value; }
        }

        public string CompareSign
        {
            get { return compareSign; }
            set { compareSign = value; }
        }

        public LogicSign logic_sign
        {
            get { return logicSign; }
            set { logicSign = value; }
        }

        public WhereIgrons where_igrones
        {
            get { return whereIgrons; }
            set { whereIgrons = value; }
        }

        public class PropertyInfoExt
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public object GetValue(object entity)
            {
                return Value;
            }

            public object GetValue()
            {
                return Value;
            }

            public Type PropertyType { get; set; }

            public Type DeclaringType { get; set; }
        }

        /// <summary>
        /// 获取条件单元
        /// </summary>
        /// <param name="propertyInfo">对象属性</param>
        /// <returns></returns>
        public string Unit(PropertyInfoExt propertyInfo)
        {
            string unitStr = "";
            PropertyInfoExt pi = propertyInfo;
            object val = pi.GetValue();
            if (null == val
                && ((WhereIgrons.igroneNull == (whereIgrons & WhereIgrons.igroneNull))
                || (WhereIgrons.igroneEmptyNull == (whereIgrons & WhereIgrons.igroneEmptyNull)))) return unitStr;

            if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
            {
                if (null == val) return unitStr;
                DateTime dt = Convert.ToDateTime(val);
                if (DateTime.MinValue == dt
                    && ((WhereIgrons.igroneMinDate == (whereIgrons & WhereIgrons.igroneMinDate))
                    || (WhereIgrons.igroneMinMaxDate == (whereIgrons & WhereIgrons.igroneMinMaxDate)))) return unitStr;
                if (DateTime.MaxValue == dt
                    && ((WhereIgrons.igroneMaxDate == (whereIgrons & WhereIgrons.igroneMaxDate))
                    || (WhereIgrons.igroneMinMaxDate == (whereIgrons & WhereIgrons.igroneMinMaxDate)))) return unitStr;
            }
            else if (pi.PropertyType == typeof(Guid) || pi.PropertyType == typeof(Guid?))
            {
                if (null == val) val = Guid.Empty.ToString();
                Guid g = new Guid(val.ToString());
                if (Guid.Empty == g
                    && ((WhereIgrons.igroneNull == (whereIgrons & WhereIgrons.igroneNull))
                    || (WhereIgrons.igroneEmptyNull == (whereIgrons & WhereIgrons.igroneEmptyNull)))) return unitStr;
            }
            else if (pi.PropertyType == typeof(string))
            {
                if (null == val) val = "";
                string s = val.ToString();
                if (0 == s.Length
                    && ((WhereIgrons.igroneNull == (whereIgrons & WhereIgrons.igroneNull))
                    || (WhereIgrons.igroneEmptyNull == (whereIgrons & WhereIgrons.igroneEmptyNull)))) return unitStr;
            }
            else if (pi.PropertyType == typeof(bool) || pi.PropertyType == typeof(bool?))
            {
                if (null == val) return unitStr;
                bool mbool = Convert.ToBoolean(val);
                if (false == mbool && (WhereIgrons.igroneFalse == (whereIgrons & WhereIgrons.igroneFalse))) return unitStr;
            }
            else if (typeof(ValueType) == pi.PropertyType.BaseType)
            {
                if (null == val) return unitStr;
                double db = Convert.ToDouble(val);
                if (0 == db && (WhereIgrons.igroneZero == (whereIgrons & WhereIgrons.igroneZero))) return unitStr;
            }
            else
            {
                return unitStr;
            }

            bool isLike = false;
            string comp = compareSign.ToLower().Trim();
            Regex leftRg = new Regex(@"(^left[_\-\s]+like$)|(^llike$)", RegexOptions.IgnoreCase);
            Regex rightRg = new Regex(@"(^right[_\-\s]+like$)|(^rlike$)", RegexOptions.IgnoreCase);
            if (comp.Equals("like"))
            {
                comp = " like '%{0}%'";
                isLike = true;
            }
            else if (leftRg.IsMatch(comp))
            {
                comp = " like '%{0}'";
                isLike = true;
            }
            else if (rightRg.IsMatch(comp))
            {
                comp = " like '{0}%'";
                isLike = true;
            }
            else
            {
                comp = compareSign + "{0}";
            }

            string nullstr = null == val ? "null" : "";
            if (isLike)
            {
                if (string.IsNullOrEmpty(nullstr))
                {
                    comp = comp.Replace("{0}", val.ToString());
                }
                else
                {
                    comp = comp.Replace("{0}", " ");
                }
            }
            else if (pi.PropertyType == typeof(string)
                || pi.PropertyType == typeof(DateTime)
                || pi.PropertyType == typeof(DateTime?)
                || pi.PropertyType == typeof(Guid)
                || pi.PropertyType == typeof(Guid?))
            {
                if (string.IsNullOrEmpty(nullstr))
                {
                    comp = comp.Replace("{0}", "'" + val.ToString() + "'");
                }
                else
                {
                    comp = comp.Replace("{0}", nullstr);
                }
            }
            else if (pi.PropertyType == typeof(bool) || pi.PropertyType == typeof(bool?))
            {
                if (string.IsNullOrEmpty(nullstr))
                {
                    bool vbool = Convert.ToBoolean(val);
                    string v = vbool ? "1" : "0";
                    comp = comp.Replace("{0}", v);
                }
                else
                {
                    comp = comp.Replace("{0}", "0");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(nullstr))
                {
                    comp = comp.Replace("{0}", val.ToString());
                }
                else
                {
                    comp = comp.Replace("{0}", nullstr);
                }
            }

            string tb = "";
            if (!string.IsNullOrEmpty(TableSign))
            {
                tb = TableSign + ".";
            }

            if (string.IsNullOrEmpty(fieldMapping))
            {
                comp = tb + pi.Name + comp;
            }
            else
            {
                comp = tb + fieldMapping + comp;
            }

            unitStr = And_Or + " " + comp;

            return unitStr;
        }

        /// <summary>
        /// 获取 and 或者 or
        /// </summary>
        private string And_Or
        {
            get
            {
                string andor = "and";
                if (LogicSign.or == logicSign) andor = "or";
                return andor;
            }
        }

        /// <summary>
        /// 忽视属性值
        /// </summary>
        public enum WhereIgrons
        {
            none = 0,
            igroneEmpty = 2,
            igroneNull = 4,
            igroneEmptyNull = 8,
            igroneFalse = 16,
            igroneMinMaxDate = 32,
            igroneMinDate = 64,
            igroneMaxDate = 128,
            igroneZero = 256
        }

        /// <summary>
        /// and 或者 or 逻辑连接符
        /// </summary>
        public enum LogicSign
        {
            and = 0,
            or
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="fieldMapping">映射的数据表字段名称,为空时等同于属性名称</param>
        /// <param name="compareSign">比较符号,可为: like, left_like(llike), right_like(rlike), 大于等于, 小于等于, 不等于</param>
        /// <param name="logicSign">逻辑连接符</param>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(string fieldMapping, string compareSign, LogicSign logicSign, WhereIgrons whereIgrons)
        {
            this.fieldMapping = fieldMapping;
            this.compareSign = compareSign;
            this.logicSign = logicSign;
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="field_compare">可以是字段映射，也可是比较符</param>
        private void FieldCompare(string field_compare)
        {
            Regex rg = new Regex(@"(^like$)|(^left[_\-\s]+like$)|(^llike$)|(^right[_\-\s]+like$)|(^rlike$)|(^\<\=$)|(^\<$)|(^\>\=$)|(^\>$)|(^\=$)", RegexOptions.IgnoreCase);
            if (rg.IsMatch(field_compare))
            {
                compareSign = field_compare;
            }
            else
            {
                fieldMapping = field_compare;
            }
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="field_compare">可以是字段映射，也可是比较符</param>
        public Condition(string field_compare)
        {
            FieldCompare(field_compare);
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="fieldMapping">映射的字段名称</param>
        /// <param name="compareSign">比较符号,可为: like, left_like(llike), right_like(rlike), 大于等于, 小于等于, 不等于</param>
        public Condition(string fieldMapping, string compareSign)
        {
            this.fieldMapping = fieldMapping;
            this.compareSign = compareSign;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="fieldMapping">映射的字段名称</param>
        /// <param name="compareSign">比较符号,可为: like, left_like(llike), right_like(rlike), 大于等于, 小于等于, 不等于</param>
        /// <param name="logicSign">逻辑连接符</param>
        public Condition(string fieldMapping, string compareSign, LogicSign logicSign)
        {
            this.fieldMapping = fieldMapping;
            this.compareSign = compareSign;
            this.logicSign = logicSign;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="fieldMapping">映射的字段名称</param>
        /// <param name="compareSign">比较符号,可为: like, left_like(llike), right_like(rlike), 大于等于, 小于等于, 不等于</param>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(string fieldMapping, string compareSign, WhereIgrons whereIgrons)
        {
            this.fieldMapping = fieldMapping;
            this.compareSign = compareSign;
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="field_compare">可以是字段映射，也可是比较符</param>
        /// <param name="logicSign">逻辑连接符</param>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(string field_compare, LogicSign logicSign, WhereIgrons whereIgrons)
        {
            FieldCompare(field_compare);
            this.logicSign = logicSign;
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="logicSign">逻辑连接符</param>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(LogicSign logicSign, WhereIgrons whereIgrons)
        {
            this.logicSign = logicSign;
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="field_compare">可以是字段映射，也可是比较符</param>
        /// <param name="logicSign">逻辑连接符</param>
        public Condition(string field_compare, LogicSign logicSign)
        {
            FieldCompare(field_compare);
            this.logicSign = logicSign;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="field_compare">可以是字段映射，也可是比较符</param>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(string field_compare, WhereIgrons whereIgrons)
        {
            FieldCompare(field_compare);
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="logicSign">条件逻辑连接符</param>
        public Condition(LogicSign logicSign)
        {
            this.logicSign = logicSign;
        }

        /// <summary>
        /// where 条件解析属性
        /// </summary>
        /// <param name="whereIgrons">设置忽视属性值</param>
        public Condition(WhereIgrons whereIgrons)
        {
            this.whereIgrons = whereIgrons;
        }

        /// <summary>
        /// where 条件解析属性,默认情况下采用对象属性做为字段名,采用等号(=)做为比较符,采用and做为逻辑连接符,不忽视任何值
        /// </summary>
        public Condition() { }
    }
}
