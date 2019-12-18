﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1/assignments")]
    public class AssignmentsApiV1Controller : AssignmentsControllerBase
    {
        private readonly IAssignmentsService assignmentsService;

        public AssignmentsApiV1Controller(IAuthorizedUser authorizedUser, IAssignmentsService assignmentsService,
            ICommandService commandService) 
            : base(authorizedUser, assignmentsService, commandService)
        {
            this.assignmentsService = assignmentsService;
        }

        protected override IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId)
            => assignmentsService.GetAssignmentsForSupervisor(responsibleId);

        [WriteToSyncLog(SynchronizationLogType.GetAssignment)]
        [HttpGet]
        [Route("{id:int}")]
        public override ActionResult<AssignmentApiDocument> GetAssignment(int id)
            => base.GetAssignment(id);

        [WriteToSyncLog(SynchronizationLogType.GetAssignmentsList)]
        [HttpGet]
        [Route("")]
        public override Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
            => base.GetAssignmentsAsync(cancellationToken);

        [HttpPost]
        [Route("{id:int}/Received")]
        public override IActionResult Received(int id) => base.Received(id);
    }
}
