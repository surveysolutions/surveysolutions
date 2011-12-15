using System.Collections.Generic;

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

        public int TotalCount { get; private set; }

        public IEnumerable<CompleteQuestionnaireBrowseItem> Items
        {
            get;
            private set;
        }

        public CompleteQuestionnaireBrowseView(int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
