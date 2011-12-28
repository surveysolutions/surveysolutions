using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseView
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

        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;

        public int TotalCount { get; private set; }

        public IEnumerable<CompleteQuestionnaireBrowseItem> Items
        {
            get;
            private set;
        }

        public CompleteQuestionnaireBrowseView(int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }
    }
}
