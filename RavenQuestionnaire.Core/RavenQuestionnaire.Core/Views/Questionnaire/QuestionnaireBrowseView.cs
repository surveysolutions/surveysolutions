using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireBrowseView
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

        public IEnumerable<QuestionnaireBrowseItem> Items
        {
            get;
            private set;
        }

        public QuestionnaireBrowseView(int page, int pageSize, int totalCount, IEnumerable<QuestionnaireBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }
    }
}
