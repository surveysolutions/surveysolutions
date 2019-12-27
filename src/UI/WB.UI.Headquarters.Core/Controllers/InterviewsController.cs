using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewsController : Controller
    {
        private readonly IAuthorizedUser currentUser;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;

        public InterviewsController(IAuthorizedUser currentUser, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory)
        {
            this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory ?? throw new ArgumentNullException(nameof(allUsersAndQuestionnairesFactory));
        }

        [ActivePage(MenuItem.Interviews)]
        public IActionResult Index()
        {
            return View("Index", GetViewModel());
        }

        private InterviewsFilterModel GetViewModel()
        {
            var title = Common.Interviews;

            var statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.currentUser.IsSupervisor).ToList();

            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            ViewBag.Title = title;
            var model = new InterviewsFilterModel
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Title = title,
                AllInterviews = this.currentUser.IsSupervisor ? Url.Content(@"~/api/InterviewApi/GetTeamInterviews") : Url.Content(@"~/api/InterviewApi/Interviews"),
                AssignmentsUrl = @Url.Action("Index", "Assignments"),
                InterviewReviewUrl = Url.Action("Review", "Interview"),
                ProfileUrl = Url.Action("Profile", "Interviewer"),
                CommandsUrl = Url.Action("ExecuteCommands", "CommandApi"),
                Statuses = statuses
                    .Select(item => new ComboboxViewItem { Key = ((int)item.Status).ToString(), Value = item.Status.ToLocalizeString(), Alias = item.Status.ToString() })
                    .ToArray(),
                Questionnaires = questionnaires
            };

            model.Api = new InterviewsFilterModel.ApiEndpoints
            {
                Responsible = model.IsSupervisor
                    ? Url.Action("InterviewersCombobox", "Teams")
                    : Url.Action("ResponsiblesCombobox", "Teams"),
                InterviewStatuses = Url.Action("ChangeStateHistory", "InterviewApi"),
                QuestionnaireByIdUrl = Url.Action("QuestionnairesComboboxById", "QuestionnairesApi")
            };

            return model;
        }
    }
}
