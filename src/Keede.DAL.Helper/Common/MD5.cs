using System;
using System.Web.Security;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// 加密功能
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class MD5
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        public static string Encrypt(string value)
        {
            return Encrypt(value, "00000000000000000000000000000000");
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        public static string Encrypt(string value, string defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }
            var md5 = FormsAuthentication.HashPasswordForStoringInConfigFile(value, "MD5");
            return md5?.ToLower() ?? defaultValue;
        }

        ///<summary>
        /// 生成MD5摘要加密，可以对加密结果进行截取
        ///</summary>
        ///<param name="value">源字符串</param>
        ///<param name="start">截取开始位置</param>
        ///<param name="length">截取长度</param>
        public static string EncryptSubstring(string value, int start, int length)
        {
            return Encrypt(value).Substring(start, length);
        }
    }
}