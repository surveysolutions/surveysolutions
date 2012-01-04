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

        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;


        public int TotalCount { get; private set; }

        public IEnumerable<QuestionnaireBrowseItem> Items
        {
            get;
            private set;
        }

        public QuestionnaireBrowseView(int page, int pageSize, int totalCount, IEnumerable<QuestionnaireBrowseItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }
    }
}
