using System.Collections.Generic;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;

namespace Web.Supervisor.Models
{
    public class DocumentFilter
    {
        public IEnumerable<SurveyUsersViewItem> Responsibles { get; set; }
        public IEnumerable<SummaryTemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
    }
}