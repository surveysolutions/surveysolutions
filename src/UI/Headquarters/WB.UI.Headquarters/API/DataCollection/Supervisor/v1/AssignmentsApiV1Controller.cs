using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(UserRoles.Supervisor)]
    public class AssignmentsApiV1Controller : AssignmentsControllerBase
    {
        private readonly IAssignmentsService assignmentsService;

        public AssignmentsApiV1Controller(IAuthorizedUser authorizedUser, IAssignmentsService assignmentsService) : base(authorizedUser, assignmentsService)
        {
            this.assignmentsService = assignmentsService;
        }

        protected override IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId)
            => assignmentsService.GetAssignmentsForSupervisor(responsibleId);

        [WriteToSyncLog(SynchronizationLogType.GetAssignment)]
        [HttpGet]
        public override Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken)
            => base.GetAssignmentAsync(id, cancellationToken);

        [WriteToSyncLog(SynchronizationLogType.GetAssignmentsList)]
        [HttpGet]
        public override Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
            => base.GetAssignmentsAsync(cancellationToken);

    }
}
