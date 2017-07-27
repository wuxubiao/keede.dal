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
            if (cmdText.Trim().ToLower().StartsWith("select"))
                return true;
            else
                return false;
        }
    }   
}
