using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Dapper.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public static class TypeMapper
    {
        private static readonly Hashtable _typeMaps = new Hashtable();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespace"></param>
        public static void Initialize(string @namespace)
        {
            var types = from type in Assembly.GetCallingAssembly().GetTypes()//GetExecutingAssembly
                        where type.IsClass && type.Namespace == @namespace
                        select type;

            types.ToList().ForEach(type =>
            {
                var map = new CustomPropertyTypeMap(type,
                          (type_, columnName) => type_.GetProperties().FirstOrDefault(prop => string.Equals(GetDescriptionFromAttribute(prop), columnName, StringComparison.OrdinalIgnoreCase)));//GetDescriptionFromAttribute(prop) == columnName));
                SqlMapper.SetTypeMap(type, map);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="overWrite"></param>
        public static void SetTypeMap(Type type, bool overWrite=false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var tableAttr = type .GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TypeMapperAttribute");
            if (tableAttr != null)
            {
                var map = (CustomPropertyTypeMap)_typeMaps[type];

                //string.Equals(GetDescriptionFromAttribute(prop), columnName, StringComparison.OrdinalIgnoreCase)));//
                if (overWrite || map == null)
                {
                    map = new CustomPropertyTypeMap(type,
                                  (type_, columnName) => type_.GetProperties().FirstOrDefault(prop => string.Equals(GetDescriptionFromAttribute(prop), columnName, StringComparison.OrdinalIgnoreCase)));//GetDescriptionFromAttribute(prop) == columnName));
                    SqlMapper.SetTypeMap(type, map);
                    lock (_typeMaps)
                    {
                        _typeMaps[type] = map;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;
#if COREFX
            var data = member.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute));
            return (string)data?.ConstructorArguments.Single().Value;
#else
            var attrib = (ColumnAttribute)Attribute.GetCustomAttribute(member, typeof(ColumnAttribute), false);
            if (attrib == null) return member.Name;
            return attrib?.Name;
#endif
        }
    }
}
