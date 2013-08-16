using System.Collections.Generic;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;

namespace Core.Supervisor.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesView
    {
        public IEnumerable<SurveyUsersViewItem> Users { get; set; }
        public IEnumerable<SummaryTemplateViewItem> Questionnaires { get; set; }
    }
}
