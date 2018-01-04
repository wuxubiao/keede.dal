using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Dapper.Extension;
using System.Linq.Expressions;

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
            if (string.IsNullOrEmpty(tableName))
                return "";

            if (SqlMapperExtensions.KeyPropertiesCache(type).Count > 0)
            {
                if (idBuilder.Length > 0) idBuilder.Append("_");
                idBuilder.Append(entity.GetHashCode());
            }
            else
            {
                foreach (var propertyInfo in type.GetProperties())
                {
                    var attr = Utility.AttributeUtility.GetAttribute<ExplicitKeyAttribute>(propertyInfo, true);
                    if (attr != null)
                    {
                        if (idBuilder.Length > 0) idBuilder.Append("_");
                        idBuilder.Append(propertyInfo.GetValue(entity, null));
                    }
                }
            }

            if (idBuilder.Length > 0)
            {
                idBuilder.Insert(0, tableName + "_");
            }

            return idBuilder.ToString();
        }

        public static string GetId<TEntity>(Expression<Func<TEntity, bool>> whereExpression, dynamic data=null) where TEntity : IEntity
        {
            var type = whereExpression.Parameters[0].Type;
            StringBuilder idBuilder = new StringBuilder();

            var tableName = SqlMapperExtensions.GetTableName(type);
            idBuilder.Append(tableName + "_");

            var translate = new SqlTranslateFormater();
            string whereSql = translate.Translate(whereExpression);
            if(!string.IsNullOrEmpty(whereSql))
            {
                idBuilder.Append(whereSql);
            }

            if(data!=null)
            {
                var conditionObj = data as object;

                var wherePropertyInfos = SqlMapperExtensions.GetPropertyAndFieldName(conditionObj);

                foreach (var item in wherePropertyInfos)
                {
                    idBuilder.Append(item.Key.GetValue(conditionObj, null));
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
