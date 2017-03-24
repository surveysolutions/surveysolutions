using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DocumentFilter
    {
        public IEnumerable<UsersViewItem> Users { get; set; } 
        public IEnumerable<UsersViewItem> Responsibles { get; set; }
        public IEnumerable<TemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
    }
}