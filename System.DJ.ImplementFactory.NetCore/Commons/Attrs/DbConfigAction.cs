using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.IO;
using System.Xml;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class DbConfigAction : AbsSysAttributer
    {
        private string dbConnectionStringMapping = "dbConeectionString";
        private string dbTypeMapping = "dbType";

        /// <summary>
        /// Identifies an interface method that can set the database connection string with the database type
        /// </summary>
        public DbConfigAction() { }

        /// <summary>
        /// Identifies an interface method that can set the database connection string with the database type
        /// </summary>
        /// <param name="dbConnectionStringMapping">The parameter name of the dbConeectionString mapping</param>
        /// <param name="dbTypeMapping">The parameter name of the dbType mapping</param>
        public DbConfigAction(string dbConnectionStringMapping, string dbTypeMapping)
        {
            this.dbConnectionStringMapping = dbConnectionStringMapping;
            this.dbTypeMapping = dbTypeMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<string> list = new List<string>()
            {
                dbConnectionStringMapping,
                dbTypeMapping
            };
            int size = list.Count;
            for (int i = 0; i < size; i++)
            {
                list[i] = list[i].Trim().ToLower();
            }

            Dictionary<string, object> dic = GetKVListFromHeader(context.HttpContext, list, false);
            if (0 == dic.Count) dic = GetKVListFromBody(context.HttpContext, list, false);
            if (0 == dic.Count) dic = GetKVListFromForm(context.HttpContext, list, false);
            if (0 == dic.Count) dic = GetKVListFromQuery(context.HttpContext, list, false);

            string dbConeectionString = "";
            string dbTypeVal = "";

            string key = dbConnectionStringMapping.Trim().ToLower();
            if (dic.ContainsKey(key))
            {
                dbConeectionString = dic[key].ToString().Trim();
            }

            key = dbTypeMapping.Trim().ToLower();
            if (dic.ContainsKey(key))
            {
                dbTypeVal = dic[key].ToString().Trim();
            }

            if (!string.IsNullOrEmpty(dbTypeVal))
            {
                bool mbool = DbAdapter.SetConfig(dbTypeVal);
                if (mbool)
                {
                    ImplementAdapter.LoadDataServerProvider();
                }
                else
                {
                    dbTypeVal = "";
                }
            }

            if (!string.IsNullOrEmpty(dbConeectionString))
            {
                ImplementAdapter.dbConnectionString = dbConeectionString;
                ImplementAdapter.dbInfo1.ConnectionString = dbConeectionString;
                ImplementAdapter.DbHelper.connectString = dbConeectionString;
            }

            SaveConfigToXml(dbConeectionString, dbTypeVal);
            base.OnActionExecuting(context);
        }

        private void SaveConfigToXml(string dbConeectionString, string dbTypeVal)
        {
            if (string.IsNullOrEmpty(dbConeectionString) && string.IsNullOrEmpty(dbTypeVal)) return;
            string fPath = Path.Combine(DJTools.RootPath, ImplementAdapter.configFile_Xml);
            if (!File.Exists(fPath)) return;
            XmlDoc doc = new XmlDoc();
            XmlElement rootNode = doc.Load(fPath);
            if (null == rootNode) return;
            if (!rootNode.HasChildNodes) return;

            string sysBaseNode = ImplementAdapter._rootNodeName.ToLower();
            string sysBaseNode1 = ImplementAdapter._rootNodeName1.ToLower();
            string fn = "";
            XmlNode baseConfigNode = null;
            rootNode.ForeachChildNode(item =>
            {
                fn = item.Name.ToLower();
                if (fn.Equals(sysBaseNode) || fn.Equals(sysBaseNode1))
                {
                    baseConfigNode = item;
                    return false;
                }
                return true;
            });

            if (null == baseConfigNode) return;

            int num1 = 0;
            if (!string.IsNullOrEmpty(dbConeectionString)) num1++;
            if (!string.IsNullOrEmpty(dbTypeVal)) num1++;

            int num2 = 0;
            string connStrName = "ConnectionString".ToLower();
            string dbTypeName = "DatabaseType".ToLower();
            baseConfigNode.ForeachChildNode(item =>
            {
                fn = item.Name.ToLower();
                if (fn.Equals(connStrName))
                {
                    if (!string.IsNullOrEmpty(dbConeectionString))
                    {
                        item.InnerText = dbConeectionString;
                        num2++;
                    }
                }
                else if (fn.Equals(dbTypeName))
                {
                    if (!string.IsNullOrEmpty(dbTypeVal))
                    {
                        item.InnerText = dbTypeVal;
                        num2++;
                    }
                }
                if (num1 == num2) return false;
                return true;
            });            
            doc.Save(fPath);
        }
    }
}
