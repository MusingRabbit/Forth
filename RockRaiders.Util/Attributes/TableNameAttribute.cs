using System;

namespace RockRaiders.Util.Attributes
{
    /// <summary>
    /// Name of the table a POCO represents
    /// </summary>
    public class TableNameAttribute : Attribute
    {
        public string Name { get; set; }

        public TableNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
