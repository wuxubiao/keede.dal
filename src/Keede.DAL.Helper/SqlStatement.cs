using System;

namespace Keede.DAL.Helper
{
    public static class SqlStatement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public static bool IsRead(string cmdText)
        {
            if (cmdText.IndexOf("insert", StringComparison.OrdinalIgnoreCase) >0 || 
                cmdText.IndexOf("delete", StringComparison.OrdinalIgnoreCase) > 0|| 
                cmdText.IndexOf("update", StringComparison.OrdinalIgnoreCase) > 0||
                cmdText.IndexOf("truncate", StringComparison.OrdinalIgnoreCase) > 0)
                return false;
            else
                return true;
        }
    }   
}
