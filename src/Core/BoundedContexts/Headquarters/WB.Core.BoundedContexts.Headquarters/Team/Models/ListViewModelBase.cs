using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class ListViewModelBase
    {
        private int page = 1;

        private int pageSize = 20;

        private IEnumerable<OrderRequestItem> orders = new List<OrderRequestItem>();

        public string Order
        {
            get { return StringUtil.GetOrderRequestString(this.orders); }

            set { this.orders = StringUtil.ParseOrderRequestString(value); }
        }

        public IEnumerable<OrderRequestItem> Orders
        {
            get { return this.orders; }

            set { this.orders = value ?? new List<OrderRequestItem>(); }
        }

        public int Page
        {
            get { return this.page; }

            set { this.page = value; }
        }

        public int PageSize
        {
            get { return this.pageSize; }

            set { this.pageSize = value; }
        }
    }
}