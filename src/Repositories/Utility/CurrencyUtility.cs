using System;

namespace Framework.Core.Utility
{
    /// <summary>
    /// 货币工具类
    /// </summary>
    public class CurrencyUtility
    {
        /// <summary>
        /// 转成汉字货币描述
        /// </summary>
        /// <param name="money">货币字符串</param>
        /// <returns>返回字符串</returns>
        public static string ToChineseChineseCurrency(string money)
        {
            if (!IsPositiveDecimal(money))
                return "";
            if (Double.Parse(money) > 999999999999.99)
            {
                var failMessage = "数字太大，无法换算，请输入一万亿元以下的金额";
                throw new ApplicationException(failMessage);
            }
            var ch = new char[1];
            ch[0] = '.'; //小数点
            string[] splitstr = money.Split(ch[0]);
            if (splitstr.Length == 1) //只有整数部分
                return ConvertData(money) + "圆整";
            string rstr = ConvertData(splitstr[0]) + "圆";//转换整数部分
            rstr += ConvertXiaoShu(splitstr[1]);//转换小数部分
            return rstr;
        }

        /// <summary>
        /// 是否是正确的带小数点类型的数字
        /// </summary>
        /// <param name="decimalString"></param>
        /// <returns>返回bool值</returns>
        private static bool IsPositiveDecimal(string decimalString)
        {
            decimal d;
            try
            {
                d = decimal.Parse(decimalString);
            }
            catch (Exception)
            {
                return false;
            }
            return d > 0;
        }

        /// <summary>
        /// 转换数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ConvertData(string str)
        {
            string rstr = "";
            int strlen = str.Length;
            if (strlen <= 4)//数字长度小于四位
            {
                rstr = ConvertDigit(str);
            }
            else
            {
                string tmpstr;
                if (strlen <= 8)//数字长度大于四位，小于八位
                {
                    tmpstr = str.Substring(strlen - 4, 4);//先截取最后四位数字
                    rstr = ConvertDigit(tmpstr);//转换最后四位数字
                    tmpstr = str.Substring(0, strlen - 4);//截取其余数字
                    //将两次转换的数字加上万后相连接
                    rstr = String.Concat(ConvertDigit(tmpstr) + "万", rstr);
                    rstr = rstr.Replace("零万", "万");
                    rstr = rstr.Replace("零零", "零");
                }
                else if (strlen <= 12)//数字长度大于八位，小于十二位
                {
                    tmpstr = str.Substring(strlen - 4, 4);//先截取最后四位数字
                    rstr = ConvertDigit(tmpstr);//转换最后四位数字
                    tmpstr = str.Substring(strlen - 8, 4);//再截取四位数字
                    rstr = String.Concat(ConvertDigit(tmpstr) + "万", rstr);
                    tmpstr = str.Substring(0, strlen - 8);
                    rstr = String.Concat(ConvertDigit(tmpstr) + "亿", rstr);
                    rstr = rstr.Replace("零亿", "亿");
                    rstr = rstr.Replace("零万", "万");
                    rstr = rstr.Replace("零零", "零");
                    rstr = rstr.Replace("零零", "零");
                    rstr = rstr.Replace("亿万", "亿");
                }
            }
            strlen = rstr.Length;
            if (strlen >= 2)
            {
                switch (rstr.Substring(strlen - 2, 2))
                {
                    case "佰零": rstr = rstr.Substring(0, strlen - 2) + "佰"; break;
                    case "仟零": rstr = rstr.Substring(0, strlen - 2) + "仟"; break;
                    case "万零": rstr = rstr.Substring(0, strlen - 2) + "万"; break;
                    case "亿零": rstr = rstr.Substring(0, strlen - 2) + "亿"; break;
                }
            }
            return rstr;
        }

        /// <summary>
        /// 转换位数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ConvertDigit(string str)
        {
            int strlen = str.Length;
            string rstr = string.Empty;
            switch (strlen)
            {
                case 1: rstr = NumberToChinese(str); break;
                case 2: rstr = Convert2Digit(str); break;
                case 3: rstr = Convert3Digit(str); break;
                case 4: rstr = Convert4Digit(str); break;
            }
            rstr = rstr.Replace("拾零", "拾");
            return rstr;
        }

        /// <summary>
        /// 转换四位数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Convert4Digit(string str)
        {
            string str1 = str.Substring(0, 1);
            string str2 = str.Substring(1, 1);
            string str3 = str.Substring(2, 1);
            string str4 = str.Substring(3, 1);
            string rstring = "";
            rstring += NumberToChinese(str1) + "仟";
            rstring += NumberToChinese(str2) + "佰";
            rstring += NumberToChinese(str3) + "拾";
            rstring += NumberToChinese(str4);
            rstring = rstring.Replace("零仟", "零");
            rstring = rstring.Replace("零佰", "零");
            rstring = rstring.Replace("零拾", "零");
            rstring = rstring.Replace("零零", "零");
            rstring = rstring.Replace("零零", "零");
            rstring = rstring.Replace("零零", "零");
            return rstring;
        }

        /// <summary>
        /// 转换三位数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Convert3Digit(string str)
        {
            string str1 = str.Substring(0, 1);
            string str2 = str.Substring(1, 1);
            string str3 = str.Substring(2, 1);
            string rstring = NumberToChinese(str1) + "佰";
            rstring += NumberToChinese(str2) + "拾";
            rstring += NumberToChinese(str3);
            rstring = rstring.Replace("零佰", "零");
            rstring = rstring.Replace("零拾", "零");
            rstring = rstring.Replace("零零", "零");
            rstring = rstring.Replace("零零", "零");
            return rstring;
        }

        /// <summary>
        /// 转换二位数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Convert2Digit(string str)
        {
            string str1 = str.Substring(0, 1);
            string str2 = str.Substring(1, 1);
            string rstring = NumberToChinese(str1) + "拾";
            rstring += NumberToChinese(str2);
            rstring = rstring.Replace("零拾", "零");
            rstring = rstring.Replace("零零", "零");
            return rstring;
        }

        /// <summary>
        /// 转换小数后的中文数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ConvertXiaoShu(string str)
        {
            int strlen = str.Length;
            string rstr;
            if (strlen == 1)
            {
                rstr = NumberToChinese(str) + "角整";
                return rstr;
            }
            var tmpstr = str.Substring(0, 1);
            rstr = NumberToChinese(tmpstr) + "角";
            tmpstr = str.Substring(1, 1);
            rstr += NumberToChinese(tmpstr) + "分";
            rstr = rstr.Replace("零角", "零");
            rstr = rstr.Replace("零零分", "整");
            rstr = rstr.Replace("零分", "");
            return rstr;
        }

        /// <summary>
        /// 数字转换中文数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string NumberToChinese(string str)
        {
            //"零壹贰叁肆伍陆柒捌玖拾佰仟万亿圆整角分"
            string cstr = string.Empty;
            switch (str)
            {
                case "0": cstr = "零"; break;
                case "1": cstr = "壹"; break;
                case "2": cstr = "贰"; break;
                case "3": cstr = "叁"; break;
                case "4": cstr = "肆"; break;
                case "5": cstr = "伍"; break;
                case "6": cstr = "陆"; break;
                case "7": cstr = "柒"; break;
                case "8": cstr = "捌"; break;
                case "9": cstr = "玖"; break;
            }
            return cstr;
        }
    }
}
