using System;

namespace Keede.SQLHelper.Attributes
{
    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IDAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public IDAttribute(string name)
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