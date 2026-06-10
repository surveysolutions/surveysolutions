using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentsApiV2ControllerTests
    {
        private AssignmentsApiV2Controller CreateController(
            IAuthorizedUser authorizedUser = null,
            IAssignmentsService assignmentsService = null,
            ICommandService commandService = null,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage = null)
        {
            if (interviewerSettingsStorage == null)
            {
                var storageMock = new Mock<IPlainKeyValueStorage<InterviewerSettings>>();
                storageMock.Setup(s => s.GetById(AppSetting.InterviewerSettings))
                    .Returns(new InterviewerSettings());
                interviewerSettingsStorage = storageMock.Object;
            }

            var controller = new AssignmentsApiV2Controller(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                assignmentsService ?? Mock.Of<IAssignmentsService>(),
                Mock.Of<IUserToDeviceService>(),
                commandService ?? Mock.Of<ICommandService>(),
                interviewerSettingsStorage);

            controller.ControllerContext = new ControllerContext(new ActionContext(
                new DefaultHttpContext(), new RouteData(), new ControllerActionDescriptor()));
            controller.Request.Headers[HeaderNames.UserAgent] =
                "org.worldbank.solutions.interviewer/25.01 (build 33333) (QuestionnaireVersion/27.0.0)";

            return controller;
        }

        [Test]
        public async Task should_return_assignmentsList_accounting_created_interviews()
        {
            var assignmentsList = new List<Assignment>
            {
                Create.Entity.Assignment(quantity: 10, interviewSummary: new HashSet<InterviewSummary>
                {
                    Create.Entity.InterviewSummary(status: InterviewStatus.Completed),
                    Create.Entity.InterviewSummary(status: InterviewStatus.Completed),

                    Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned),

                    Create.Entity.InterviewSummary(status: InterviewStatus.RejectedByHeadquarters),
                    Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor)
                })
            };

            var assignmentService = Mock.Of<IAssignmentsService>(s => s.GetAssignments(It.IsAny<Guid>()) == assignmentsList);

            var controller = CreateController(assignmentsService: assignmentService);

            var assignments = controller.GetAssignments(new CancellationToken()).Value;

            Assert.That(assignments.Single(), Has.Property(nameof(AssignmentApiView.Quantity))
                .EqualTo(10 /* assignment.Quantity */ - 5 /* interviewSummary.Count */));
        }

        [Test]
        public void should_return_assignment_accounting_created_interviews()
        {
            var assignmentEntity = Create.Entity.Assignment(quantity: 10, interviewSummary: new HashSet<InterviewSummary>
            {
                Create.Entity.InterviewSummary(status: InterviewStatus.Completed),
                Create.Entity.InterviewSummary(status: InterviewStatus.Completed),

                Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned),

                Create.Entity.InterviewSummary(status: InterviewStatus.RejectedByHeadquarters),
                Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor)
            });
            
            var assignmentServiceImpl = new AssignmentsService(null, null, null, null);

            var assignment = assignmentServiceImpl.MapAssignment(assignmentEntity);
            
            Assert.That(assignment, Has.Property(nameof(AssignmentApiView.Quantity))
                .EqualTo(10 /* assignment.Quantity */ - 5 /* interviewSummary.Count */));
        }

        [TestCase(AssignmentStatus.Open)]
        [TestCase(AssignmentStatus.Completed)]
        [TestCase(AssignmentStatus.Closed)]
        public void GetAssignments_should_include_status_in_api_view(AssignmentStatus status)
        {
            var assignmentEntity = Create.Entity.Assignment(quantity: 1);
            assignmentEntity.Status = status;
            assignmentEntity.StatusComment = $"Comment for {status}";

            var assignmentService = Mock.Of<IAssignmentsService>(
                s => s.GetAssignments(It.IsAny<Guid>()) == new List<Assignment> { assignmentEntity });

            var controller = CreateController(assignmentsService: assignmentService);

            var result = controller.GetAssignments(new CancellationToken()).Value;

            result.Should().HaveCount(1);
            result[0].Status.Should().Be(status);
            result[0].StatusComment.Should().Be($"Comment for {status}");
        }

        [Test]
        public void GetAssignments_should_return_all_three_statuses_for_mixed_assignments()
        {
            var openAssignment = Create.Entity.Assignment(id: 1, quantity: 5);
            openAssignment.Status = AssignmentStatus.Open;

            var completedAssignment = Create.Entity.Assignment(id: 2, quantity: 3);
            completedAssignment.Status = AssignmentStatus.Completed;
            completedAssignment.StatusComment = "All done";

            var closedAssignment = Create.Entity.Assignment(id: 3, quantity: 2);
            closedAssignment.Status = AssignmentStatus.Closed;
            closedAssignment.StatusComment = "Closed by supervisor";

            var assignmentService = Mock.Of<IAssignmentsService>(
                s => s.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>
                    { openAssignment, completedAssignment, closedAssignment });

            var controller = CreateController(assignmentsService: assignmentService);

            var result = controller.GetAssignments(new CancellationToken()).Value;

            result.Should().HaveCount(3);
            result.Should().ContainSingle(a => a.Status == AssignmentStatus.Open && a.StatusComment == null);
            result.Should().ContainSingle(a => a.Status == AssignmentStatus.Completed && a.StatusComment == "All done");
            result.Should().ContainSingle(a => a.Status == AssignmentStatus.Closed && a.StatusComment == "Closed by supervisor");
        }

        [Test]
        public void ChangeStatus_to_Completed_dispatches_CompleteAssignment_command()
        {
            var assignmentId = 1;
            var assignmentPublicKey = Guid.NewGuid();
            var userId = Id.gA;

            var assignmentEntity = Create.Entity.Assignment(id: assignmentId, publicKey: assignmentPublicKey, responsibleId: userId);
            assignmentEntity.Status = AssignmentStatus.Open;

            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(assignmentId)).Returns(assignmentEntity);

            var commandService = new Mock<ICommandService>();

            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId && u.IsInterviewer == true);

            var controller = CreateController(
                authorizedUser: authorizedUser,
                assignmentsService: assignmentService.Object,
                commandService: commandService.Object);

            controller.ChangeStatus(assignmentId, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Completed,
                Comment = "Done for now"
            });

            commandService.Verify(cs => cs.Execute(
                It.Is<CompleteAssignment>(c => c.PublicKey == assignmentPublicKey && c.Comment == "Done for now"),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ChangeStatus_to_Closed_dispatches_CloseAssignment_command()
        {
            var assignmentId = 2;
            var assignmentPublicKey = Guid.NewGuid();
            var userId = Id.gA;

            var assignmentEntity = Create.Entity.Assignment(id: assignmentId, publicKey: assignmentPublicKey,
                assigneeSupervisorId: userId);
            assignmentEntity.Status = AssignmentStatus.Completed;

            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(assignmentId)).Returns(assignmentEntity);

            var commandService = new Mock<ICommandService>();

            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId && u.IsSupervisor == true);

            var controller = CreateController(
                authorizedUser: authorizedUser,
                assignmentsService: assignmentService.Object,
                commandService: commandService.Object);

            controller.ChangeStatus(assignmentId, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Closed,
                Comment = "Looks good"
            });

            commandService.Verify(cs => cs.Execute(
                It.Is<CloseAssignment>(c => c.PublicKey == assignmentPublicKey && c.Comment == "Looks good"),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ChangeStatus_to_Open_dispatches_ReopenAssignment_command()
        {
            var assignmentId = 3;
            var assignmentPublicKey = Guid.NewGuid();
            var userId = Id.gA;

            var assignmentEntity = Create.Entity.Assignment(id: assignmentId, publicKey: assignmentPublicKey,
                assigneeSupervisorId: userId);
            assignmentEntity.Status = AssignmentStatus.Closed;

            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(assignmentId)).Returns(assignmentEntity);

            var commandService = new Mock<ICommandService>();

            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId && u.IsSupervisor == true);

            var controller = CreateController(
                authorizedUser: authorizedUser,
                assignmentsService: assignmentService.Object,
                commandService: commandService.Object);

            controller.ChangeStatus(assignmentId, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Open,
                Comment = "Need more data"
            });

            commandService.Verify(cs => cs.Execute(
                It.Is<ReopenAssignment>(c => c.PublicKey == assignmentPublicKey && c.Comment == "Need more data"),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ChangeStatus_Complete_by_non_interviewer_returns_Forbid()
        {
            var assignmentId = 4;
            var userId = Id.gA;

            var assignmentEntity = Create.Entity.Assignment(id: assignmentId, assigneeSupervisorId: userId);
            assignmentEntity.Status = AssignmentStatus.Open;

            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(assignmentId)).Returns(assignmentEntity);

            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId && u.IsInterviewer == false && u.IsSupervisor == true);

            var controller = CreateController(authorizedUser: authorizedUser, assignmentsService: assignmentService.Object);

            var result = controller.ChangeStatus(assignmentId, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Completed
            });

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public void ChangeStatus_Close_by_non_supervisor_returns_Forbid()
        {
            var assignmentId = 5;
            var userId = Id.gA;

            var assignmentEntity = Create.Entity.Assignment(id: assignmentId, assigneeSupervisorId: userId);
            assignmentEntity.Status = AssignmentStatus.Completed;

            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(assignmentId)).Returns(assignmentEntity);

            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId && u.IsInterviewer == true && u.IsSupervisor == false);

            var controller = CreateController(authorizedUser: authorizedUser, assignmentsService: assignmentService.Object);

            var result = controller.ChangeStatus(assignmentId, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Closed
            });

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public void ChangeStatus_returns_NotFound_when_assignment_does_not_exist()
        {
            var assignmentService = new Mock<IAssignmentsService>();
            assignmentService.Setup(s => s.GetAssignment(It.IsAny<int>())).Returns((Assignment)null);

            var controller = CreateController(assignmentsService: assignmentService.Object);

            var result = controller.ChangeStatus(999, new AssignmentStatusChangeApiView
            {
                Status = AssignmentStatus.Completed
            });

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
