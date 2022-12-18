using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public static class DataTranslation
    {

        public static string ByteToStr(this byte[] dt)
        {
            if (null == dt) return "";
            string v = Encoding.UTF8.GetString(dt);
            if (-1 != v.IndexOf("\0")) v = v.Substring(0, v.IndexOf("\0"));
            return v;
        }

        public static byte[] StrToByte(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private static int headSize = 6;
        private static string headFlag = "@";
        private static string CollectSign = "IEnumerable";
        public static byte[] ObjectToByteArray(this object dataObj)
        {
            byte[] result = null;
            if (null == dataObj) return result;
            Type type = dataObj.GetType();

            if (DJTools.IsBaseType(type))
            {
                result = dataObj.ToString().StrToByte();
            }
            else if (null != (dataObj as IEnumerable))
            {
                IEnumerable enumerable = dataObj as IEnumerable;
                List<byte[]> list = new List<byte[]>();
                int size = 0;
                byte[] buffer = null;
                string prop = "";
                foreach (var item in enumerable)
                {
                    buffer = item.ObjectToByteArray();
                    size += buffer.Length;
                    prop += "," + buffer.Length;
                    list.Add(buffer);
                }
                if (0 == list.Count) return result;

                prop = prop.Substring(1);
                byte[] propertyData = prop.StrToByte();

                int AllSize = propertyData.Length + size;
                string typeName = type.TypeToString(true);
                typeName += ":" + CollectSign + ":" + AllSize + ":" + propertyData.Length;
                byte[] headBuffer = typeName.StrToByte();

                string headStr = headFlag + headBuffer.Length;
                buffer = headStr.StrToByte();
                AllSize += (headSize + headBuffer.Length);

                int pos = 0;
                result = new byte[AllSize];
                Array.Copy(buffer, 0, result, pos, buffer.Length);
                pos += headSize;

                Array.Copy(headBuffer, 0, result, pos, headBuffer.Length);
                pos += headBuffer.Length;

                Array.Copy(propertyData, 0, result, pos, propertyData.Length);
                pos += propertyData.Length;

                foreach (var item in list)
                {
                    Array.Copy(item, 0, result, pos, item.Length);
                    pos += item.Length;
                }
            }
            else
            {
                result = dataObj.EntityToByteArray();
            }
            return result;
        }

        public static byte[] EntityToByteArray(this object entity)
        {
            if (null == entity) return null;
            byte[] dt = null;
            byte[] buffer = null;
            if (DJTools.IsBaseType(entity.GetType()))
            {
                dt = entity.ToString().StrToByte();
                return dt;
            }

            string paras = "";
            int dataSize = 0;
            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            entity.ForeachProperty((pi, type, fn, fv) =>
            {
                buffer = null;
                if (null != fv)
                {
                    if (typeof(byte[]) == type)
                    {
                        buffer = (byte[])fv;
                    }
                    else if (DJTools.IsBaseType(type))
                    {
                        buffer = fv.ToString().StrToByte();
                    }
                    else
                    {
                        buffer = fv.ObjectToByteArray();
                    }
                }

                if (null == buffer) buffer = new byte[] { };
                dic.Add(fn, buffer);
                dataSize += buffer.Length;
                paras += "," + fn + ":" + type.TypeToString(true) + ":" + buffer.Length;
            });

            if (!string.IsNullOrEmpty(paras))
            {
                paras = paras.Substring(1);
            }

            paras = entity.GetType().TypeToString(true) + "#" + paras;
            byte[] infoDatas = paras.StrToByte();
            int AllSize = infoDatas.Length + dataSize;

            string sizeInfo = "object:" + AllSize + ",property:" + infoDatas.Length;
            byte[] hdData = sizeInfo.StrToByte();

            string headStr = headFlag + hdData.Length;
            byte[] headBuffer = headStr.StrToByte();
            AllSize += (headSize + hdData.Length);

            int pos = 0;
            dt = new byte[AllSize];
            Array.Copy(headBuffer, 0, dt, pos, headBuffer.Length);
            pos += headSize;

            Array.Copy(hdData, 0, dt, pos, hdData.Length);
            pos += hdData.Length;

            Array.Copy(infoDatas, 0, dt, pos, infoDatas.Length);
            pos += infoDatas.Length;

            foreach (var item in dic)
            {
                Array.Copy(item.Value, 0, dt, pos, item.Value.Length);
                pos += item.Value.Length;
            }

            return dt;
        }

        public static T ByteArrayToEntity<T>(this byte[] data)
        {
            Type type = typeof(T);
            return (T)data.ByteArrayToEntity(type);
        }

        public static T ByteArrayToObject<T>(this byte[] data)
        {
            Type type = typeof(T);
            return (T)data.ByteArrayToObject(type);
        }

        public static object ByteArrayToObject(this byte[] data, Type type)
        {
            object result = null;
            if (null == data) return result;
            if (0 == data.Length) return result;

            string s = "";
            if (DJTools.IsBaseType(type))
            {
                s = data.ByteToStr();
                result = DJTools.ConvertTo(s, type);
                return result;
            }

            if (headSize > data.Length) return result;
            byte[] buffer = new byte[headSize];
            Array.Copy(data, 0, buffer, 0, headSize);
            s = buffer.ByteToStr();
            if (string.IsNullOrEmpty(s)) return result;
            if (!s.Substring(0, 1).Equals(headFlag)) return result;

            s = s.Substring(1);
            int len = Convert.ToInt32(s);
            buffer = new byte[len];
            Array.Copy(data, headSize, buffer, 0, len);
            s = buffer.ByteToStr();

            string sign = ":" + CollectSign + ":";
            if (-1 != s.IndexOf(sign))
            {
                string[] arr = s.Split(':');
                int propSize = Convert.ToInt32(arr[3]);

                int pos = headSize + len;
                buffer = new byte[propSize];
                Array.Copy(data, pos, buffer, 0, propSize);
                pos += propSize;

                s = buffer.ByteToStr();
                arr = s.Split(',');
                len = arr.Length;
                Type[] types = type.GenericTypeArguments;
                Type paraType = null;
                bool isArr = false;
                if (0 == types.Length)
                {
                    result = DJTools.createArrayByType(type, len);
                    s = type.FullName;
                    s = s.Replace("[]", "");
                    paraType = s.GetClassTypeByPath();
                    isArr = true;
                }
                else
                {
                    result = DJTools.createListByType(type);
                    paraType = types[0];
                }
                if (null == paraType) return result;

                int size = 0;
                object vObj = null;
                for (int i = 0; i < len; i++)
                {
                    size = Convert.ToInt32(arr[i]);
                    buffer = new byte[size];
                    Array.Copy(data, pos, buffer, 0, size);
                    pos += size;
                    vObj = buffer.ByteArrayToObject(paraType);
                    if (isArr)
                    {
                        DJTools.arrayAdd(result, vObj, i);
                    }
                    else
                    {
                        DJTools.listAdd(result, vObj);
                    }
                }
            }
            else
            {
                result = data.ByteArrayToEntity(type);
            }
            return result;
        }

        public static object ByteArrayToEntity(this byte[] data, Type type)
        {
            object dt = null;
            if (null == data) return dt;
            if (0 == data.Length) return dt;
            Func<byte[], Type, object> func = (_para, _paraType) =>
            {
                string s = _para.ByteToStr();
                return DJTools.ConvertTo(s, _paraType);
            };

            if (headSize > data.Length)
            {
                dt = func(data, type);
                return dt;
            }
            byte[] buffer = new byte[headSize];
            Array.Copy(data, 0, buffer, 0, headSize);
            string headStr = buffer.ByteToStr();
            if (string.IsNullOrEmpty(headStr)) return dt;
            if (!headStr.Substring(0, 1).Equals(headFlag)) return dt;

            headStr = headStr.Substring(1);
            int len = Convert.ToInt32(headStr);
            buffer = new byte[len];
            Array.Copy(data, headSize, buffer, 0, len);
            headStr = buffer.ByteToStr();
            if (string.IsNullOrEmpty(headStr)) return dt;

            Regex rg = new Regex(@"object\s*\:\s*(?<ObjectSize>[0-9]+)\s*\,\s*property\s*\:\s*(?<PropertySize>[0-9]+)", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(headStr))
            {
                dt = func(data, type);
                return dt;
            }

            int ObjectSize = 0;
            int PropertySize = 0;

            Match match = rg.Match(headStr);
            string sv = match.Groups["ObjectSize"].Value;
            int.TryParse(sv, out ObjectSize);

            sv = match.Groups["PropertySize"].Value;
            int.TryParse(sv, out PropertySize);

            int pos = headSize + len;
            buffer = new byte[PropertySize];
            Array.Copy(data, pos, buffer, 0, PropertySize);
            string prop = buffer.ByteToStr();

            pos = prop.IndexOf("#");
            string typeName = prop.Substring(0, pos);
            prop = prop.Substring(pos + 1);

            if (null == type) type = typeName.GetClassTypeByPath();
            if (null == type) throw new Exception("Object type '" + typeName + "' is not exist.");

            dt = Activator.CreateInstance(type);
            PropertyInfo pi = null;

            int size = 0;
            string[] arr = prop.Split(',');
            string[] arr1 = null;
            string fn = "";
            object fv = null;
            //Type ft = null;
            pos = headSize + len + PropertySize;
            foreach (var item in arr)
            {
                size = 0;
                arr1 = item.Split(':');
                fn = arr1[0].Trim();
                //ft = arr1[1].GetClassTypeByPath();
                //if (null == ft) continue;
                int.TryParse(arr1[2].Trim(), out size);
                buffer = new byte[size];
                Array.Copy(data, pos, buffer, 0, buffer.Length);
                pos += buffer.Length;

                pi = dt.GetType().GetProperty(fn);
                if (null == pi) continue;
                if (pi.PropertyType == typeof(byte[]))
                {
                    pi.SetValue(dt, buffer);
                }
                else if (DJTools.IsBaseType(pi.PropertyType))
                {
                    fv = buffer.ByteToStr();
                    fv = DJTools.ConvertTo(fv, pi.PropertyType);
                    pi.SetValue(dt, fv);
                }
                else
                {
                    fv = buffer.ByteArrayToObject(pi.PropertyType);
                    if (null != fv) pi.SetValue(dt, fv);
                }
            }

            return dt;
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
            if (null == data) return null;
            if (dataTableTopSize >= data.Length) return null;
            byte[] topData = new byte[dataTableTopSize];
            Array.Copy(data, 0, topData, 0, dataTableTopSize);
            string top = topData.ByteToStr();
            if (0 != top.IndexOf(dataTableTag)) return null;

            string s = top.Substring(dataTableTag.Length);
            int infoSize = Convert.ToInt32(s);

            byte[] infoData = new byte[infoSize];
            Array.Copy(data, dataTableTopSize, infoData, 0, infoSize);

            s = infoData.ByteToStr();
            string[] rows = s.Split(rowSplit);
            string[] columnInfos = rows[0].Split(cellSplit);

            string[] arr = null;
            Type type = null;
            DataTable dt = new DataTable();
            foreach (string item in columnInfos)
            {
                arr = item.Split(unitSplit);
                type = Type.GetType(arr[1]);
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

    }
}
