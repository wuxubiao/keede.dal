using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// DES����/������
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class DES
    {
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="text">�����ı�</param>
        /// <param name="key">���ܹؼ���</param>
        /// <returns></returns>
        public static string Encrypt(string text, string key)
        {
            var keyMd5Value = FormsAuthentication.HashPasswordForStoringInConfigFile(key, "md5");
            if (keyMd5Value != null)
            {
                var keys = Encoding.ASCII.GetBytes(keyMd5Value.Substring(0, 8));
                var iv = Encoding.ASCII.GetBytes(keyMd5Value.Substring(8, 8));
                return Encrypt(text, keys, iv);
            }
            return string.Empty;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="text">�����ı�</param>
        /// <param name="key">���ܹؼ���</param>
        /// <param name="iv">���ܵĶԳ��㷨</param>
        /// <returns></returns>
        public static string Encrypt(string text, byte[] key, byte[] iv)
        {
            var des = new DESCryptoServiceProvider();
            var inputByteArray = Encoding.Default.GetBytes(text);
            des.Key = key;
            des.IV = iv;
            var ms = new System.IO.MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="text">�����ı�</param>
        /// <param name="key">���ܹؼ���</param>
        /// <returns></returns>
        public static string Decrypt(string text, string key)
        {
            var keyMd5Value = FormsAuthentication.HashPasswordForStoringInConfigFile(key, "md5");
            if (keyMd5Value != null)
            {
                var keys = Encoding.ASCII.GetBytes(keyMd5Value.Substring(0, 8));
                var iv = Encoding.ASCII.GetBytes(keyMd5Value.Substring(8, 8));
                return Decrypt(text, keys, iv);
            }
            return string.Empty;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="text">�����ı�</param>
        /// <param name="key">���ܹؼ���</param>
        /// <param name="iv">���ܵĶԳ��㷨</param>
        /// <returns></returns>
        public static string Decrypt(string text, byte[] key, byte[] iv)
        {
            var des = new DESCryptoServiceProvider();
            var len = text.Length / 2;
            var inputByteArray = new byte[len];
            int x;
            for (x = 0; x < len; x++)
            {
                var i = Convert.ToInt32(text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = key;
            des.IV = iv;
            var ms = new System.IO.MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }
    }
}