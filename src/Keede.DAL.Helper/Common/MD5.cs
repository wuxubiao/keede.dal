using System;
using System.Web.Security;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// ���ܹ���
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class MD5
    {
        /// <summary>
        /// MD5����
        /// </summary>
        public static string Encrypt(string value)
        {
            return Encrypt(value, "00000000000000000000000000000000");
        }

        /// <summary>
        /// MD5����
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
        /// ����MD5ժҪ���ܣ����ԶԼ��ܽ�����н�ȡ
        ///</summary>
        ///<param name="value">Դ�ַ���</param>
        ///<param name="start">��ȡ��ʼλ��</param>
        ///<param name="length">��ȡ����</param>
        public static string EncryptSubstring(string value, int start, int length)
        {
            return Encrypt(value).Substring(start, length);
        }
    }
}