using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.BoundedContexts.Headquarters.Team.ViewFactories;
using WB.UI.Headquarters.Extensions;
using WB.UI.Headquarters.Models.Interview;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class InterviewsController : BaseController
    {
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IUserListViewFactory userListViewFactory;


        public InterviewsController(IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IUserListViewFactory userListViewFactory)
        {
            if (takeNewInterviewViewFactory == null) throw new ArgumentNullException("takeNewInterviewViewFactory");
            if (allUsersAndQuestionnairesFactory == null) throw new ArgumentNullException("allUsersAndQuestionnairesFactory");
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.userListViewFactory = userListViewFactory;
        }

        public ActionResult Index(Guid? questionnaireId)
        {
            return this.View(this.Filters());
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems();

            AllUsersAndQuestionnairesView usersAndQuestionnaires = this.allUsersAndQuestionnairesFactory.Load();

            var result = new DocumentFilter
            {
                Users = this.userListViewFactory.GetActiveUsers(int.MaxValue)
                    .Where(u => !u.IsLocked)
                    .Select(u => new UsersViewItem
                    {
                        UserId = u.PublicKey,
                        UserName = u.UserName
                    }),
                Responsibles = usersAndQuestionnaires.Users,
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            };
            return result;
        }


        public ActionResult TakeNew(Guid id)
        {
            Guid questionnaireId = id;
            Guid userId = User.UserId();
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(questionnaireId, userId));
            
            return this.View(model);
        }
    }
}