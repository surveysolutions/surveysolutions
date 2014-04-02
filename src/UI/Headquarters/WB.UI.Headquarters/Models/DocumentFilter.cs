using System.Collections.Generic;
using Core.Supervisor.Views.Reposts.Views;
using Core.Supervisor.Views.Survey;

namespace WB.UI.Headquarters.Models
{
    public class DocumentFilter
    {
        public IEnumerable<UsersViewItem> Responsibles { get; set; }
        public IEnumerable<TemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
        public IEnumerable<UsersViewItem> Users { get; set; }
    }
}