using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowGraphBrowseView
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

        public IEnumerable<FlowGraphBrowseItem> Items
        {
            get;
            private set;
        }

        public FlowGraphBrowseView(int page, int pageSize, int totalCount, IEnumerable<FlowGraphBrowseItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }
    }
}
