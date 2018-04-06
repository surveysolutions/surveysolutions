using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API
{
    [CamelCase]
    [RoutePrefix("api/AssignmentsUpgrade")]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class AssignmentsUpgradeApiContoller : ApiController
    {
        private readonly IAssignmentsUpgradeService upgradeService;

        public AssignmentsUpgradeApiContoller(IAssignmentsUpgradeService upgradeService)
        {
            this.upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
        }

        [HttpGet]
        public HttpResponseMessage Status(Guid id)
        {
            var assignmentUpgradeProgressDetails = this.upgradeService.Status(id);
            if (assignmentUpgradeProgressDetails != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, assignmentUpgradeProgressDetails);
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No process with provided it was queued");
        }
    }
}
