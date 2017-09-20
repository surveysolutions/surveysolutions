using System.ComponentModel;
using System.Resources;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [LimitsFilter]
    [ActivePage(MenuItem.Assignments)]
    public class AssignmentsController : BaseController
    {
        private readonly IAuthorizedUser currentUser;
        private readonly IAuthorizedUser authorizedUser;

        public AssignmentsController(ICommandService commandService,
            ILogger logger,
            IAuthorizedUser currentUser,
            IAuthorizedUser authorizedUser)
            : base(commandService, logger)
        {
            this.currentUser = currentUser;
            this.authorizedUser = authorizedUser;
        }
        
        [Localizable(false)]
        public ActionResult Index()
        {
            var model = new AssignmentsFilters
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                resources = Resources.Translations()
            };

            model.Api = new AssignmentsFilters.ApiEndpoints
            {
                Questionnaire = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesCombobox"}),
                QuestionnaireById = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesComboboxById"}),
                Responsible = model.IsSupervisor
                    ? Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "InterviewersCombobox"})
                    : Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "ResponsiblesCombobox"}),
                Assignments = Url.Content(@"~/api/Assignments"),
                Interviews = model.IsSupervisor ? Url.Action("Interviews", "Survey") : Url.Action("Interviews", "HQ"),
                Profile = Url.Action("Profile", "Interviewer")
            };

            return View(model);
        }

        private static readonly ResourceManager[] Resources =
        {
            MainMenu.ResourceManager,
            Assignments.ResourceManager,
            Pages.ResourceManager,
            global::Resources.Common.ResourceManager
        };

    }

    public class AssignmentsFilters
    {
        public TranslationModel resources { get; set; }
        public bool IsHeadquarter { get; set; }
        public bool IsSupervisor { get; set; }
        public ApiEndpoints Api { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }

        public class ApiEndpoints
        {
            public string Assignments { get; set; }

            public string Profile { get; set; }
            public string Interviews { get; set; }
            public string Questionnaire { get; set; }
            public string QuestionnaireById { get; set; }
            public string Responsible { get; set; }
        }
    }


}