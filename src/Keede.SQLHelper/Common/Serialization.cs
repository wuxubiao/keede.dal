using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Framework.Common
{
    /// <summary>
    /// 序列化
    /// </summary>
    public class Serialization
    {
        /// <summary>
        /// 序列化某对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Byte[] SerializationObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            stream.Seek(0, 0);
            byte[] returnBytes = stream.ToArray();
            stream.Close();
            return returnBytes;
        }

        /// <summary>
        /// 反序列化某对象
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static object DeserializationObject(Byte[] content)
        {
            var stream = new MemoryStream(content);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            stream.Position = 0;
            try
            {
                return binaryFormatter.Deserialize(stream);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// 反序列化某对象
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T DeserializationObject<T>(Byte[] content)
        {
            var stream = new MemoryStream(content);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            stream.Position = 0;
            try
            {
                return (T)binaryFormatter.Deserialize(stream);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static string ToJsonString(object obj)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    return serializer.Serialize(obj);
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize(object obj)
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                var json = serializer.Serialize(obj);
                json = Regex.Replace(json, @"\\/Date\((\d+)\)\\/", match =>
                {
                    DateTime dt = new DateTime(1970, 1, 1);
                    dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                    dt = dt.ToLocalTime();
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");
                });
                return json;
            }
            catch
            {
                //LogHelper.Log("JsonSerialize", "JSON序列化错误", "类型:" + obj.GetType().FullName);
                return string.Empty;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string data)
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                return serializer.Deserialize<T>(data);
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Format("类型:{0}", typeof(T).FullName));
                sb.AppendLine("数据:");
                sb.AppendLine(data);
                //LogHelper.Log("JsonDeserialize", "JSON反序列化错误", sb.ToString());
                return default(T);
            }
        }
    }
}