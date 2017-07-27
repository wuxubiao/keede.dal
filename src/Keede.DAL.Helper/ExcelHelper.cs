using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace Keede.DAL.Helper
{
    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public static class ExcelHelper
    {
        private static DataSet _myDataSet;

        /// <summary>
        /// 得到Excel文件的Sheet1的数据（用DataSet返回）
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataSet GetDataSet(string strFilePath)
        {
            string strCon = String.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;IMEX=1; 8.0;HDR=1;'", strFilePath);
            var myConn = new OleDbConnection(strCon);
            const string STR_COM = " SELECT * FROM [Sheet1$] ";
            myConn.Open();
            var myAdapter = new OleDbDataAdapter(STR_COM, myConn);
            _myDataSet = new DataSet();
            myAdapter.Fill(_myDataSet, "[Sheet1$]");
            myConn.Close();
            return _myDataSet;
        }

        /// <summary>
        /// 得到Excel文件的Sheet1的数据（用DataSet返回）
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataSet GetDataSet(string strFilePath, string sheetName)
        {
            string strCon = String.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;IMEX=1; 8.0;HDR=1;'", strFilePath);
            var myConn = new OleDbConnection(strCon);
            string strCom = String.Format(" SELECT * FROM [{0}$] ", sheetName);
            myConn.Open();
            var myAdapter = new OleDbDataAdapter(strCom, myConn);
            _myDataSet = new DataSet();
            myAdapter.Fill(_myDataSet, "[Sheet1$]");
            myConn.Close();
            return _myDataSet;
        }

        /// <summary>
        /// 得到Excel文件的Sheet1的数据（用DataReader返回）
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static OleDbDataReader GetDataReader(string strFilePath)
        {
            string strCon = String.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;IMEX=1; 8.0;HDR=1;'", strFilePath);
            var myConn = new OleDbConnection(strCon);
            const string STR_COM = " SELECT * FROM [Sheet1$] ";
            myConn.Open();
            var myCommand = new OleDbCommand(STR_COM, myConn);
            OleDbDataReader reader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        /// <summary>
        /// 得到Excel文件的Sheet1的数据（用DataReader返回）
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <param name="sheetName"></param>
        /// <param name="sType"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static OleDbDataReader GetDataReader(string strFilePath, string sheetName, SheetType sType)
        {
            string strCon = String.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'", strFilePath);
            var myConn = new OleDbConnection(strCon);
            try
            {
                if (myConn.State == ConnectionState.Closed)
                    myConn.Open();
            }
            catch
            {
                try
                {
                    myConn.Close();
                    //strCon = String.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=Yes'", strFilePath);
                    myConn = new OleDbConnection(strCon);
                    myConn.Open();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }

            string strCom = String.Format(" SELECT * FROM [{0}$] ", sheetName);
            switch (sType)
            {
                case SheetType.FirstSheet:
                    strCom = String.Format(" SELECT * FROM [{0}] ", GetFirstSheetName(myConn));
                    break;

                case SheetType.LastSheet:
                    strCom = String.Format(" SELECT * FROM [{0}] ", GetLastSheetName(myConn));
                    break;

                case SheetType.SheetOne:
                    break;
            }
            var myCommand = new OleDbCommand(strCom, myConn);
            OleDbDataReader reader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        /// <summary>
        /// 得到Excel文件的Sheet1的数据（用DataReader返回）
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="strFilePath"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static OleDbDataReader GetDataReader(SheetType sType, string strFilePath, params string[] sheetName)
        {
            if (sheetName.Length > 1)
            {
                throw new ApplicationException("keede提示：Sheet名字只能为一个 或者没有");
            }
            if (sType == SheetType.AppointSheet && sheetName.Length == 0)
            {
                throw new ApplicationException("keede提示：SheetType为AppointSheet(指定Sheet)时，sheetName参数必须且只能有一个");
            }

            if (sType == SheetType.SheetOne)
                return GetDataReader(strFilePath);
            if (sType == SheetType.AppointSheet)
                return GetDataReader(strFilePath, sheetName[0], sType);
            return GetDataReader(strFilePath, "", sType);
        }

        /// <summary>
        /// 得到第一个Sheet
        /// </summary>
        /// <param name="oledbconn1"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static string GetFirstSheetName(OleDbConnection oledbconn1)
        {
            DataTable dt = oledbconn1.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0]["TABLE_NAME"].ToString();
            return "NotExist";
        }

        /// <summary>
        /// 得到最后一个Sheet
        /// </summary>
        /// <param name="oledbconn1"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static string GetLastSheetName(OleDbConnection oledbconn1)
        {
            DataTable dt = oledbconn1.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[dt.Rows.Count - 1]["TABLE_NAME"].ToString();
            return "NotExist";
        }

        /// <summary>
        /// 得到Sheet列表
        /// </summary>
        /// <param name="oledbconn1"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static IList<string> GetFirstSheetNameList(OleDbConnection oledbconn1)
        {
            IList<string> sheetNameList = new List<string>();
            DataTable dt = oledbconn1.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            if (dt != null)
                foreach (DataRow dr in dt.Rows)
                {
                    sheetNameList.Add(dr["TABLE_NAME"].ToString());
                }
            return sheetNameList;
        }

    }

    /// <summary>
    ///
    /// </summary>
    public enum SheetType
    {
        /// <summary>
        ///
        /// </summary>
        SheetOne = 0,

        /// <summary>
        /// 第一个Sheet
        /// </summary>
        FirstSheet = 1,

        /// <summary>
        /// 指定Sheet
        /// </summary>
        AppointSheet = 2,

        /// <summary>
        /// 最后一个Sheet
        /// </summary>
        LastSheet = 3
    }
}