using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.User
{
    [TestFixture]
    public class CreateOrUnArchiveUserRequestHandlerTests
    {
        [Test]
        public async Task when_preload_users_should_set_correct_workspaces_to_it()
        {
            var request = new CreateOrUnArchiveUserRequest();

            var userToImport = new UserToImport()
            {
                Login = "int",
                Password = "int",
                Role = "Supervisor",
                Workspace = "primary, second"
            };
            var importService = Mock.Of<IUserImportService>(s =>
                s.GetUserToImport() == userToImport);

            var workspacesService = new Mock<IWorkspacesService>();

            var handler = CreateHandler(importService, workspacesService.Object);

            var user = await handler.Handle(request, CancellationToken.None);
            
            Assert.That(user.GetWorkspaces().Length, Is.EqualTo(2));
            Assert.That(user.GetWorkspaces()[0], Is.EqualTo("primary"));
            Assert.That(user.GetWorkspaces()[1], Is.EqualTo("second"));

            workspacesService.Verify(s => s.AddUserToWorkspace(
                It.IsAny<HqUser>(),
                "primary",
                null
                ));
            workspacesService.Verify(s => s.AddUserToWorkspace(
                It.IsAny<HqUser>(),
                "second",
                null
                ));
        }

        private CreateOrUnArchiveUserRequestHandler CreateHandler(
            IUserImportService userImportService = null,
            IWorkspacesService workspacesService = null)
        {
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.As<IUserPasswordStore<HqUser>>();
            
            return new CreateOrUnArchiveUserRequestHandler(
                Mock.Of<IUserRepository>(),
                CreateHqUserManager(userStore.Object),
                userImportService ?? Mock.Of<IUserImportService>(),
                Mock.Of<ISystemLog>(),
                workspacesService ?? Mock.Of<IWorkspacesService>() 
                );
        }
        
        private HqUserManager CreateHqUserManager(IUserStore<HqUser> userStore)
        {
            var hqUserManager = new HqUserManager(
                userStore ?? new Mock<IUserStore<HqUser>>().As<IUserPasswordStore<HqUser>>().Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<HqUser>>(),
                new List<IUserValidator<HqUser>>(),
                new List<IPasswordValidator<HqUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<HqUserManager>>(),
                Mock.Of<IWorkspacesService>(),
                Mock.Of<ISystemLog>(),
                Mock.Of<IAuthorizedUser>());
            hqUserManager.RegisterTokenProvider(
                hqUserManager.Options.Tokens.PasswordResetTokenProvider, 
                Mock.Of<IUserTwoFactorTokenProvider<HqUser>>(v => 
                    v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), hqUserManager,
                        It.IsAny<HqUser>()) == Task.FromResult(true)));
            return hqUserManager;
        }
    }
}