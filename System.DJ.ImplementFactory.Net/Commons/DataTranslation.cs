using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public static class DataTranslation
    {

        public static string ByteToStr(this byte[] dt)
        {
            if (null == dt) return "";
            string v = Encoding.UTF8.GetString(dt);
            int n = v.IndexOf("\0");
            if (0 < n) v = v.Substring(0, n);
            char[] c = v.ToCharArray();
            n = c.Length - 1;
            while ('\0' == c[n])
            {
                n--;
                if (0 > n) break;
            }
            if ((n + 1) < c.Length)
            {
                n++;
                v = v.Substring(0, n);
            }
            return v;
        }

        public static byte[] StrToByte(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private const int dataTableTopSize = 50;
        private const string dataTableTag = "DataTable@";
        private const char unitSplit = ':';
        private const char cellSplit = ',';
        private const char rowSplit = ';';
        private static string getCells(object key, object val)
        {
            return key + unitSplit.ToString() + val;
        }

        private static string Join(this Array array, string splitTag)
        {
            string s = "";
            if (null == array) return s;
            if (0 == array.Length) return s;
            foreach (var item in array)
            {
                s += splitTag + item;
            }
            s = s.Substring(splitTag.Length);
            return s;
        }

        public static byte[] DataTableToByteArray(this DataTable dataTable)
        {
            byte[] result = null;
            if (null == dataTable) return result;

            string cells = "";
            string rows = "";
            string grids = "";
            string tp = "";
            int dataSize = 0;
            int len = 0;
            object val = null;
            byte[] bt = null;
            List<byte[]> list = new List<byte[]>();

            List<string> cols = new List<string>();
            foreach (DataColumn dc in dataTable.Columns)
            {
                tp = dc.DataType.TypeToString(true);
                cells = getCells(dc.ColumnName, tp);
                cols.Add(cells);
            }

            dataSize = 0;
            foreach (DataRow dr in dataTable.Rows)
            {
                rows = "";
                foreach (DataColumn dc in dataTable.Columns)
                {
                    val = dr[dc.ColumnName];
                    cells = "";
                    if (DBNull.Value == val)
                    {
                        len = 0;
                    }
                    else if (typeof(byte[]) == dc.DataType)
                    {
                        len = ((byte[])val).Length;
                        list.Add((byte[])val);
                    }
                    else
                    {
                        bt = val.ToString().StrToByte();
                        len = bt.Length;
                        list.Add(bt);
                    }
                    cells = getCells(dataSize, len);
                    dataSize += len;
                    rows += cellSplit + cells;
                }
                rows = rows.Substring(1);
                grids += rowSplit + rows;
            }

            string s = cols.ToArray().Join(cellSplit.ToString());
            s += grids;
            byte[] infoData = s.StrToByte();
            int infoSize = infoData.Length;

            int size = dataTableTopSize;
            size += infoSize;
            size += dataSize;

            result = new byte[size];
            string top = dataTableTag + infoSize;
            bt = top.StrToByte();
            int pos = 0;
            Array.Copy(bt, pos, result, 0, bt.Length);
            pos += dataTableTopSize;

            Array.Copy(infoData, 0, result, pos, infoSize);
            pos += infoSize;

            foreach (byte[] item in list)
            {
                if (0 == item.Length) continue;
                Array.Copy(item, 0, result, pos, item.Length);
                pos += item.Length;
            }
            return result;
        }

        public static DataTable ByteArrayToDataTable(this byte[] data)
        {
            DataTable dt = new DataTable();
            if (null == data) return dt;
            if (dataTableTopSize >= data.Length) return dt;
            byte[] topData = new byte[dataTableTopSize];
            Array.Copy(data, 0, topData, 0, dataTableTopSize);
            string top = topData.ByteToStr();
            if (0 != top.IndexOf(dataTableTag)) return dt;

            string s = top.Substring(dataTableTag.Length);
            int infoSize = Convert.ToInt32(s);

            byte[] infoData = new byte[infoSize];
            Array.Copy(data, dataTableTopSize, infoData, 0, infoSize);

            s = infoData.ByteToStr();
            string[] rows = s.Split(rowSplit);
            string[] columnInfos = rows[0].Split(cellSplit);

            string[] arr = null;
            Type type = null;
            foreach (string item in columnInfos)
            {
                arr = item.Split(unitSplit);
                type = Type.GetType(arr[1]);
                if (null == type)
                {
                    type = DJTools.GetTypeByFullName(arr[1]);
                }
                dt.Columns.Add(arr[0], type);
            }

            int pos = dataTableTopSize + infoSize;
            int dataSize = data.Length - pos;
            byte[] dataBt = new byte[dataSize];
            Array.Copy(data, pos, dataBt, 0, dataSize);
            int len = rows.Length;
            int col = 0;
            int size = 0;
            string row = "";
            string[] units = null;
            byte[] bt = null;
            pos = 0;
            for (int i = 1; i < len; i++)
            {
                col = 0;
                row = rows[i];
                arr = row.Split(cellSplit);
                DataRow dr = dt.NewRow();
                foreach (DataColumn dc in dt.Columns)
                {
                    units = arr[col].Split(unitSplit);
                    col++;
                    pos = Convert.ToInt32(units[0]);
                    size = Convert.ToInt32(units[1]);
                    if (0 == size) continue;
                    bt = new byte[size];
                    Array.Copy(dataBt, pos, bt, 0, size);
                    if (typeof(byte[]) == dc.DataType)
                    {
                        dr[dc.ColumnName] = bt;
                    }
                    else
                    {
                        s = bt.ByteToStr();
                        dr[dc.ColumnName] = DJTools.ConvertTo(s, dc.DataType);
                    }
                }
                dt.Rows.Add(dr);
            }
            return dt;
            //throw new NotImplementedException();
        }

        public static byte[] ListToByteArray<T>(this List<T> list)
        {
            if (DJTools.IsBaseType(typeof(T))) return null;
            if (0 == list.Count) return null;
            Type type = list[0].GetType();
            DataTable dt = new DataTable();
            Attribute attr = null;
            Dictionary<string, string> dicFn = new Dictionary<string, string>();
            type.ForeachProperty((pi, pt, fn) =>
            {
                attr = pi.GetCustomAttribute(typeof(Attrs.Constraint), true);
                if (null != attr) return;
                if (!pt.IsBaseType()) return;
                dt.Columns.Add(fn, pi.PropertyType);
                dicFn.Add(fn.ToLower(), fn);
            });

            DataRow dr = null;
            string field = "";
            foreach (T item in list)
            {
                dr = dt.NewRow();
                item.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) return;
                    field = fn.ToLower();
                    if (!dicFn.ContainsKey(field)) return;
                    dr[fn] = DJTools.ConvertTo(fv, pi.PropertyType);
                });
                dt.Rows.Add(dr);
            }
            return dt.DataTableToByteArray();
            //throw new NotImplementedException();
        }

        public static List<T> ByteArrayToList<T>(this byte[] data)
        {
            List<T> list = new List<T>();
            if (DJTools.IsBaseType(typeof(T))) return list;
            DataTable dt = data.ByteArrayToDataTable();
            list = dt.DataTableToList<T>();
            return list;
            //throw new NotImplementedException();
        }

        public static byte[] ObjectToByteArray<T>(this T dataObj)
        {
            List<T> list = new List<T>();
            list.Add(dataObj);
            return list.ListToByteArray();
        }

        public static T ByteArrayToObject<T>(this byte[] data)
        {
            if (DJTools.IsBaseType(typeof(T))) return default(T);
            List<T> list = data.ByteArrayToList<T>();
            if (0 == list.Count) return default(T);
            return list[0];
        }

        public static byte[] SerializableObjectToByteArray(this object entity)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            byte[] dt = null;
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, entity);
                dt = ms.ToArray();
            }
            return dt;
        }

        public static object ByteArrayToSerializableObject(this byte[] data)
        {
            object vObj = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                vObj = binaryFormatter.Deserialize(ms);
            }
            return vObj;
        }

        public static object JsonToObject(this string json)
        {
            JsonToEntity toEntity = new JsonToEntity();
            return toEntity.GetObject(json);
        }

        public static int HexToDecimalism(this string hex)
        {
            int dec = 0;
            if (string.IsNullOrEmpty(hex)) return dec;
            dec = (int)Convert.ToInt64(hex, 16);
            return dec;
        }

        public static string DecimalismToHex(this Int64 dec)
        {
            string hex = dec.ToString("X6");
            while (0 < hex.Length)
            {
                if (!hex.Substring(0, 1).Equals("0")) break;
                hex = hex.Substring(1);
            }
            return hex;
        }

        public static Int64 ByteArrayToDecimalism(this byte[] bytes)
        {
            string s = ByteArrayToHex(bytes);
            Int64 num = Convert.ToInt64(s, 16);
            return num;
        }

        public static string ByteArrayToHex(this byte[] bytes)
        {
            string s = "";
            if (null == bytes) return s;
            if (0 == bytes.Length) return s;
            int len = bytes.Length;
            string[] arr = new string[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = Convert.ToString(bytes[i], 16);
            }
            s = string.Join("", arr);
            return s;
        }

        public static byte[] DecimalismToByteArray(this int decNum)
        {
            byte[] n16 = null;
            string s = Convert.ToString(decNum, 16);
            n16 = HexToByteArray(s);
            return n16;
        }

        public static byte[] HexToByteArray(this string hex)
        {
            byte[] n16 = null;
            string s = hex;
            string[] arr = null;
            if (2 < s.Length)
            {
                if (s.Substring(0, 2).ToLower().Equals("0x")) s = s.Substring(2);
            }
            int len = s.Length;
            int size = len / 2;
            if (0 < (len % 2)) size++;
            arr = new string[size];
            n16 = new byte[size];
            int n = size;
            while (!string.IsNullOrEmpty(s))
            {
                n--;
                if (2 < s.Length)
                {
                    arr[n] = s.Substring(s.Length - 2);
                    s = s.Substring(0, s.Length - 2);
                }
                else
                {
                    arr[n] = s;
                    s = "";
                }
            }

            len = arr.Length;
            int num = 0;
            for (int i = 0; i < len; i++)
            {
                num = Convert.ToInt32(arr[i], 16);
                n16[i] = (byte)num;
            }
            return n16;
        }

        public static List<object> DataTableToList(this DataTable dataTable, Type type)
        {
            List<object> list = new List<object>();
            if (null == dataTable) return list;
            if (0 == dataTable.Rows.Count) return list;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DataColumn item in dataTable.Columns)
            {
                dic.Add(item.ColumnName.ToLower(), item.ColumnName);
            }

            object ele = null;
            object vObj = null;
            string field = "";
            string fn1 = "";
            string ft = "";
            Type type1 = null;
            bool isArr = false;
            Attribute att = null;
            foreach (DataRow dr in dataTable.Rows)
            {
                ele = Activator.CreateInstance(type);
                type.ForeachProperty((pi, tp, fn) =>
                {
                    isArr = false;
                    if (!DJTools.IsBaseType(tp))
                    {
                        if (tp.IsArray)
                        {
                            ft = tp.TypeToString(true);
                            ft = ft.Replace("[]", "");
                            type1 = Type.GetType(ft);
                            if (null != type1)
                            {
                                isArr = DJTools.IsBaseType(type1);
                            }
                            if (!isArr) return;
                        }
                        else
                        {
                            return;
                        }
                    }
                    field = "";
                    fn1 = fn.ToLower();
                    if (dic.ContainsKey(fn1)) field = dic[fn1];
                    if (string.IsNullOrEmpty(field))
                    {
                        att = pi.GetCustomAttribute(typeof(FieldMapping));
                        if (null != att)
                        {
                            fn1 = ((FieldMapping)att).FieldName.ToLower();
                            if (dic.ContainsKey(fn1)) field = dic[fn1];
                        }
                    }
                    if (string.IsNullOrEmpty(field)) return;
                    vObj = dr[field];
                    if (DBNull.Value == vObj) return;
                    if (null == vObj) return;

                    if (!isArr)
                    {
                        vObj = DJTools.ConvertTo(vObj, tp);
                    }

                    try
                    {
                        pi.SetValue(ele, vObj);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ele.SetPropertyValue(pi.Name, vObj);
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                        //throw;
                    }
                });
                list.Add(ele);
            }
            return list;
        }

        public static List<T> DataTableToList<T>(this DataTable dataTable)
        {
            Type type = typeof(T);
            List<T> datas = new List<T>();
            List<object> list = dataTable.DataTableToList(type);
            foreach (var item in list)
            {
                datas.Add((T)item);
            }
            return datas;
        }

    }
}
