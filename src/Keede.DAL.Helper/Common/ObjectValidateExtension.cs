using System;
using System.Text.RegularExpressions;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    /// �ַ�������֤��չ
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public static class ObjectValidateExtension
    {
        private static readonly Regex _regexEmail = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        private static readonly Regex _regexRegisterUsername = new Regex(@"^[0-9A-Za-z\u2E80-\u9FFF_\.@-]+$");
        private static readonly Regex _regexMobile = new Regex(@"^((1\d{10})|([569]\d{7}))$");
        private static readonly Regex _regexPhone = new Regex(@"^([0\+]\d{2,3}-?)?(0\d{2,3}-?)?(\d{7,8})([- ]+\d{1,6})?$");
        private static readonly Regex _regexInt = new Regex(@"^[\+\-]?[1-9][0-9]*$");
        private static readonly Regex _regexNumber = new Regex(@"^[0-9]+$");
        private static readonly Regex _regexGuid = new Regex(@"^[A-Fa-f0-9]{8}\-[A-Fa-f0-9]{4}\-[A-Fa-f0-9]{4}\-[A-Fa-f0-9]{4}\-[A-Fa-f0-9]{12}$");
        private static readonly Regex _regexEngLetter = new Regex(@"^[A-Za-z]+$");
        private static readonly Regex _regexUrl = new Regex(@"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        private static readonly Regex _regexChinese = new Regex(@"^[\u2E80-\u9FFF]+$");

        /// <summary>
        /// ��֤�ַ����ǲ��Ǻ�������ʽƥ��
        /// </summary>
        /// <param name="regex">������ʽ</param>
        /// <param name="value">����֤���ַ���</param>
        /// <returns></returns>
        public static bool IsValidate(Regex regex, string value)
        {
            return !string.IsNullOrEmpty(value) && regex.IsMatch(value);
        }

        /// <summary>
        /// �ж��ǲ���һ�������ַ
        /// </summary>
        /// <param name="value">�ʼ���ַ</param>
        /// <returns></returns>
        public static bool IsEmail(this string value)
        {
            return IsValidate(_regexEmail, value);
        }

        /// <summary>
        /// �ж��ǲ���һ���û����ַ���
        /// </summary>
        /// <param name="value">�û�����</param>
        /// <returns></returns>
        public static bool IsRegisterUserName(this string value)
        {
            return IsValidate(_regexRegisterUsername, value);
        }

        /// <summary>
        /// �ж��ǲ���һ���ֻ�����
        /// </summary>
        /// <param name="value">�ֻ�����</param>
        /// <returns></returns>
        public static bool IsMobile(this string value)
        {
            return IsValidate(_regexMobile, value);
        }

        /// <summary>
        /// ��֤�Ƿ���һ�������绰����
        /// </summary>
        /// <param name="value">�����绰����</param>
        /// <returns></returns>
        public static bool IsPhone(this string value)
        {
            return IsValidate(_regexPhone, value);
        }

        /// <summary>
        /// �ж��ǲ���һ������
        /// </summary>
        /// <param name="value">����</param>
        /// <returns></returns>
        public static bool IsInt(this string value)
        {
            return IsValidate(_regexInt, value);
        }

        /// <summary>
        /// �ж��ǲ��Ǵ����֣��������Ӽ���
        /// </summary>
        /// <param name="value">����</param>
        /// <returns></returns>
        public static bool IsNumericString(this string value)
        {
            return IsValidate(_regexNumber, value);
        }

        /// <summary>
        /// �ж��ǲ���Guid
        /// </summary>
        /// <param name="value">Guid�ַ���</param>
        /// <returns></returns>
        public static bool IsGuid(this string value)
        {
            return IsValidate(_regexGuid, value);
        }

        /// <summary>
        /// �ж��ǲ���ȫ����ĸ
        /// </summary>
        /// <param name="value">��ĸ</param>
        /// <returns></returns>
        public static bool IsEnglishLetters(this string value)
        {
            return IsValidate(_regexEngLetter, value);
        }

        /// <summary>
        /// �жϲ����ǲ���URL��ַ
        /// </summary>
        /// <param name="value">��ҳ��ַ</param>
        /// <returns></returns>
        public static bool IsUrl(this string value)
        {
            return IsValidate(_regexUrl, value);
        }

        /// <summary>
        /// �ж��ǲ��Ǵ������ַ���
        /// </summary>
        public static bool IsChineseString(this string value)
        {
            return IsValidate(_regexChinese, value);
        }

        /// <summary>
        /// �ж϶����Ƿ�Ϊ��
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNull(this object value)
        {
            return ReferenceEquals(value, null);
        }

        /// <summary>
        /// �ж��Ƿ�Ϊ�ջ���NULLֵ
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}