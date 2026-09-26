using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.UsersApiControllerTests
{
    internal class when_importing_users_and_error_contains_tags_in_cell_value : UsersApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public async Task context()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(x => x.Send(It.IsAny<UserImportRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserImportVerificationError[])new[]
                {
                    new UserImportVerificationError
                    {
                        Code = "PLU0007",
                        RowNumber = 1,
                        ColumnName = "<i>LoGin</i>",
                        CellValue = "<i>test > 1</i>"
                    }
                });

            controller = CreateController(mediator: mediatorMock.Object);
            await BecauseOf();
        }

        public async Task BecauseOf()
        {
            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("login\tpassword"));
            var formFile = new FormFile(fileStream, 0, fileStream.Length, "File", "users.tab");

            actionResult = await controller.ImportUsers(new ImportUsersRequest
            {
                File = formFile,
                Workspace = "primary"
            });

            importErrors = ((OkObjectResult)actionResult).Value as UsersApiController.ImportUserError[];
        }

        [NUnit.Framework.Test]
        public void should_return_import_errors() =>
            importErrors.Should().NotBeNull();

        [NUnit.Framework.Test]
        public void should_remove_tags_from_column() =>
            importErrors.Single().Column.Should().Be("login");

        [NUnit.Framework.Test]
        public void should_remove_tags_from_description_cell_value() =>
            importErrors.Single().Description.Should().Contain("test &gt; 1");

        [NUnit.Framework.Test]
        public void should_include_Recommendation_for_error_code() =>
            importErrors.Single().Recommendation
                .Should().Be(UserPreloadingVerificationMessages.ResourceManager.GetString("PLU0007Recommendation"));

        private static UsersApiController controller;
        private static IActionResult actionResult;
        private static UsersApiController.ImportUserError[] importErrors;
    }
}
