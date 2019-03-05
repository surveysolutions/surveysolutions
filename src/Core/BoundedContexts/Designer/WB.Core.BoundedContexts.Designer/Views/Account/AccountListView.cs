﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class AccountListView
    {
        public AccountListView(int page, int pageSize, int totalCount, IEnumerable<IdentityUser> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        public IEnumerable<IdentityUser> Items { get; private set; }
        public string Order { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
    }
}
