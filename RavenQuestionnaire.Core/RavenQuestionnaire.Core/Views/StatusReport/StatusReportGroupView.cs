using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportGroupView 
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StatusReportItemView> Items { get; set; }

        public StatusReportGroupView()
        {
            Items = new List<StatusReportItemView>();
        }

        public StatusReportGroupView(string id, string title)
            : this()
        {
            this.Id = id;
            this.Title = title;
        }
    }
}
