using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.UsersApiControllerTests
{
    [NUnit.Framework.TestOf(typeof(UsersApiController))]
    internal class UsersApiControllerTestsContext
    {
        protected static UsersApiController CreateController(
            IMediator mediator = null,
            IUserImportService userImportService = null)
        {
            return new UsersApiController(
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IUserViewFactory>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<IInterviewerVersionReader>(),
                Mock.Of<IExportFactory>(),
                Mock.Of<IInterviewerProfileFactory>(),
                Mock.Of<IFileSystemAccessor>(),
                userImportService ?? Mock.Of<IUserImportService>(),
                Mock.Of<IMoveUserToAnotherTeamService>(),
                Mock.Of<IUserArchiveService>(),
                mediator ?? Mock.Of<IMediator>(),
                Mock.Of<ILogger<UsersApiController>>());
        }
    }2222222
}

