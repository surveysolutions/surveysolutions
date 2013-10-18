using System.Collections.Generic;
using Core.Supervisor.Views.Reposts.Views;
using Core.Supervisor.Views.Survey;

namespace Core.Supervisor.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
    }
}
