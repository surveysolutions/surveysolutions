﻿using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Models.Api
{
    public class DataTableRequest
    {
        public bool EmptyOnError { get; set; } = false;

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
                get { return this.value?.Trim(); }
                set { this.value = value; }
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

        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<SortOrder> Order { get; set; }

        public List<ColumnInfo> _C { get; set; }
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
