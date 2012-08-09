using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseView
    {
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

        public int TotalCount { get; private set; }

        public IEnumerable<FileBrowseItem> Items
        {
            get;
            private set;
        }

        public FileBrowseView(int page, int pageSize, int totalCount, IEnumerable<FileBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
