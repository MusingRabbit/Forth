using System;

namespace RockRaiders.Util.Attributes
{
    /// <summary>
    /// Attribute for marking a POCO property as primary key
    /// </summary>
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Column Name
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// Sequence Name
        /// </summary>
        public string SequenceName { get; set; }
        /// <summary>
        /// Flag stating wheather the primary key is auto incremenet
        /// </summary>
        public bool AutoIncremenet { get; set; }


        public PrimaryKeyAttribute()
        {
            AutoIncremenet = true;
        }

        /// <summary>
        ///     Constructs an instance of <seealso cref="PrimaryKeyAttribute"/>
        /// </summary>
        /// <param name="primaryKey">Column name of the Primary Key</param>
        public PrimaryKeyAttribute(string primaryKey)
        {
            Value = primaryKey;
            AutoIncremenet = true;
        }
    }
}
