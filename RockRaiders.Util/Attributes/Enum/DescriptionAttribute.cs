using System;

namespace RockRaiders.Util.Attributes.Enum
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Description;
        }
    }
}
