using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Reports.Views
{
    public class AllUsersAndQuestionnairesView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
    }
}
