using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;

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