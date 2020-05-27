using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class AccountListView
    {
        public AccountListView(int page, int pageSize, int totalCount, IEnumerable<DesignerIdentityUser> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        public IEnumerable<DesignerIdentityUser> Items { get; private set; }
        public string Order { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
    }
}
