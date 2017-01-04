using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Models.Api
{
    public class DataTableRequest
    {
        public class SortOrder
        {
            public int Column { get; set; }
            public OrderDirection Dir { get; set; }
        }

        public class SearchInfo
        {
            public string Value { get; set; }
            public bool Regex { get; set; }
        }

        public class ColumnInfo
        {
            public int Title { get; set; }
            public string Data { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public SearchInfo Search { get; set; }
        }
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<SortOrder> Order { get; set; }
        public List<ColumnInfo> Columns { get; set; }
        public SearchInfo Search { get; set; }
        public int PageIndex => 1 + this.Start / this.Length;
        public int PageSize => this.Length;

        public string GetSortOrder()
        {
            var order = this.Order.FirstOrDefault();
            if (order == null)
                return string.Empty;

            var columnName = this.Columns[order.Column].Name;
            var stringifiedOrder = order.Dir == OrderDirection.Asc ? string.Empty : OrderDirection.Desc.ToString();

            return $"{columnName} {stringifiedOrder}";
        }
    }
}