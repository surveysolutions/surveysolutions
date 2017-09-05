using System;
using System.Linq;
using System.Reflection;

namespace WB.Core.GenericSubdomains.Portable
{
    public class OrderRequestItem
    {
        public OrderDirection Direction { get; set; }

        public string Field { get; set; }

        public override string ToString()
        {
            return string.Format("{{\"Direction\": \"{0}\", \"Field\": \"{1}\" }}", this.Direction, this.Field);
        }

        public string ToSqlOrderBy()
        {
            return $"{Field} {Direction}";
        }

        public virtual bool IsSortedByOneOfTheProperties(Type target)
        {
            return target.GetRuntimeProperties().Any(x => x.Name == Field);
        }
    }
}