﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Data
{
    public static partial class SqlHelper
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
        //   IsRead(cmdText) ? Databases.GetSqlConnection() :  Databases.GetSqlConnection(false))
        //    Databases.GetSqlConnection()
        //    Databases.GetDbConnectionStr()
}