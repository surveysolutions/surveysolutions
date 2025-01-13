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

        public virtual Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
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
                    TargetArea = assignment.TargetArea
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
            
            var deviceId = userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id);

            commandService.Execute(
                new MarkAssignmentAsReceivedByTablet(assignment.PublicKey, authorizedUserId, deviceId, assignment.QuestionnaireId));

            return Ok();
        }

        protected abstract IEnumerable<Assignment> GetAssignmentsForResponsible(Guid responsibleId);

        protected abstract string ProductName { get; }

        private bool IsNeedUpdateApp(Assignment assignment)
        {
            var productVersion = this.Request.GetProductVersionFromUserAgent(ProductName);

            if (!string.IsNullOrWhiteSpace(assignment.TargetArea) 
                && productVersion != null 
                && productVersion <= new Version(24, 6))
            {
#if DEBUG 
                return false;
#else
                return true;
#endif
            }
            
            return false;
        }
    }
}
