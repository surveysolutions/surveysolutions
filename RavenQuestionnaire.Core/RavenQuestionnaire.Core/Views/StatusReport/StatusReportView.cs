using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportView
    {
        public List<StatusReportGroupView> Items { get; set; }

        public StatusReportView()
        {
            Items = new List<StatusReportGroupView>();
        }
    }
}
