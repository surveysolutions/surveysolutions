using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamUsersAndQuestionnairesView
    {
        public IEnumerable<SurveyUsersViewItem> Users { get; set; }
        public IEnumerable<SummaryTemplateViewItem> Questionnaires { get; set; }
    }
}
