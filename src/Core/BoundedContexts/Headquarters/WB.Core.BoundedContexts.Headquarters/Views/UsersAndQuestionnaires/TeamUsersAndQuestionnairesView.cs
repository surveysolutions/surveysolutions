using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
    }
}
