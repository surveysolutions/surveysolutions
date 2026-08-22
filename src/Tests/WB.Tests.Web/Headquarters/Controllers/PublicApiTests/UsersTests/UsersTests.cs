using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.UsersTests;

[TestOf(nameof(UsersController))]
internal class UsersTests : ApiTestContext
{
    [Test]
    [TestCase("super1", "Supervisor was not found")]
    [TestCase(null, "Supervisor name is required for interviewer creation")]
    public async Task when_creating_interviewer_without_valid_supervisor(string supervisor, string message)
    {
        var workspaceContextAccessor = new Mock<IWorkspaceContextAccessor>();

        workspaceContextAccessor.Setup(x => x.CurrentWorkspace())
            .Returns(new WorkspaceContext("test", "test"));
        
        var controller = CreateUsersController(workspaceContextAccessor: workspaceContextAccessor.Object);
        RegisterUserModel model = new RegisterUserModel()
        {
            Role = Roles.Interviewer,
            Supervisor = supervisor
        };
        var response = await controller.Register(model);
        
        Assert.That((((ValidationProblemDetails) ((ObjectResult) response.Result).Value).Errors).First().Value.First(), 
            Is.EqualTo(message));
    }

    [Test]
    public async Task when_moving_interviewer_and_interviewer_not_found_should_return_not_found()
    {
        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory.Setup(x => x.GetUser(It.IsAny<UserViewInputModel>())).Returns((UserView)null);

        var controller = CreateUsersController(userViewViewFactory: userViewFactory.Object);

        var result = await controller.MoveInterviewerToAnotherTeam(Guid.NewGuid(), new MoveInterviewerRequest
        {
            SupervisorId = Guid.NewGuid(),
            Mode = MoveUserToAnotherTeamMode.MoveAllToNewTeam
        });

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task when_moving_non_interviewer_should_return_not_found()
    {
        var interviewerId = Guid.NewGuid();
        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == interviewerId)))
            .Returns(new UserView { PublicKey = interviewerId, Roles = new HashSet<UserRoles> { UserRoles.Supervisor } });

        var controller = CreateUsersController(userViewViewFactory: userViewFactory.Object);

        var result = await controller.MoveInterviewerToAnotherTeam(interviewerId, new MoveInterviewerRequest
        {
            SupervisorId = Guid.NewGuid(),
            Mode = MoveUserToAnotherTeamMode.MoveAllToNewTeam
        });

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task when_moving_interviewer_and_supervisor_not_found_should_return_not_found()
    {
        var interviewerId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();

        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == interviewerId)))
            .Returns(new UserView
            {
                PublicKey = interviewerId,
                Roles = new HashSet<UserRoles> { UserRoles.Interviewer },
                Supervisor = new UserLight(Guid.NewGuid(), "old-super")
            });
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == supervisorId)))
            .Returns((UserView)null);

        var controller = CreateUsersController(userViewViewFactory: userViewFactory.Object);

        var result = await controller.MoveInterviewerToAnotherTeam(interviewerId, new MoveInterviewerRequest
        {
            SupervisorId = supervisorId,
            Mode = MoveUserToAnotherTeamMode.MoveAllToNewTeam
        });

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task when_moving_interviewer_should_call_service_and_return_ok()
    {
        var interviewerId = Guid.NewGuid();
        var oldSupervisorId = Guid.NewGuid();
        var newSupervisorId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == interviewerId)))
            .Returns(new UserView
            {
                PublicKey = interviewerId,
                Roles = new HashSet<UserRoles> { UserRoles.Interviewer },
                Supervisor = new UserLight(oldSupervisorId, "old-super")
            });
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == newSupervisorId)))
            .Returns(new UserView
            {
                PublicKey = newSupervisorId,
                Roles = new HashSet<UserRoles> { UserRoles.Supervisor }
            });
        userViewFactory.Setup(x => x.GetUser(It.Is<UserViewInputModel>(m => m.PublicKey == oldSupervisorId)))
            .Returns(new UserView
            {
                PublicKey = oldSupervisorId,
                Roles = new HashSet<UserRoles> { UserRoles.Supervisor }
            });

        var moveService = new Mock<IMoveUserToAnotherTeamService>();
        moveService.Setup(x => x.Move(currentUserId, interviewerId, newSupervisorId, oldSupervisorId, MoveUserToAnotherTeamMode.MoveAllToNewTeam))
            .ReturnsAsync(new MoveInterviewerToAnotherTeamResult { InterviewsProcessed = 1 });

        var authorizedUser = new Mock<IAuthorizedUser>();
        authorizedUser.Setup(x => x.Id).Returns(currentUserId);

        var controller = CreateUsersController(
            userViewViewFactory: userViewFactory.Object,
            moveUserToAnotherTeamService: moveService.Object,
            authorizedUser: authorizedUser.Object);

        var result = await controller.MoveInterviewerToAnotherTeam(interviewerId, new MoveInterviewerRequest
        {
            SupervisorId = newSupervisorId,
            Mode = MoveUserToAnotherTeamMode.MoveAllToNewTeam
        });

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        moveService.Verify(x => x.Move(currentUserId, interviewerId, newSupervisorId, oldSupervisorId, MoveUserToAnotherTeamMode.MoveAllToNewTeam), Times.Once);
    }
}
