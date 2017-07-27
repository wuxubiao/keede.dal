using System;
using System.Collections.Generic;
using System.Reflection;

namespace Keede.DAL.Helper.Common
{
    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class AttributeUtility
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TArrtibute> GetEnumAttributes<TEnum, TArrtibute>(bool inhert = false) where TArrtibute : Attribute
        {
            var type = typeof(TEnum);
            if (type.IsEnum)
            {
                var fields = type.GetFields();
                foreach (var fd in fields)
                {
                    var tas = (TArrtibute[])fd.GetCustomAttributes(typeof(TArrtibute), inhert);
                    if (tas.Length > 0)
                    {
                        yield return tas[0];
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TArrtibute> GetEnumAttributes<TArrtibute>(Type enumType, bool inhert = false) where TArrtibute : Attribute
        {
            var type = enumType;
            if (type.IsEnum)
            {
                var fields = type.GetFields();
                foreach (var fd in fields)
                {
                    var tas = (TArrtibute[])fd.GetCustomAttributes(typeof(TArrtibute), inhert);
                    if (tas.Length > 0)
                    {
                        yield return tas[0];
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TArrtibute GetEnumAttribute<TEnum, TArrtibute>(TEnum e, bool inhert = false) where TArrtibute : Attribute
        {
            var type = typeof(TEnum);
            if (type.IsEnum)
            {
                var field = type.GetField(e.ToString());
                var items = GetAttributes<TArrtibute>(field, inhert);
                if (items != null && items.Length > 0)
                {
                    return items[0];
                }
            }
            return default(TArrtibute);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TArrtibute GetEnumAttribute<TEnum, TArrtibute>(bool inhert = false) where TArrtibute : Attribute
        {
            var type = typeof(TEnum);
            if (type.IsEnum)
            {
                var items = GetAttributes<TArrtibute>(type, inhert);
                if (items != null && items.Length > 0)
                {
                    return items[0];
                }
            }
            return default(TArrtibute);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <param name="field"></param>
        /// <param name="inhert"></param>
        /// <returns></returns>
        public static TArrtibute[] GetAttributes<TArrtibute>(FieldInfo field, bool inhert = false) where TArrtibute : Attribute
        {
            var abs = (TArrtibute[])field.GetCustomAttributes(typeof(TArrtibute), inhert);
            return abs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inhert"></param>
        /// <returns></returns>
        public static TArrtibute[] GetAttributes<TArrtibute>(Type type, bool inhert = false) where TArrtibute : Attribute
        {
            var abs = (TArrtibute[])type.GetCustomAttributes(typeof(TArrtibute), inhert);
            return abs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <param name="pi"></param>
        /// <param name="inhert"></param>
        /// <returns></returns>
        public static TArrtibute[] GetAttributes<TArrtibute>(PropertyInfo pi, bool inhert = false) where TArrtibute : Attribute
        {
            var abs = (TArrtibute[])pi.GetCustomAttributes(typeof(TArrtibute), inhert);
            return abs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <param name="mi"></param>
        /// <param name="inhert"></param>
        /// <returns></returns>
        public static TArrtibute[] GetAttributes<TArrtibute>(MethodInfo mi, bool inhert = false) where TArrtibute : Attribute
        {
            var abs = (TArrtibute[])mi.GetCustomAttributes(typeof(TArrtibute), inhert);
            return abs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TArrtibute"></typeparam>
        /// <param name="pi"></param>
        /// <param name="inhert"></param>
        /// <returns></returns>
        public static TArrtibute[] GetAttributes<TArrtibute>(ParameterInfo pi, bool inhert = false) where TArrtibute : Attribute
        {
            var abs = (TArrtibute[])pi.GetCustomAttributes(typeof(TArrtibute), inhert);
            return abs;
        }
    }
}