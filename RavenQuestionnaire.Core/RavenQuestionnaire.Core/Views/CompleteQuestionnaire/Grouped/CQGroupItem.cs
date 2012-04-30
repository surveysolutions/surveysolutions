using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupItem
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

        public IEnumerable<CompleteQuestionnaireBrowseItem> Items
        {
            get;
            private set;
        }

        public string Title { get; set; }
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        private string _id;

        public CQGroupItem(int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items, string title, string id)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Title = title;
            this.Id = id;
        }
    }
}
