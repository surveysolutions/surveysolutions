using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    public class AssignmentsUpgradeApiController : BaseApiController
    {
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IQuestionnaireBrowseViewFactory browseViewFactory;

        public AssignmentsUpgradeApiController(ICommandService commandService, 
            ILogger logger, 
            IAssignmentsUpgradeService upgradeService,
            IQuestionnaireBrowseViewFactory browseViewFactory) : base(commandService, logger)
        {
            this.upgradeService = upgradeService;
            this.browseViewFactory = browseViewFactory;
        }

        
        [CamelCase]
        [HttpGet]
        public HttpResponseMessage Status(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));
            if (assignmentUpgradeProgressDetails != null)
            {
                dynamic response = new ExpandoObject();
                response.progressDetails = assignmentUpgradeProgressDetails;

                var questionnaireTo = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateTo);
                response.migrateToTitle = string.Format(Pages.QuestionnaireNameFormat, questionnaireTo.Title,
                    questionnaireTo.Version);
                var questionnaireFrom = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateFrom);
                response.migrateFromTitle = string.Format(Pages.QuestionnaireNameFormat, questionnaireFrom.Title,
                    questionnaireFrom.Version);

                return Request.CreateResponse(HttpStatusCode.OK, (object)response);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
