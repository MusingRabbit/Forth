using System;

namespace RockRaiders.Util.Attributes
{
    /// <summary>
    /// Result Column is similar to a column attribute, though its Binding is GET only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResultColumnAttribute : ColumnAttribute
    {
        public ResultColumnAttribute(string name)
            : base(name)
        {
        }
    }
}
