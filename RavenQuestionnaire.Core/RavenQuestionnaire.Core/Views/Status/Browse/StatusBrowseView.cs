using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Status.Browse
{
    public class StatusBrowseView
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

        public string QuestionnaireId { set; get; }

        public IEnumerable<StatusBrowseItem> Items
        {
            get;
            private set;
        }

        public StatusBrowseView(int page, int pageSize, int totalCount, IEnumerable<StatusBrowseItem> items, string questionnaireId)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.QuestionnaireId = questionnaireId;
        }

    }
}
