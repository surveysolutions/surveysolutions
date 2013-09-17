using System.Collections.Generic;
using Core.Supervisor.Views.Reposts.Views;
using Core.Supervisor.Views.Survey;
using Main.Core.Documents;

namespace Web.Supervisor.Models
{
    public class DocumentFilter
    {
        public IEnumerable<UsersViewItem> Responsibles { get; set; }
        public IEnumerable<TemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
        public IEnumerable<UsersViewItem> Users { get; set; }
    }
}