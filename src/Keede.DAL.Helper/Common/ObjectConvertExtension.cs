using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public static class ObjectConvertExtension
    {
        #region > 数字扩展

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBool(this int value)
        {
            if (value == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <remarks>
        /// 注意在转换之前先判断能否可转，扩展方法：IsInt()
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this double value)
        {
            return int.Parse(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 补足数字位数
        /// </summary>
        /// <returns></returns>
        public static string ToMendString(this long number, int length)
        {
            if (number.ToString(CultureInfo.InvariantCulture).Length < 8)
            {
                var sb = new StringBuilder();
                var ce = length - number.ToString(CultureInfo.InvariantCulture).Length;
                if (ce > 0)
                {
                    for (int i = 0; i < ce; i++)
                    {
                        sb.Append("0");
                    }
                }
                sb.Append(number.ToString(CultureInfo.InvariantCulture));
                return sb.ToString();
            }
            return number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 补足数字位数
        /// </summary>
        /// <returns></returns>
        public static string ToMendString(this int number, int length)
        {
            if (number.ToString(CultureInfo.InvariantCulture).Length < 8)
            {
                var sb = new StringBuilder();
                var ce = length - number.ToString(CultureInfo.InvariantCulture).Length;
                if (ce > 0)
                {
                    for (int i = 0; i < ce; i++)
                    {
                        sb.Append("0");
                    }
                }
                sb.Append(number.ToString(CultureInfo.InvariantCulture));
                return sb.ToString();
            }
            return number.ToString(CultureInfo.InvariantCulture);
        }

        #endregion > 数字扩展

        #region > 时间类型扩展

        /// <summary>
        /// 计算天数间隔
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="targetDateTime">目标日期</param>
        /// <returns></returns>
        public static int DaySpan(this DateTime dateTime, DateTime targetDateTime)
        {
            var day = (targetDateTime - dateTime).TotalDays;
            return Math.Abs((int)day);
        }

        #endregion > 时间类型扩展

        #region > 字节 Byte 的扩展

        /// <summary>
        /// 转换到16位字符串
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string ToHex(this byte b)
        {
            return b.ToString("X2");
        }

        /// <summary>
        /// 转换到16位字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHex(this IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2") + " ");
            return sb.ToString();
        }

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string RestoreString(this IEnumerable<byte> bytes)
        {
            return bytes.RestoreString(Encoding.Default);
        }

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="inputCharset"></param>
        /// <returns></returns>
        public static string RestoreString(this IEnumerable<byte> bytes, string inputCharset)
        {
            if (string.IsNullOrEmpty(inputCharset))
            {
                return bytes.RestoreString();
            }
            return bytes.RestoreString(Encoding.GetEncoding(inputCharset));
        }

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string RestoreString(this IEnumerable<byte> bytes, Encoding encoding)
        {
            if (bytes == null || !bytes.Any())
            {
                return null;
            }
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }

            return encoding.GetString(bytes.ToArray());
        }

        #endregion > 字节 Byte 的扩展

        #region >流类型扩展

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RestoreString(this Stream content)
        {
            return content.RestoreString(Encoding.Default);
        }

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="content"></param>
        /// <param name="inputCharset"></param>
        /// <returns></returns>
        public static string RestoreString(this Stream content, string inputCharset)
        {
            if (string.IsNullOrEmpty(inputCharset))
            {
                return content.RestoreString();
            }
            return content.RestoreString(Encoding.GetEncoding(inputCharset));
        }

        /// <summary>
        /// 还原到字符串值
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"> </param>
        /// <returns></returns>
        public static string RestoreString(this Stream content, Encoding encoding)
        {
            if (content == null || content.Length == 0)
            {
                return null;
            }

            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            var bytes = new byte[content.Length];
            content.Position = 0;
            content.Read(bytes, 0, bytes.Length);

            return bytes.RestoreString(encoding);
        }

        #endregion

        #region > 字符串类型扩展

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBool(this string value)
        {
            if (value.ToLower() == "true")
            {
                return true;
            }
            if (value == "1")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDouble(this string value)
        {
            double val;
            double.TryParse(value, out val);
            return val;
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this string value)
        {
            decimal val;
            decimal.TryParse(value, out val);
            return val;
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <remarks>
        /// 注意在转换之前先判断能否可转，使用扩展方法：CanToInt()
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this string value)
        {
            if (!value.IsInt())
            {
                return 0;
            }
            return int.Parse(value);
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <remarks>
        /// 注意在转换之前先判断能否可转，使用扩展方法：CanToInt()
        /// </remarks>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static int ToInt(this char chr)
        {
            return int.Parse(chr.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 正则匹配判断
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsMatch(this string value, string pattern)
        {
            return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// 正则匹配判断
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string Match(this string value, string pattern)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : Regex.Match(value, pattern).Value;
        }

        /// <summary>
        /// 转换到编码字节组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string value)
        {
            return Encoding.Default.GetBytes(value);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToMD5String(this string value)
        {
            return MD5.Encrypt(value);
        }

        /// <summary>
        /// 转换到Base64位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(value.ToBytes());
        }

        /// <summary>
        /// 还原到正常字符串值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RestoreBase64String(this string value)
        {
            var bytes = Convert.FromBase64String(value);
            return bytes.RestoreString();
        }

        /// <summary>
        /// 转换到GUID
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Guid.Empty;
            }
            if (value.IsGuid())
            {
                return Guid.Parse(value);
            }
            return Guid.Empty;
        }

        /// <summary>
        /// 批量转换到GUID
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<Guid> ToGuids(this IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    yield return Guid.Empty;
                }
                else if (value.IsGuid())
                {
                    yield return Guid.Parse(value);
                }
                yield return Guid.Empty;
            }
        }

        /// <summary>
        /// 指定替换字符串中的占位符
        /// </summary>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string DoFormat(this string value, params object[] obj)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return string.Format(value, obj);
            }
            return value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value)
        {
            DateTime time;
            DateTime.TryParse(value, out time);
            return time;
        }

        #endregion > 字符串类型扩展

        #region > 其它扩展

        /// <summary>
        /// 转换到DBNull
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object ToDBNull(this object obj)
        {
            return obj ?? DBNull.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(this object obj)
        {
            if (ReferenceEquals(obj,null))
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        /// <summary>
        /// 转换到中文货币格式
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static string ToChineseMoney(this decimal money)
        {
            return ChineseCurrency.ToChineseChineseCurrency(money.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 是否在两个值范围内
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="lowerBound">最低界限值</param>
        /// <param name="upperBound">最高界限值</param>
        /// <param name="includeLowerBound">是否可以包含进最低界限值</param>
        /// <param name="includeUpperBound">是否可以包含进最高界限值</param>
        /// <returns></returns>
        public static bool IsBetween<T>(this T t, T lowerBound, T upperBound, bool includeLowerBound, bool includeUpperBound) where T : class, IComparable<T>
        {
            if (t == null) throw new ArgumentNullException("t");
            var lowerCompareResult = t.CompareTo(lowerBound);
            var upperCompareResult = t.CompareTo(upperBound);

            return (includeLowerBound && lowerCompareResult == 0) ||
                (includeUpperBound && upperCompareResult == 0) ||
                (lowerCompareResult > 0 && upperCompareResult < 0);
        }

        #endregion > 其它扩展
    }
}