using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        var controller = CreateUsersController();
        RegisterUserModel model = new RegisterUserModel()
        {
            Role = Roles.Interviewer,
            Supervisor = supervisor
        };
        var response = await controller.Register(model);
        
        Assert.That((((ValidationProblemDetails) ((ObjectResult) response.Result).Value).Errors).First().Value.First(), 
            Is.EqualTo(message));
    }
}
