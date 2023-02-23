using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.DJ.ImplementFactory.Commons;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Test.NetCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : Controller
    {
        [HttpPost, Route("GetUserInfo")]
        public object GetUserInfo(object data)
        {
            //string pwd = DJPW.GetPW(3024);
            return new { success = true, message = "", data = data };
        }

        [Route("GetCount")]
        public int Count()
        {
            return 3;
        }

        [HttpPost, Route("GetUserName")]
        public string UserName(string name)
        {
            return name;
        }

        [HttpPost, Route("GetAuthCode")]
        public string AuthCode()
        {
            string code = getBase64Code();
            return code;
        }

        [HttpPost, Route("ValidateLogin")]
        public string ValidateLogin(object data)
        {
            if (null == data) return "empty";
            JToken jt = JToken.Parse(data.ToString());
            string UserName = jt["UserName"].ToString();
            string Password = jt["Password"].ToString();
            string AuthCode = jt["AuthCode"].ToString();
            return DJTools.ExtFormat("[{0}] 登录成功！", UserName);
        }

        private static object _getCode = new object();
        private string getBase64Code()
        {
            lock (_getCode)
            {
                string code = "";
                int width = 62;
                int height = 30;
                Bitmap img = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img);
                g.Clear(Color.White);

                Color fore_color = Color.FromArgb(36, 173, 243);
                Brush brush = new SolidBrush(fore_color);

                Random rnd = new Random();
                int a = rnd.Next(1, 9);
                int b = rnd.Next(1, 9);

                string s = a + " + " + b;
                Font fond = new Font("宋体", 12f);
                int fH = fond.Height;

                float x = 10f;
                float y = (height - fH) / 2;
                g.DrawString(s, fond, brush, x, y);

                fond.Dispose();
                brush.Dispose();
                g.Dispose();

                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    code = Convert.ToBase64String(ms.ToArray());
                }

                img.Dispose();

                if (!string.IsNullOrEmpty(code))
                {
                    s = "data:image/png;base64,";
                    string s1 = s.ToLower();
                    string s2 = code.Substring(0, s.Length).ToLower();
                    if (!s1.Equals(s2))
                    {
                        code = s + code;
                    }
                }
                return code;
            }
        }
    }
}
