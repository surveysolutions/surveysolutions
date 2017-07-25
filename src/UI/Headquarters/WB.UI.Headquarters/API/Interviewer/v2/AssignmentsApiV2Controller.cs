using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.API.Interviewer.v2
{
    public class AssignmentsApiV2Controller : ApiController, IAssignmentSynchronizationApi
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;

        public AssignmentsApiV2Controller(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignment)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken)
        {
            var authorizedUserId = this.authorizedUser.Id;

            var assignment = this.assignmentsService.GetAssignment(id);

            if (assignment.ResponsibleId != authorizedUserId)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            return Task.FromResult(this.assignmentsService.MapAssignment(assignment));
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignmentsList)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
        {
            var authorizedUserId = this.authorizedUser.Id;

            var assignments = this.assignmentsService.GetAssignments(authorizedUserId);

            var assignmentApiViews = new List<AssignmentApiView>();

            foreach (var assignment in assignments)
            {
                assignmentApiViews.Add(new AssignmentApiView
                {
                    Id = assignment.Id,
                    Quantity = assignment.InterviewsNeeded,
                    QuestionnaireId = assignment.QuestionnaireId
                });
            }

            return Task.FromResult(assignmentApiViews);
        }
    }
}