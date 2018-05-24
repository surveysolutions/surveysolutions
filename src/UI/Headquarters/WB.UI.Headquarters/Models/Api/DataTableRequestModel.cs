using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Models.Api
{
    /// <summary>
    /// Datatable specific
    /// </summary>
    public class DataTableRequest
    {
        public class SortOrder
        {
            public int Column { get; set; }
            public OrderDirection Dir { get; set; }
        }

        public class SearchInfo
        {
            private string value;

            public string Value
            {
                get => this.value?.Trim();
                set => this.value = value;
            }

            public bool Regex { get; set; }
        }

        public class ColumnInfo
        {
            public int Title { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public SearchInfo Search { get; set; }
        }

        /// <summary>
        /// Datatables specific
        /// </summary>
        public int Draw { get; set; }

        /// <summary>
        /// Paging - Start row
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// How many rows per page 
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// order of output
        /// </summary>
        public List<SortOrder> Order { get; set; }
        
        /// <summary>
        /// This is shorthand for <c>Columns</c> property. DataTable works via Get requests
        /// </summary>
        public List<ColumnInfo> _C { get; set; }

        /// <summary>
        /// List of columns that need to be outputed. Leave null for default values
        /// </summary>
        public List<ColumnInfo> Columns { get; set; }

        public List<ColumnInfo> ColummnsList => Columns ?? _C;

        public SearchInfo Search { get; set; }


        public int PageIndex => 1 + this.Start / this.Length;
        public int PageSize => this.Length;

        public string GetSortOrder()
        {
            var order = this.Order?.FirstOrDefault();
            if (order == null)
                return string.Empty;

            var columnName = this.ColummnsList[order.Column].Name;
            var stringifiedOrder = order.Dir == OrderDirection.Asc ? string.Empty : OrderDirection.Desc.ToString();

            return $"{columnName} {stringifiedOrder}";
        }

        public IEnumerable<OrderRequestItem> GetSortOrderRequestItems()
        {
            var order = this.Order?.FirstOrDefault();
            if (order == null)
                return Enumerable.Empty<OrderRequestItem>();

            var columnName = this.ColummnsList[order.Column].Name;

            return new[] {new OrderRequestItem {Direction = order.Dir, Field = columnName}};
        }

        public IEnumerable<OrderRequestItem> ToOrderRequestItems()
        {
            if (this.Order == null)
                yield break;

            foreach (var order in this.Order)
            {
                var columnName = this.ColummnsList[order.Column].Name;

                yield return new OrderRequestItem {Direction = order.Dir, Field = columnName};
            }
        }
    }
}
