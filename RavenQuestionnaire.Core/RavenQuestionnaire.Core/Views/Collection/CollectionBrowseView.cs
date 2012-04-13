using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionBrowseView
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

        public IEnumerable<CollectionBrowseItem> Items
        {
            get;
            private set;
        }

        public CollectionBrowseView(int page, int pageSize, int totalCount, IEnumerable<CollectionBrowseItem> items)
        {
            this.TotalCount = totalCount;
            this.Items = items;
            this.Page = page;
            this.PageSize = pageSize;
        }
    }
}
