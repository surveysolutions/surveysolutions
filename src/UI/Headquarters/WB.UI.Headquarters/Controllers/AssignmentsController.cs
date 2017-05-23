using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [LimitsFilter]
    [ActivePage(MenuItem.Assignments)]
    public class AssignmentsController : BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory usersAndQuestionnairesFactory;
        private readonly IAssignmentViewFactory assignmentViewFactory;

        public AssignmentsController(ICommandService commandService, 
            ILogger logger,
            IAllUsersAndQuestionnairesFactory usersAndQuestionnairesFactory,
            IAssignmentViewFactory assignmentViewFactory)
            : base(commandService, logger)
        {
            this.usersAndQuestionnairesFactory = usersAndQuestionnairesFactory;
            this.assignmentViewFactory = assignmentViewFactory;
        }

        public ActionResult Index()
        {
            var templateViewItems = this.usersAndQuestionnairesFactory.GetQuestionnaires();

            var model = new AssignmentsFilters(templateViewItems);

            return View(model);
        }

        public ActionResult List(AssignmentsDataTableRequest request)
        {
            var input = new AssignmentsInputModel();

            var result = this.assignmentViewFactory.Load(input);

            var response = new AssignmetsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items
            };
            return JsonCamelCase(response);
        }

    public class AssignmetsDataTableResponse : DataTableResponse<AssignmentWithoutIdentifingData>
    {
    }

    public class AssignmentsDataTableRequest : DataTableRequest
    {
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