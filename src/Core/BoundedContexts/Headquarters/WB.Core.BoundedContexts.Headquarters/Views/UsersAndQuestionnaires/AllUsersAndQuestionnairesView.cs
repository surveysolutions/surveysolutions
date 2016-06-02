using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public class AllUsersAndQuestionnairesView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
    }
}
