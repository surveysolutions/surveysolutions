using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseView
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

        public IEnumerable<CQGroupItem> Groups
        {
            get;
            private set;
        }

        public CQGroupedBrowseView(int page, int pageSize, int totalCount, IEnumerable<CQGroupItem> groups)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Groups = groups;
        }
    }
}
