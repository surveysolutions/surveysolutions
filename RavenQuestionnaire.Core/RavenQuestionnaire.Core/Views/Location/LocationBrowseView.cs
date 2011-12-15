using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Location
{
    public class LocationBrowseView
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

        public IEnumerable<LocationBrowseItem> Items
        {
            get;
            private set;
        }

        public LocationBrowseView(int page, int pageSize, int totalCount, IEnumerable<LocationBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
