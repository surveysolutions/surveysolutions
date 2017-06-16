using System.Collections.Generic;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.Interviewer.v2
{
    public class AssignmentsApiV2Controller : ApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly IAssignmentViewFactory assignmentViewFactory;

        public AssignmentsApiV2Controller(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService, IAssignmentViewFactory assignmentViewFactory)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
            this.assignmentViewFactory = assignmentViewFactory;
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignments)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public List<AssignmentApiView> List()
        {
            var authorizedUserId = this.authorizedUser.Id;
            var assignments = this.assignmentsService.GetAssignments(authorizedUserId);

            var assignmentApiViews = new List<AssignmentApiView>();

            foreach (var assignment in assignments)
            {
                var assignmentApiView = this.assignmentViewFactory.MapAssignment(assignment);
                assignmentApiViews.Add(assignmentApiView);
            }

            return assignmentApiViews;
        }
    }
}