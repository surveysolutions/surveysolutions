using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class AssignmentsControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly ICommandService commandService;

        protected AssignmentsControllerBase(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService,
            ICommandService commandService)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
            this.commandService = commandService;
        }

        public virtual ActionResult<AssignmentApiDocument> GetAssignment(int id)
        {
            var authorizedUserId = this.authorizedUser.Id;

            Assignment assignment = this.assignmentsService.GetAssignment(id);

            if (assignment.ResponsibleId != authorizedUserId && assignment.Responsible.ReadonlyProfile.SupervisorId != authorizedUserId)
            {
                return Forbid();
            }

            AssignmentApiDocument assignmentApiDocument = this.assignmentsService.MapAssignment(assignment);

            return assignmentApiDocument;
        }

        public virtual Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
        {
            var authorizedUserId = this.authorizedUser.Id;

            var assignments = GetAssignmentsForResponsible(authorizedUserId);

            var assignmentApiViews = new List<AssignmentApiView>();

            foreach (var assignment in assignments)
            {
                assignmentApiViews.Add(new AssignmentApiView
                {
                    Id = assignment.Id,
                    Quantity = assignment.InterviewsNeeded, // + assignment.InterviewsProvided,
                    QuestionnaireId = assignment.QuestionnaireId,
                    ResponsibleId = assignment.ResponsibleId,
                    ResponsibleName = assignment.Responsible.Name,
                    IsAudioRecordingEnabled = assignment.AudioRecording
                });
            }

            return Task.FromResult(assignmentApiViews);
        }

        public virtual IActionResult Received(int id)
        {
            var assignment = this.assignmentsService.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound("Assignment not found");
            }

            var authorizedUserId = this.authorizedUser.Id;
            if (assignment.ResponsibleId != authorizedUserId &&
                assignment.Responsible.ReadonlyProfile.SupervisorId != authorizedUserId)
            {
                return NotFound("Assignment was reassigned");
            }

            commandService.Execute(
                new MarkAssignmentAsReceivedByTablet(assignment.PublicKey, authorizedUserId, assignment.QuestionnaireId));

            return Ok();
        }

        protected abstract IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId);

    }
}
