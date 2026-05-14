using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class AssignmentsControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly ICommandService commandService;
        private readonly IUserToDeviceService userToDeviceService;

        protected AssignmentsControllerBase(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService,
            IUserToDeviceService userToDeviceService,
            ICommandService commandService)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
            this.commandService = commandService;
            this.userToDeviceService = userToDeviceService;
        }

        public virtual ActionResult<AssignmentApiDocument> GetAssignment(int id)
        {
            var authorizedUserId = this.authorizedUser.Id;

            Assignment assignment = this.assignmentsService.GetAssignment(id);

            if (assignment.ResponsibleId != authorizedUserId && assignment.Responsible.ReadonlyProfile.SupervisorId != authorizedUserId)
            {
                return Forbid();
            }
            
            var isNeedUpdateApp = IsNeedUpdateApp(assignment);
            if (isNeedUpdateApp)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            AssignmentApiDocument assignmentApiDocument = this.assignmentsService.MapAssignment(assignment);

            return assignmentApiDocument;
        }

        public virtual ActionResult<List<AssignmentApiView>> GetAssignments(CancellationToken cancellationToken)
        {
            var authorizedUserId = this.authorizedUser.Id;

            var assignments = GetAssignmentsForResponsible(authorizedUserId);

            var assignmentApiViews = new List<AssignmentApiView>();

            foreach (var assignment in assignments)
            {
                if (IsNeedUpdateApp(assignment))
                    return StatusCode(StatusCodes.Status426UpgradeRequired);
                
                assignmentApiViews.Add(new AssignmentApiView
                {
                    Id = assignment.Id,
                    Quantity = assignment.InterviewsNeeded, // + assignment.InterviewsProvided,
                    QuestionnaireId = assignment.QuestionnaireId,
                    ResponsibleId = assignment.ResponsibleId,
                    ResponsibleName = assignment.Responsible.Name,
                    IsAudioRecordingEnabled = assignment.AudioRecording,
                    TargetArea = assignment.TargetArea,
                    Status = assignment.Status,
                    StatusComment = assignment.StatusComment,
                    UpdatedAtUtc = assignment.UpdatedAtUtc
                });
            }

            return assignmentApiViews;
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
            
            var deviceId = userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id);

            commandService.Execute(
                new MarkAssignmentAsReceivedByTablet(assignment.PublicKey, authorizedUserId, deviceId, assignment.QuestionnaireId));

            return Ok();
        }

        public virtual IActionResult ChangeStatus(int id, [FromBody] AssignmentStatusChangeApiView request)
        {
            if (request == null)
                return BadRequest();

            var assignment = this.assignmentsService.GetAssignment(id);
            if (assignment == null)
                return NotFound("Assignment not found");

            var authorizedUserId = this.authorizedUser.Id;
            if (assignment.ResponsibleId != authorizedUserId &&
                (assignment.Responsible?.ReadonlyProfile?.SupervisorId != authorizedUserId))
            {
                return Forbid();
            }

            try
            {
                switch (request.Status)
                {
                    case AssignmentStatus.Completed:
                        if (!this.authorizedUser.IsInterviewer)
                            return Forbid();
                        commandService.Execute(new CompleteAssignment(assignment.PublicKey, authorizedUserId, assignment.QuestionnaireId, request.Comment));
                        break;
                    case AssignmentStatus.Approved:
                        if (!this.authorizedUser.IsSupervisor)
                            return Forbid();
                        commandService.Execute(new ApproveAssignment(assignment.PublicKey, authorizedUserId, assignment.QuestionnaireId, request.Comment));
                        break;
                    case AssignmentStatus.Open:
                        commandService.Execute(new ReopenAssignment(assignment.PublicKey, authorizedUserId, assignment.QuestionnaireId, request.Comment));
                        break;
                    default:
                        return BadRequest("Unknown status");
                }
            }
            catch (AssignmentException e)
            {
                return BadRequest(new { Message = e.Message });
            }

            return Ok();
        }

        protected abstract IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId);

        protected abstract string ProductName { get; }

        private bool IsNeedUpdateApp(Assignment assignment)
        {
            var productVersion = this.Request.GetProductVersionFromUserAgent(ProductName);

            if (productVersion == null)
                return true;

            if (!string.IsNullOrWhiteSpace(assignment.TargetArea) && productVersion <= new Version(24, 6))
                return true;
            
            return false;
        }
    }
}
