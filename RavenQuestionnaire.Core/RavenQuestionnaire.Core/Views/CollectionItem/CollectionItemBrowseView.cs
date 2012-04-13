using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseView
    {
        public int TotalCount { get; private set; }

        public int PageSize
        {
            get;
            private set;
        }

        public int Page
        {
            get;
            private set;
        }


        public CollectionItemBrowseView(int page, int pageSize, int totalCount)
        {
            this.TotalCount = totalCount;
            this.Page = page;
            this.PageSize = pageSize;
        }
    }
}
