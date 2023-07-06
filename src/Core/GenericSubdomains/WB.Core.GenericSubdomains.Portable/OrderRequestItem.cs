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

        public string ValidateAndGetOrderOrNull(Type target)
        {
            var property = target.GetRuntimeProperties().FirstOrDefault(x => x.Name == Field);
            
            if(property == null)
                return null;
         
            var columnName = property.Name;
            var stringifiedOrder = this.Direction == OrderDirection.Asc ? string.Empty : OrderDirection.Desc.ToString();
            
            return $"{columnName} {stringifiedOrder}";
        }
    }
}
