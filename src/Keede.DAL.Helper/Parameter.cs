using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Keede.DAL.Helper
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class Parameter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static IEnumerable<Parameter> Get(params SqlParameter[] parameters)
        {
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    yield return new Parameter(p.ParameterName, p.Value);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Parameter(string name, object value)
        {
            if (!name.StartsWith("@"))
            {
                name = "@" + name;
            }
            Name = name;
            Value = value;
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public Parameter(string name, object value, ParameterDirection direction)
        {
            if (!name.StartsWith("@"))
            {
                name = "@" + name;
            }
            Name = name;
            Value = value;
            Direction = direction;
        }

        /// <summary>
        ///
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public ParameterDirection Direction { get; private set; }
    }
}