using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Supervisor.Views.User;
using Main.Core.View.Questionnaire;

namespace Web.Supervisor.Models
{
    public class HQDashboardModel
    {
        public QuestionnaireBrowseView Questionnaires { get; set; }

        public UserListView Teams { get; set; }
    }
}