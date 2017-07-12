using System;
using System.Linq;
using System.Reflection;
using Dapper;

namespace Dapper.Extension
{
    public static class TypeMapper
    {
        public static void Initialize(string @namespace)
        {
            var types = from type in Assembly.GetCallingAssembly().GetTypes()//GetExecutingAssembly
                        where type.IsClass && type.Namespace == @namespace
                        select type;

            types.ToList().ForEach(type =>
            {
                var map = new CustomPropertyTypeMap(type,
                          (type_, columnName) => type_.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName));
                SqlMapper.SetTypeMap(type, map);
            });
        }

        public static void SetTypeMap(Type type)
        {
            //string.Equals(GetDescriptionFromAttribute(prop), columnName, StringComparison.Ordinal)));//
            var map = new CustomPropertyTypeMap(type,
                          (type_, columnName) => type_.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName));
            SqlMapper.SetTypeMap(type, map);
        }

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
