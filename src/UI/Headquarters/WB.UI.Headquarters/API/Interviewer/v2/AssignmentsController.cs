using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.Interviewer.v2
{
    public class AssignmentsController : ApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Assignment> assignmentsAccessor;
        private readonly IMapper autoMapper;

        public AssignmentsController(IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<Assignment> assignmentsAccessor,
            IMapper autoMapper)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsAccessor = assignmentsAccessor;
            this.autoMapper = autoMapper;
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignments)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public List<AssignmentApiView> List()
        {
            var authorizedUserId = this.authorizedUser.Id;
            //TODO: Do not return "completed" assignments
            var assignments = this.assignmentsAccessor.Query(x => x.Where(assigment => assigment.ResponsibleId == authorizedUserId && !assigment.Archived).ToList());

            return this.autoMapper.Map<List<Assignment>, List<AssignmentApiView>>(assignments);
        }
    }
}