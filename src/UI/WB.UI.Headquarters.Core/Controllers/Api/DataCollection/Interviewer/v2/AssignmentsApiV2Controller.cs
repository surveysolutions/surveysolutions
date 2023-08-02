using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2/assignments")]
    public class AssignmentsApiV2Controller : AssignmentsControllerBase
    {
        private readonly IAssignmentsService assignmentsService;

        public AssignmentsApiV2Controller(IAuthorizedUser authorizedUser, IAssignmentsService assignmentsService,
            IUserToDeviceService userToDeviceService,
            ICommandService commandService) 
            : base(authorizedUser, assignmentsService, userToDeviceService,  commandService)
        {
            this.assignmentsService = assignmentsService;
        }

        protected override IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId)
            => this.assignmentsService.GetAssignments(responsibleId);


        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetAssignmentsList)]
        public override Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
            => base.GetAssignmentsAsync(cancellationToken);

        [HttpGet]
        [Route("{id:int}")]
        [WriteToSyncLog(SynchronizationLogType.GetAssignment)]
        public override ActionResult<AssignmentApiDocument> GetAssignment(int id)
            => base.GetAssignment(id);

        [HttpPost]
        [Route("{id:int}/Received")]
        [WriteToSyncLog(SynchronizationLogType.AssignmentReceived)]
        public override IActionResult Received(int id) => base.Received(id);
    }
}
