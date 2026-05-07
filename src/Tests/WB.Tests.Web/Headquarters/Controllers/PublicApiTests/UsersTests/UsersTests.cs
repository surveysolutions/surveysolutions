using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public async Task Archive_when_target_user_is_admin_should_return_400_with_message()
    {
        var userId = Guid.NewGuid();
        var adminUser = new UserView
        {
            PublicKey = userId,
            UserName = "admin_user",
            Roles = new HashSet<UserRoles> { UserRoles.Administrator }
        };

        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory
            .Setup(x => x.GetUser(It.IsAny<UserViewInputModel>()))
            .Returns(adminUser);

        var controller = CreateUsersController(userViewViewFactory: userViewFactory.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.Archive(userId.ToString());

        var problemDetails = (ValidationProblemDetails)((ObjectResult)result).Value;
        Assert.That(problemDetails.Errors.ContainsKey("user"), Is.True);
        Assert.That(problemDetails.Errors["user"].First(), Does.Contain("Only interviewers and supervisors"));
    }

    [Test]
    public async Task UnArchive_when_target_user_is_admin_should_return_400_with_message()
    {
        var userId = Guid.NewGuid();
        var adminUser = new UserView
        {
            PublicKey = userId,
            UserName = "admin_user",
            Roles = new HashSet<UserRoles> { UserRoles.Administrator }
        };

        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory
            .Setup(x => x.GetUser(It.IsAny<UserViewInputModel>()))
            .Returns(adminUser);

        var controller = CreateUsersController(userViewViewFactory: userViewFactory.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.UnArchive(userId.ToString());

        var problemDetails = (ValidationProblemDetails)((ObjectResult)result).Value;
        Assert.That(problemDetails.Errors.ContainsKey("user"), Is.True);
        Assert.That(problemDetails.Errors["user"].First(), Does.Contain("Only interviewers and supervisors"));
    }
}
