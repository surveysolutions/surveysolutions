using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
    }
}
