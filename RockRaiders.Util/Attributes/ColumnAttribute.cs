using System;

namespace RockRaiders.Util.Attributes
{
    /// <summary>
    /// Attribute for mapping POCO properties to Table Columns 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }

        /// <summary>
        ///     Construct an instance of <seealso cref="ColumnAttribute"/>
        /// </summary>
        /// <param name="name">Column name the property is to be mapped to</param>
        public ColumnAttribute(string name)
        {
            this.Name = name;
        }
    }
}
