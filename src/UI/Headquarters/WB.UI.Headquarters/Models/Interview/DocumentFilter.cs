using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Reports.Views;

namespace WB.UI.Headquarters.Models.Interview
{
    public class DocumentFilter
    {
        public IEnumerable<UsersViewItem> Responsibles { get; set; }
        public IEnumerable<TemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
        public IEnumerable<UsersViewItem> Users { get; set; }
    }
}