using System;
using System.Globalization;

namespace Framework.Core.Utility
{
    /// <summary>
    /// 日期帮助类
    /// </summary>
    public class DateTimeUtility
    {
        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static long GetTimeStamp(DateTime? initiationDateTime = null)
        {
            if (initiationDateTime == null) initiationDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var ts = DateTime.UtcNow - initiationDateTime.Value;
            return Convert.ToInt64(ts.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).Replace(".", string.Empty));
        }
    }
}
