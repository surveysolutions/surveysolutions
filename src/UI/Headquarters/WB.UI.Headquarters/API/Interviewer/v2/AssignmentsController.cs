using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.Interviewer.v2
{
    public class AssignmentsController : ApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly IMapper autoMapper;

        public AssignmentsController(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService,
            IMapper autoMapper)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
            this.autoMapper = autoMapper;
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignments)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public List<AssignmentApiView> List()
        {
            var authorizedUserId = this.authorizedUser.Id;
            var assignments = this.assignmentsService.GetAssignments(authorizedUserId);

            return this.autoMapper.Map<List<Assignment>, List<AssignmentApiView>>(assignments);
        }
    }
}