using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Drawing;
using System.Runtime.InteropServices;
using Test.NetCore.Entities;

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

        public class testJson
        {
            public int key { get; set; }
            public string val { get; set; }
        }

        static void Main(string[] args)
        {
            SetWindowPositionCenter();
            //, \"arr\": [1,2,3]
            string s = "{\"token\":\"abc\", \"arr\": [1,2,3], \"data\": [{\"key\":1, \"val\":\"a\"}, {\"key\":2, \"val\":\"b\"}]}";
            //testJson[] list1 = (testJson[])s.JsonToList<testJson[]>();
            JObject jt = JObject.Parse(s);
            IEnumerable<JProperty> list = jt.Properties();
            string k = "";
            object v = null;

            object arr = new int[] { 1, 2 };
            if (null != arr.GetType().GetInterface("System.Collections.IEnumerable"))
            {
                k = "1";
            }

            foreach (JProperty item in list)
            {
                if(item.Value.Type == JTokenType.Array)
                {                    
                    JArray jo = item.Value.ToObject<JArray>();
                    int ncount = jo.Count;
                    v = "";
                    foreach (JToken item1 in item.Value)
                    {
                        v = item1.ToString();
                    }
                }
                v = item.Value.ToString();
                k = item.Name;
            }

            LogicCalculate logicCalculate = new LogicCalculate();
            Console.WriteLine("result: " + logicCalculate.testCalculate());
            Console.WriteLine("");

            UserInfoLogic userInfoLogic = new UserInfoLogic();
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

            Console.WriteLine("Hello World!");
            Console.ReadKey(true);
        }

        
    }
}
