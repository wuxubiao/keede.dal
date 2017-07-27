using System;

namespace Keede.DAL.Helper.Attributes
{
    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("This class is obsolete,don't use it in new project")]
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