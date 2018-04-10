using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    public class AssignmentsUpgradeApiContoller : BaseApiController
    {
        private readonly IAssignmentsUpgradeService upgradeService;

        public AssignmentsUpgradeApiContoller(ICommandService commandService, 
            ILogger logger, 
            IAssignmentsUpgradeService upgradeService) : base(commandService, logger)
        {
            this.upgradeService = upgradeService;
        }

        
        [HttpGet]
        [CamelCase]
        public HttpResponseMessage Status(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));
            if (assignmentUpgradeProgressDetails != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, assignmentUpgradeProgressDetails);
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No process with provided it was queued");
        }
    }
}
