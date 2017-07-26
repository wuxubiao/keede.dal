using System;

namespace Keede.SQLHelper.Attributes
{
    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}