using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// <para>中文字符的处理类</para>
    /// <para>date:2011-8-4，修正了正则对象为静态声明；原先的GetSpellCode函数变为GetInitialSpell</para>
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public static class ChineseCurrency
    {
        #region -- 拼音值，数字类型
        private static readonly int[] _pyValue =
        {
            -20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,
            -20032,-20026,-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,
            -19756,-19751,-19746,-19741,-19739,-19728,-19725,-19715,-19540,-19531,-19525,-19515,
            -19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263,-19261,-19249,
            -19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,
            -19003,-18996,-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,
            -18731,-18722,-18710,-18697,-18696,-18526,-18518,-18501,-18490,-18478,-18463,-18448,
            -18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, -18181,-18012,
            -17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,
            -17733,-17730,-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,
            -17468,-17454,-17433,-17427,-17417,-17202,-17185,-16983,-16970,-16942,-16915,-16733,
            -16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459,-16452,-16448,
            -16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,
            -16212,-16205,-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,
            -15933,-15920,-15915,-15903,-15889,-15878,-15707,-15701,-15681,-15667,-15661,-15659,
            -15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416,-15408,-15394,
            -15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,
            -15149,-15144,-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,
            -14941,-14937,-14933,-14930,-14929,-14928,-14926,-14922,-14921,-14914,-14908,-14902,
            -14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668,-14663,-14654,
            -14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,
            -14170,-14159,-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,
            -14109,-14099,-14097,-14094,-14092,-14090,-14087,-14083,-13917,-13914,-13910,-13907,
            -13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658,-13611,-13601,
            -13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,
            -13340,-13329,-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,
            -13068,-13063,-13060,-12888,-12875,-12871,-12860,-12858,-12852,-12849,-12838,-12831,
            -12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346,-12320,-12300,
            -12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,
            -11781,-11604,-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,
            -11055,-11052,-11045,-11041,-11038,-11024,-11020,-11019,-11018,-11014,-10838,-10832,
            -10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331,-10329,-10328,
            -10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254
        };

        #endregion -- 拼音值，数字类型

        #region -- 拼音字母
        private static readonly string[] _pyName =
        {
            "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
            "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Cai","Can",
            "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
            "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
            "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
            "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
            "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
            "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
            "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
            "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
            "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
            "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
            "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
            "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
            "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
            "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
            "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
            "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
            "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
            "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
            "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
            "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
            "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
            "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
            "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
            "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
            "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
            "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
            "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
            "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
            "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
            "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
            "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
        };

        #endregion -- 拼音字母

        /// <summary>
        /// 匹配中文字符
        /// </summary>
        private static readonly Regex _regexChinese = new Regex("^[\u4e00-\u9fa5]$");

        /// <summary>
        /// 获取汉字全拼
        /// </summary>
        /// <param name="hzString">汉字字符串</param>
        /// <returns></returns>
        public static string GetSpell(string hzString)
        {
            string pyString = string.Empty;

            char[] noWChar = hzString.ToCharArray();

            foreach (char chr in noWChar)
            {
                //中文字符
                if (_regexChinese.IsMatch(chr.ToString(CultureInfo.InvariantCulture)))
                {
                    byte[] array = Encoding.Default.GetBytes(chr.ToString(CultureInfo.InvariantCulture));
                    int i1 = array[0];
                    int i2 = array[1];
                    int chrAsc = i1 * 256 + i2 - 65536;
                    if (chrAsc > 0 && chrAsc < 160)
                    {
                        pyString += chr;
                    }
                    else
                    {
                        // 修正部分文字
                        if (chrAsc == -9254)  // 修正“圳”字
                            pyString += "Zhen";
                        else
                        {
                            for (int i = (_pyValue.Length - 1); i >= 0; i--)
                            {
                                if (_pyValue[i] > chrAsc) continue;

                                pyString += _pyName[i];
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 非中文字符
                    pyString += chr.ToString(CultureInfo.InvariantCulture);
                }
            }
            return pyString;
        }

        /// <summary>
        /// 获取每个字的首字母
        /// </summary>
        /// <param name="cnStr">汉字字符串</param>
        /// <returns>相对应的汉语拼音首字母串</returns>
        public static string GetInitialSpell(string cnStr)
        {
            string strTemp = string.Empty;

            int iLen = cnStr.Length;

            for (int i = 0; i <= iLen - 1; i++)
            {
                strTemp += GetCharSpellCode(cnStr[i]);
            }

            return strTemp;
        }

        /// <summary>
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母
        /// </summary>
        /// <param name="cnChar">单个汉字</param>
        /// <returns>单个大写字母</returns>
        private static string GetCharSpellCode(char cnChar)
        {
            long iCnChar;

            byte[] zw = Encoding.Default.GetBytes(cnChar.ToString(CultureInfo.InvariantCulture));

            //如果是字母，则直接返回
            switch (zw.Length)
            {
                case 1:
                    return cnChar.ToString(CultureInfo.InvariantCulture).ToUpper();
                default:
                    {
                        // get the array of byte from the single char
                        int i1 = zw[0];
                        int i2 = zw[1];
                        iCnChar = i1 * 256 + i2;
                    }
                    break;
            }

            // iCnChar match the constant
            if ((iCnChar >= 45217) && (iCnChar <= 45252))
                return "A";

            if ((iCnChar >= 45253) && (iCnChar <= 45760))
                return "B";

            if ((iCnChar >= 45761) && (iCnChar <= 46317))
                return "C";

            if ((iCnChar >= 46318) && (iCnChar <= 46825))
                return "D";

            if ((iCnChar >= 46826) && (iCnChar <= 47009))
                return "E";

            if ((iCnChar >= 47010) && (iCnChar <= 47296))
                return "F";

            if ((iCnChar >= 47297) && (iCnChar <= 47613))
                return "G";

            if ((iCnChar >= 47614) && (iCnChar <= 48118))
                return "H";

            if ((iCnChar >= 48119) && (iCnChar <= 49061))
                return "J";

            if ((iCnChar >= 49062) && (iCnChar <= 49323))
                return "K";

            if ((iCnChar >= 49324) && (iCnChar <= 49895))
                return "L";

            if ((iCnChar >= 49896) && (iCnChar <= 50370))
                return "M";

            if ((iCnChar >= 50371) && (iCnChar <= 50613))
                return "N";

            if ((iCnChar >= 50614) && (iCnChar <= 50621))
                return "O";

            if ((iCnChar >= 50622) && (iCnChar <= 50905))
                return "P";

            if ((iCnChar >= 50906) && (iCnChar <= 51386))
                return "Q";

            if ((iCnChar >= 51387) && (iCnChar <= 51445))
                return "R";

            if ((iCnChar >= 51446) && (iCnChar <= 52217))
                return "S";

            if ((iCnChar >= 52218) && (iCnChar <= 52697))
                return "T";

            if ((iCnChar >= 52698) && (iCnChar <= 52979))
                return "W";

            if ((iCnChar >= 52980) && (iCnChar <= 53688))
                return "X";

            if ((iCnChar >= 53689) && (iCnChar <= 54480))
                return "Y";

            if ((iCnChar >= 54481) && (iCnChar <= 55289))
                return "Z";

            return ("?");
        }

        #region -- 货币转换

        /// <summary>
        /// 转成汉字货币描述
        /// </summary>
        /// <param name="money">货币字符串</param>
        /// <returns></returns>
        public static string ToChineseChineseCurrency(string money)
        {
            if (!IsPositiveDecimal(money))
                return "";
            if (Double.Parse(money) > 999999999999.99)
                return "数字太大，无法换算，请输入一万亿元以下的金额";
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
        /// <returns></returns>
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
            return (cstr);
        }

        #endregion -- 货币转换
    }
}