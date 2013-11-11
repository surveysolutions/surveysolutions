using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListView
    {
        public QuestionnaireListView(
            int page, int pageSize, int totalCount, IEnumerable<QuestionnaireListViewItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order ?? string.Empty;
        }

        public IEnumerable<QuestionnaireListViewItem> Items { get; private set; }
        
        public string Order { get; set; }
      
        public int Page { get; private set; }
    
        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }
    }
}