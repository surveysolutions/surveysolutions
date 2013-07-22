using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Supervisor.Views.User;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace Web.Supervisor.Models
{
    public class HQDashboardModel
    {
        public QuestionnaireBrowseView Questionnaires { get; set; }

        public UserListView Teams { get; set; }
    }
}