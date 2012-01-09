using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    public class CompleteQuestionnaireExportView
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

        public IEnumerable<CompleteQuestionnaireExportItem> Items
        {
            get;
            private set;
        }

        public CompleteQuestionnaireExportView(int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireExportItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }
    }
}
