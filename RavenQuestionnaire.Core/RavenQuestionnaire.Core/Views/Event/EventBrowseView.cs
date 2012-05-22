using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventBrowseView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set; }

        public int TotalCount { get; private set; }

        public IEnumerable<EventBrowseItem> Items { get; private set; }

        public EventBrowseView(int page, int pageSize, int totalCount, IEnumerable<EventBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
