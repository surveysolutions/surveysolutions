using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersView
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

        public IEnumerable<InterviewersItem> Items
        {
            get;
            private set;
        }

        public InterviewersView(int page, int pageSize, int totalCount, IEnumerable<InterviewersItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
