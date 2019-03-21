using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [LimitsFilter]
    [ActivePage(MenuItem.Assignments)]
    public class AssignmentsController : BaseController
    {
        private readonly IStatefulInterviewRepository interviews;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAuthorizedUser currentUser;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IInvitationService invitationService;

        public AssignmentsController(ICommandService commandService,
            ILogger logger,
            IStatefulInterviewRepository interviews,
            IQuestionnaireStorage questionnaireStorage,
            IAuthorizedUser currentUser, 
            IPlainStorageAccessor<Assignment> assignmentsStorage, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IInvitationService invitationService)
            : base(commandService, logger)
        {
            this.interviews = interviews;
            this.questionnaireStorage = questionnaireStorage;
            this.currentUser = currentUser;
            this.assignmentsStorage = assignmentsStorage;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.questionnaires = questionnaires;
            this.invitationService = invitationService;
        }
        
        [Localizable(false)]
        [ActivePage(MenuItem.Assignments)]
        public ActionResult Index()
        {
            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            var model = new AssignmentsFilters 
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Questionnaires = questionnaires,
                MaxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment
            };

            model.Api = new AssignmentsFilters.ApiEndpoints
            {
                Responsible = model.IsSupervisor
                    ? Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "InterviewersCombobox"})
                    : Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "ResponsiblesCombobox"}),
                Assignments = Url.Content(@"~/api/Assignments"),
                Interviews = model.IsSupervisor ? Url.Action("Interviews", "Survey") : Url.Action("Interviews", "HQ"),
                Profile = Url.Action("Profile", "Interviewer"),
                SurveySetup = model.IsSupervisor ? "" : Url.Action("Index", "SurveySetup")
            };

            return View(model);
        }

        [HttpPost]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Create(string id, Guid responsibleId, int? size, string email, string password, bool? webMode)
        {
            var interview = this.interviews.Get(id);
            if (interview == null)
            {
                return HttpNotFound();
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);
            bool isAudioRecordingEnabled = this.questionnaires.Query(_ => _
                .Where(q => q.Id == interview.QuestionnaireIdentity.ToString())
                .Select(q => q.IsAudioRecordingEnabled).FirstOrDefault());

            var assignment = Assignment.PrefillFromInterview(interview, questionnaire);
            assignment.UpdateQuantity(size);
            assignment.Reassign(responsibleId);
            assignment.SetAudioRecordingEnabled(isAudioRecordingEnabled);

            assignment.UpdateMode(webMode);

            if (webMode == true)
            {
                assignment.UpdateEmail(email);

                var updatedPassword = password == AssignmentConstants.PasswordSpecialValue
                    ? TokenGenerator.GetRandomAlphanumericString(6)
                    : password;

                assignment.UpdatePassword(updatedPassword);
            }

            this.assignmentsStorage.Store(assignment, null);

            this.invitationService.CreateInvitationForWebInterview(assignment);

            return RedirectToAction("Index");
        }
    }

    public class AssignmentsFilters
    {
        public bool IsHeadquarter { get; set; }
        public bool IsSupervisor { get; set; }
        public ApiEndpoints Api { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public List<QuestionnaireVersionsComboboxViewItem> Questionnaires { get; set; }
        public int MaxInterviewsByAssignment { get; set; }

        public class ApiEndpoints
        {
            public string Assignments { get; set; }

            public string Profile { get; set; }
            public string Interviews { get; set; }
            public string Responsible { get; set; }

            public string SurveySetup { get; set; }
        }
    }
}
