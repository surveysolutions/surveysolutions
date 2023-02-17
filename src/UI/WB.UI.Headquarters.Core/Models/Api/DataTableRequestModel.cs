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
            public string Name { get; set; }
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
        public int Length { get; set; } = 10;

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

            string columnName = order.Name;
            if (order.Column < this.ColummnsList?.Count)
            {
                columnName = this.ColummnsList[order.Column].Name;
            }

            var stringifiedOrder = order.Dir == OrderDirection.Asc ? string.Empty : OrderDirection.Desc.ToString();

            return $"{columnName} {stringifiedOrder}";
        }

        public IEnumerable<OrderRequestItem> GetSortOrderRequestItems()
        {
            var hasOrderInfo = this.Order?.Any() ?? false;
            if (!hasOrderInfo)
                return Enumerable.Empty<OrderRequestItem>();

            return this.Order.Select(order => new OrderRequestItem()
            {
                Direction = order.Dir,
                Field = this.ColummnsList != null && this.ColummnsList.Count > order.Column
                    ? this.ColummnsList?[order.Column].Name 
                    : order.Name
            });
        }

        public IEnumerable<OrderRequestItem> ToOrderRequestItems()
        {
            if (this.Order == null)
                yield break;

            foreach (var order in this.Order)
            {
                var columnName = this.ColummnsList !=null && this.ColummnsList.Count > order.Column
                    ? this.ColummnsList?[order.Column].Name 
                    : order.Name;

                yield return new OrderRequestItem {Direction = order.Dir, Field = columnName};
            }
        }
    }
}
