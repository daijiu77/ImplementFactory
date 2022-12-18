using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public static class ColorTranslation
    {
        public enum ColorSimilarDirection
        {
            /// <summary>
            /// 深色方向
            /// </summary>
            DeepColor,
            /// <summary>
            /// 浅色方向
            /// </summary>
            LightColour
        }

        /// <summary>
        /// RGB转CMYK
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <param name="k"></param>
        public static void RGB2CMYK(int red, int green, int blue, out double c, out double m, out double y, out double k)
        {
            c = (double)(255 - red) / 255;
            m = (double)(255 - green) / 255;
            y = (double)(255 - blue) / 255;

            k = (double)Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                c = m = y = 0;
            }
            else
            {
                c = (c - k) / (1 - k);
                m = (m - k) / (1 - k);
                y = (y - k) / (1 - k);
            }
        }

        /// <summary>
        /// CMYK转RGB
        /// </summary>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <param name="k"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void CMYK2RGB(double c, double m, double y, double k, out int r, out int g, out int b)
        {
            r = Convert.ToInt32((1.0 - c) * (1.0 - k) * 255.0);
            g = Convert.ToInt32((1.0 - m) * (1.0 - k) * 255.0);
            b = Convert.ToInt32((1.0 - y) * (1.0 - k) * 255.0);
        }

        public static void RGB2HSB(int red, int green, int blue, out double hue, out double sat, out double bri)
        {
            double r = ((double)red / 255.0);
            double g = ((double)green / 255.0);
            double b = ((double)blue / 255.0);

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            hue = 0.0;
            if (max == r && g >= b)
            {
                if (max - min == 0) hue = 0.0;
                else hue = 60 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                hue = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == g)
            {
                hue = 60 * (b - r) / (max - min) + 120;
            }
            else if (max == b)
            {
                hue = 60 * (r - g) / (max - min) + 240;
            }

            sat = (max == 0) ? 0.0 : (1.0 - ((double)min / (double)max));
            bri = max;
        }

        public static void HSB2RGB(double hue, double sat, double bri, out int red, out int green, out int blue)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (sat == 0)
            {
                r = g = b = bri;
            }
            else
            {
                // the color wheel consists of 6 sectors. Figure out which sector you're in.
                double sectorPos = hue / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));
                // get the fractional part of the sector
                double fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color. 
                double p = bri * (1.0 - sat);
                double q = bri * (1.0 - (sat * fractionalSector));
                double t = bri * (1.0 - (sat * (1 - fractionalSector)));

                // assign the fractional colors to r, g, and b based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = bri;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = bri;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = bri;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = bri;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = bri;
                        break;
                    case 5:
                        r = bri;
                        g = p;
                        b = q;
                        break;
                }
            }
            red = Convert.ToInt32(r * 255);
            green = Convert.ToInt32(g * 255);
            blue = Convert.ToInt32(b * 255); ;
        }

        public static string Color2Hex(this Color c)
        {
            string hex = RGB2Hex(c.R, c.G, c.B);
            return hex;
        }

        /// <summary>
        /// RGB转#000000
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string RGB2Hex(int r, int g, int b)
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}", (int)r, (int)g, (int)b);
        }

        /// <summary>
        /// #FFFFFF转Color
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Color Hex2Color(this string hexColor)
        {
            string r, g, b;

            if (string.IsNullOrEmpty(hexColor) == false)
            {
                try
                {
                    hexColor = hexColor.Trim();
                    if (hexColor[0] == '#') hexColor = hexColor.Substring(1, hexColor.Length - 1);
                    while (hexColor.Length < 6)
                    {
                        hexColor += "0";
                    }
                    //MessageBox.Show(hexColor);
                    r = hexColor.Substring(0, 2);
                    g = hexColor.Substring(2, 2);
                    b = hexColor.Substring(4, 2);

                    int r1 = 16 * GetIntFromHex(r.Substring(0, 1)) + GetIntFromHex(r.Substring(1, 1));
                    int g1 = 16 * GetIntFromHex(g.Substring(0, 1)) + GetIntFromHex(g.Substring(1, 1));
                    int b1 = 16 * GetIntFromHex(b.Substring(0, 1)) + GetIntFromHex(b.Substring(1, 1));
                    r = Convert.ToString(r1);
                    g = Convert.ToString(g1);
                    b = Convert.ToString(b1);

                    return Color.FromArgb(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b));
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }

            }

            return Color.Empty;
        }

        private static int GetIntFromHex(string strHex)
        {
            switch (strHex.ToUpper())
            {
                case ("A"):
                    {
                        return 10;
                    }
                case ("B"):
                    {
                        return 11;
                    }
                case ("C"):
                    {
                        return 12;
                    }
                case ("D"):
                    {
                        return 13;
                    }
                case ("E"):
                    {
                        return 14;
                    }
                case ("F"):
                    {
                        return 15;
                    }
                default:
                    {
                        return int.Parse(strHex);
                    }
            }
        }

        /// <summary>
        /// 获取类似的颜色
        /// </summary>
        /// <param name="color">起始颜色</param>
        /// <param name="SimilarRate">类似率1~254</param>
        /// <param name="similarDir">颜色取值方向</param>
        /// <returns></returns>
        public static Color SimilarColor(Color color, int SimilarRate, ColorSimilarDirection similarDir)
        {
            Color color1 = Color.Empty;
            if (color == Color.Empty)
            {
                return color1;
            }

            if (SimilarRate == 0)
            {
                return color1;
            }

            double rate1 = Convert.ToDouble(SimilarRate);
            rate1 = rate1 < 0 ? 0 - rate1 : rate1;

            int r = color.R;
            int g = color.G;
            int b = color.B;

            double r1 = Convert.ToDouble(r);
            double g1 = Convert.ToDouble(g);
            double b1 = Convert.ToDouble(b);

            int[] arr = new int[] { r, g, b };
            Array.Sort(arr);
            int min = arr[0];
            int max = arr[arr.Length - 1];
            double max1 = Convert.ToDouble(max);

            if (similarDir == ColorSimilarDirection.DeepColor)
            {
                if (max1 - rate1 < 0)
                {
                    rate1 = max1;
                }

                r1 = r1 - (r1 * rate1) / max1;
                g1 = g1 - (g1 * rate1) / max1;
                b1 = b1 - (b1 * rate1) / max1;
            }
            else
            {
                if (min + rate1 > 255.0)
                {
                    rate1 = 255.0 - min;
                }

                r1 = r1 + (r1 * rate1) / min;
                g1 = g1 + (g1 * rate1) / min;
                b1 = b1 + (b1 * rate1) / min;
            }

            r = Convert.ToInt32(r1);
            g = Convert.ToInt32(g1);
            b = Convert.ToInt32(b1);

            r = r < 0 ? 0 : (r > 255 ? 255 : r);
            g = g < 0 ? 0 : (g > 255 ? 255 : g);
            b = b < 0 ? 0 : (b > 255 ? 255 : b);

            color1 = Color.FromArgb(r, g, b);

            return color1;
        }
    }
}
