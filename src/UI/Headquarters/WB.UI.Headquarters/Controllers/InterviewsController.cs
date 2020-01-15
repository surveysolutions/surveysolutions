using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Resources;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewsController : BaseController
    {
        private readonly IAuthorizedUser currentUser;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersRepository;
        private readonly IPlainStorageAccessor<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;

        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;

        public InterviewsController(
            ICommandService commandService,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            IUserViewFactory usersRepository,
            IPlainStorageAccessor<InterviewSummary> interviewSummaryReader,
            IPlainStorageAccessor<Assignment> assignments,
            IInterviewUniqueKeyGenerator keyGenerator,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IAuthorizedUser currentUser) : base(commandService, logger)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.usersRepository = usersRepository;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
            this.keyGenerator = keyGenerator;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.currentUser = currentUser;
        }
        
        public ActionResult Index()
        {
            return View("Index", GetViewModel());
        }

        private InterviewsFilter GetViewModel()
        {
            var title = Common.Interviews;
            
            var statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor).ToList();

            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            ViewBag.Title = title;
            var model = new InterviewsFilter
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Title = title,
                AllInterviews = this.currentUser.IsSupervisor ? Url.Content(@"~/api/InterviewApi/GetTeamInterviews") : Url.Content(@"~/api/InterviewApi/Interviews"),
                AssignmentsUrl = @Url.Action("Index","Assignments"),
                InterviewReviewUrl = Url.Action("Review", "Interview"),
                ProfileUrl = Url.Action("Profile", "Interviewer"),
                CommandsUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "CommandApi", action = "ExecuteCommands" }),
                Statuses = statuses
                    .Select(item => new ComboboxViewItem { Key = ((int)item.Status).ToString(), Value = item.Status.ToLocalizeString(), Alias = item.Status.ToString() })
                    .ToArray(),
                Questionnaires = questionnaires
            };

            model.Api = new InterviewsFilter.ApiEndpoints
            {
                Responsible = model.IsSupervisor
                    ? Url.RouteUrl("DefaultApiWithAction",
                        new {httproute = "", controller = "Teams", action = "InterviewersCombobox"})
                    : Url.RouteUrl("DefaultApiWithAction",
                        new {httproute = "", controller = "Teams", action = "ResponsiblesCombobox"}),
                InterviewStatuses = Url.RouteUrl("DefaultApiWithAction", 
                    new { httproute = "", controller = "InterviewApi", action = "ChangeStateHistory" }),
                QuestionnaireByIdUrl = Url.RouteUrl("DefaultApiWithAction",
                    new { httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesComboboxById" })

            };

            return model;
        }
    }

    public class InterviewsFilter
    {
        public string AllInterviews { get; set; }
        
        public ComboboxViewItem[] Statuses { get; set; }
        public string Title { get; set; }
        public List<QuestionnaireVersionsComboboxViewItem> Questionnaires { get; set; }

        public ApiEndpoints Api { get; set; }
        public bool IsSupervisor { get; set; }
        public string AssignmentsUrl { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public bool IsHeadquarter { get; set; }
        public string InterviewReviewUrl { get; set; }
        public string ProfileUrl { get; set; }
        public string CommandsUrl { get; set; }

        public class ApiEndpoints
        {
            public string Responsible { get; set; }
            public string InterviewStatuses { get; set; }
            public string QuestionnaireByIdUrl { get; internal set; }
        }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, ModelSerializationSettings);
        }

        private static readonly JsonSerializerSettings ModelSerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            }
        };
    }
}
