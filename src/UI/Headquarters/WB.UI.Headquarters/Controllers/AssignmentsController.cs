using System.Collections.Generic;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [LimitsFilter]
    [ActivePage(MenuItem.Assignments)]
    public class AssignmentsController : BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory usersAndQuestionnairesFactory;

        public AssignmentsController(ICommandService commandService,
            ILogger logger,
            IAllUsersAndQuestionnairesFactory usersAndQuestionnairesFactory)
            : base(commandService, logger)
        {
            this.usersAndQuestionnairesFactory = usersAndQuestionnairesFactory;
        }

        public ActionResult Index()
        {
            var templateViewItems = this.usersAndQuestionnairesFactory.GetQuestionnaires();

            var model = new AssignmentsFilters(templateViewItems);

            return View(model);
        }
    }

    public class AssignmentsFilters
    {
        public AssignmentsFilters(IEnumerable<TemplateViewItem> templates)
        {
            this.Templates = templates;
        }

        public IEnumerable<TemplateViewItem> Templates { get; }
    }
}