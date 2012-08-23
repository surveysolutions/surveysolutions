using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyBrowseView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set; }
        
        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;

        public List<OrderRequestItem> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }

        private List<OrderRequestItem> _orders = new List<OrderRequestItem>();
        
        public int TotalCount { get; private set; }

        public List<SurveyBrowseItem> Items { get; set; }

        public List<string> Headers { get; set; }

        public SurveyBrowseView()
        {
            this.Items = new List<SurveyBrowseItem>();
        }

        public SurveyBrowseView(int page, int pageSize, int totalCount, IEnumerable<SurveyBrowseItem> items):this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            var statuses = SurveyStatus.GetAllStatuses().Select(s => s.Name).ToList();
            statuses.Insert(0, "Total");
            statuses.Insert(1, "Unassigned");
            this.Headers = statuses;
            foreach (var item in items)
                this.Items.Add(new SurveyBrowseItem(item.Id, item.Title, item.Unassigned, item.Statistic, item.Total, item.Initial, item.Error, item.Complete));
        }
    }
}
