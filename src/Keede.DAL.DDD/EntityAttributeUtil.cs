using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Dapper.Extension;

namespace Keede.DAL.DDD
{
    /// <summary>
    /// 实体特性帮助类
    /// </summary>
    public class EntityAttributeUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetId<TEntity>(TEntity entity) where TEntity : IEntity
        {
            var type = entity.GetType();
            StringBuilder idBuilder =new StringBuilder();

            var tableName = SqlMapperExtensions.GetTableName(type);
            idBuilder.Append(tableName+"_");

            foreach (var propertyInfo in type.GetProperties())
            {
                var attr = Utility.AttributeUtility.GetAttribute<Dapper.Extension.ExplicitKeyAttribute>(propertyInfo,true);
                if (attr != null)
                {
                    if (idBuilder.Length > 0) idBuilder.Append("_");
                    idBuilder.Append(propertyInfo.GetValue(entity, null));
                }
            }
            return idBuilder.ToString();
        }

        public static string GetId(string tableName, dynamic condition)
        {
            if (condition == null)
                throw new ArgumentNullException();

            StringBuilder idBuilder =new StringBuilder();
            idBuilder.Append(tableName + "_");

            var conditionObj = condition as object;

            if (conditionObj is DynamicParameters)
            {
                var dyParam = (conditionObj as DynamicParameters);
                 var paraName= dyParam.ParameterNames.ToList();
                foreach (var VARIABLE in paraName)
                {
                    idBuilder.Append(dyParam.Get<object>(VARIABLE));
                }
            }
            else
            {
                var properties = conditionObj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).ToList();
                if(properties.Count==0)
                    throw new ArgumentNullException();

                foreach (var propertyInfo in properties)
                {
                    if (idBuilder.Length > 0) idBuilder.Append("_");
                    idBuilder.Append(propertyInfo.GetValue(condition, null));
                }
            }

            return idBuilder.ToString();
        }
    }
}
