using System.Text;
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
    }
}
