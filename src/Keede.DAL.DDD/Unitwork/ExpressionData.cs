using System;
using System.Linq.Expressions;

namespace Keede.DAL.DDD.Unitwork
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ExpressionData
    {
        /// <summary>
        /// 
        /// </summary>
        public dynamic Data { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Expression WhereExpression { get; set; }

        public Type EntityType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="data"></param>
        public ExpressionData(Type entityType, Expression whereExpression, dynamic data)
        {
            EntityType = entityType;
            WhereExpression = whereExpression;
            Data = data;
        }
    }
}
