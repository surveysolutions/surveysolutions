using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Code.UsersManagement;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.Users;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(UsersController))]
    public class UsersControllerTests
    {
        #region UpdatePassword Tests

        [Test]
        public async Task when_UpdatePassword_by_admin_should_set_flag_PasswordChangeRequired()
        {
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();

            var user = Mock.Of<HqUser>(u => u.Id == userId);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u =>
                u.IsAdministrator == true && u.Id == authorizedUserId && u.UserName == "admin");
            
            var userManager = new Mock<IUserStore<HqUser>>();
            userManager.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));
            userManager.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(IdentityResult.Success));
            userManager.As<IUserPasswordStore<HqUser>>();
            
            var controller = CreateController(userManager.Object, authorizedUser);

            await controller.UpdatePassword(new ChangePasswordModel()
            {
                Password = "11",
                ConfirmPassword = "11",
                UserId = userId,
                OldPassword = "1"
            });

            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task when_UpdatePassword_by_own_user_should_not_set_flag_PasswordChangeRequired()
        {
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId);
            var userManager = new Mock<IUserStore<HqUser>>();
            userManager.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));
            userManager.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new IdentityResult()));
            userManager.As<IUserPasswordStore<HqUser>>();
            
            var controller = CreateController(userManager.Object, authorizedUser);

            await controller.UpdatePassword(new ChangePasswordModel()
            {
                Password = "11",
                ConfirmPassword = "11",
                UserId = userId,
                OldPassword = "1"
            });

            Assert.That(user.PasswordChangeRequired, Is.False);
        }

        [Test]
        public async Task UpdatePassword_WhenModelStateIsInvalid_ShouldReturnValidationErrors()
        {
            // Arrange
            var controller = CreateControllerForUpdatePassword();
            controller.ModelState.AddModelError("Password", "Required");

            var model = new ChangePasswordModel { UserId = Guid.NewGuid() };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(((JsonResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdatePassword_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userManagerStore = CreateUserManagerMockForUpdatePassword(userToUpdate: null);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdatePassword_WhenUserLacksPermissionToManage_ShouldReturnForbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => u.Id == userId && u.IsInRole(UserRoles.Supervisor) == true);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => 
                u.Id == authorizedUserId && 
                u.IsAdministrator == false &&
                u.IsHeadquarter == false);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(userToUpdate: user);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        }

        [Test]
        public async Task UpdatePassword_WhenUserIsArchived_ShouldAddModelError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => u.Id == userId && u.IsArchived == true);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(userToUpdate: user);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(ChangePasswordModel.Password)), Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenAccountIsRestricted_ShouldReturnForbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => u.Id == userId);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => 
                u.Id == userId && 
                u.IsAdministrator == true &&
                u.UserName == "admin" &&
                u.PasswordChangeRequired == false);

            var accountConfig = new AccountManagementConfig { RestrictedUser = new[] { "admin" } };
            var usersManagementSettings = new UsersManagementSettings(accountConfig);
            var userManagerStore = CreateUserManagerMockForUpdatePassword(userToUpdate: user);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(
                userManager: userManager, 
                authorizedUser: authorizedUser,
                usersManagementSettings: usersManagementSettings);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        }

        

        [Test]
        public async Task UpdatePassword_WhenPasswordResetSucceeds_ShouldReturnSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => u.Id == userId && u.IsInRole(UserRoles.ApiUser) == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenUpdatingApiUserPassword_ShouldNotSetPasswordChangeRequired()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.IsInRole(UserRoles.ApiUser) == true &&
                u.PasswordChangeRequired == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(user.PasswordChangeRequired, Is.False);
        }

        [Test]
        public async Task UpdatePassword_WhenAdminChangesPasswordForHeadquarter_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.IsInRole(UserRoles.Headquarter) == true &&
                u.IsInRole(UserRoles.ApiUser) == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenAdminChangesPasswordForInterviewer_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.IsInRole(UserRoles.Interviewer) == true &&
                u.IsInRole(UserRoles.ApiUser) == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenAdminChangesPasswordForObserver_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.IsInRole(UserRoles.Observer) == true &&
                u.IsInRole(UserRoles.ApiUser) == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenUpdateUserFails_ShouldAddModelError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedUserId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.IsInRole(UserRoles.ApiUser) == false);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == authorizedUserId && u.IsAdministrator == true);

            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success,
                updateUserResult: IdentityResult.Failed(new IdentityError 
                { 
                    Code = "UpdateFailed", 
                    Description = "Failed to update user" 
                }));
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(userManager: userManager, authorizedUser: authorizedUser);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            await controller.UpdatePassword(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(ChangePasswordModel.Password)), Is.True);
        }

        [Test]
        public async Task UpdatePassword_WhenRestrictedAccountButPasswordChangeRequired_ShouldAllow()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var user = Mock.Of<HqUser>(u => 
                u.Id == userId && 
                u.PasswordChangeRequired == true);
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => 
                u.Id == userId && 
                u.IsAdministrator == true &&
                u.UserName == "admin" &&
                u.PasswordChangeRequired == true);

            var accountConfig = new AccountManagementConfig { RestrictedUser = new[] { "admin" } };
            var usersManagementSettings = new UsersManagementSettings(accountConfig);
            var userManagerStore = CreateUserManagerMockForUpdatePassword(
                userToUpdate: user, 
                resetPasswordResult: IdentityResult.Success,
                checkPasswordResult: true);
            var userManager = CreateHqUserManager(userManagerStore);
            var controller = CreateControllerForUpdatePassword(
                userManager: userManager, 
                authorizedUser: authorizedUser,
                usersManagementSettings: usersManagementSettings);

            var model = new ChangePasswordModel
            {
                UserId = userId,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
                OldPassword = "OldPassword123!"
            };

            // Act
            var result = await controller.UpdatePassword(model);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        #endregion
        
        #region CreateUser Tests

        [Test]
        public async Task CreateUser_WhenModelStateIsInvalid_ShouldReturnValidationErrors()
        {
            // Arrange
            var controller = CreateControllerForCreateUser();
            controller.ModelState.AddModelError("UserName", "Required");

            var model = new CreateUserModel();

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(((JsonResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateUser_WhenRoleIsInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var controller = CreateControllerForCreateUser();
            var model = new CreateUserModel
            {
                Role = "InvalidRole",
                Workspace = "primary",
                UserName = "testuser",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.Value, Is.EqualTo("Unknown user type"));
        }

        [Test]
        public async Task CreateUser_WhenWorkspaceIsNull_ShouldReturnBadRequest()
        {
            // Arrange
            var controller = CreateControllerForCreateUser();
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = null,
                UserName = "testuser",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.Value, Is.EqualTo("Unknown user workspace"));
        }

        [Test]
        public async Task CreateUser_WhenWorkspaceIsEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            var controller = CreateControllerForCreateUser();
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = "",
                UserName = "testuser",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.Value, Is.EqualTo("Unknown user workspace"));
        }

        [Test]
        public async Task CreateUser_WhenHeadquarterTriesToCreateAdministrator_ShouldReturnForbid()
        {
            // Arrange
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.IsHeadquarter == true && u.IsAdministrator == false);
            var controller = CreateControllerForCreateUser(authorizedUser: authorizedUser);
            var model = new CreateUserModel
            {
                Role = "Administrator",
                Workspace = "primary",
                UserName = "testadmin",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CreateUser_WhenHeadquarterTriesToCreateHeadquarter_ShouldReturnForbid()
        {
            // Arrange
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.IsHeadquarter == true && u.IsAdministrator == false);
            var controller = CreateControllerForCreateUser(authorizedUser: authorizedUser);
            var model = new CreateUserModel
            {
                Role = "Headquarter",
                Workspace = "primary",
                UserName = "testhq",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CreateUser_WhenHeadquarterTriesToCreateObserver_ShouldReturnForbid()
        {
            // Arrange
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.IsHeadquarter == true && u.IsAdministrator == false);
            var controller = CreateControllerForCreateUser(authorizedUser: authorizedUser);
            var model = new CreateUserModel
            {
                Role = "Observer",
                Workspace = "primary",
                UserName = "testobserver",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CreateUser_WhenTryingToCreateAdministrator_ShouldReturnForbid()
        {
            // Arrange
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.IsAdministrator == true);
            var controller = CreateControllerForCreateUser(authorizedUser: authorizedUser);
            var model = new CreateUserModel
            {
                Role = "Administrator",
                Workspace = "primary",
                UserName = "testadmin",
                Password = "Test@123"
            };

            // Act
            var result = await controller.CreateUser(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CreateUser_WhenInterviewerWithoutSupervisor_ShouldAddModelError()
        {
            // Arrange
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);
            
            var controller = CreateControllerForCreateUser(workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testinterviewer",
                Password = "Test@123",
                SupervisorId = null
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.SupervisorId)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenUserNameAlreadyExists_ShouldAddModelError()
        {
            // Arrange
            var existingUser = new HqUser { UserName = "existinguser" };
            var userManagerStore = CreateUserManagerMock(existingUser: existingUser);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Headquarter",
                Workspace = workspaceId,
                UserName = "existinguser",
                Password = "Test@123"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.UserName)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenWorkspaceDoesNotExist_ShouldAddModelError()
        {
            // Arrange
            var workspacesStorage = CreateWorkspacesStorage("primary", null);
            var controller = CreateControllerForCreateUser(workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = "nonexistent",
                UserName = "testuser",
                Password = "Test@123",
                SupervisorId = Guid.NewGuid()
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.Workspace)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenWorkspaceIsRemoved_ShouldAddModelError()
        {
            // Arrange
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var removedAtUtcProperty = workspace.GetType().GetProperty("RemovedAtUtc");
            if (removedAtUtcProperty != null)
            {
                removedAtUtcProperty.SetValue(workspace, DateTime.UtcNow);
            }
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);
            
            var controller = CreateControllerForCreateUser(workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testuser",
                Password = "Test@123",
                SupervisorId = Guid.NewGuid()
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.Workspace)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenSupervisorDoesNotExist_ShouldAddModelError()
        {
            // Arrange
            var supervisorId = Guid.NewGuid();
            var userManagerStore = CreateUserManagerMock(supervisorId: supervisorId, supervisor: null);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testinterviewer",
                Password = "Test@123",
                SupervisorId = supervisorId
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.SupervisorId)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenSupervisorIsNotInSupervisorRole_ShouldAddModelError()
        {
            // Arrange
            var supervisorId = Guid.NewGuid();
            var supervisor = Mock.Of<HqUser>(s => 
                s.Id == supervisorId && 
                s.IsInRole(UserRoles.Supervisor) == false &&
                s.IsArchivedOrLocked == false);
            
            var userManagerStore = CreateUserManagerMock(supervisorId: supervisorId, supervisor: supervisor);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testinterviewer",
                Password = "Test@123",
                SupervisorId = supervisorId
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.SupervisorId)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenSupervisorIsArchived_ShouldAddModelError()
        {
            // Arrange
            var supervisorId = Guid.NewGuid();
            var supervisor = Mock.Of<HqUser>(s => 
                s.Id == supervisorId && 
                s.IsInRole(UserRoles.Supervisor) == true &&
                s.IsArchivedOrLocked == true);
            
            var userManagerStore = CreateUserManagerMock(supervisorId: supervisorId, supervisor: supervisor);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testinterviewer",
                Password = "Test@123",
                SupervisorId = supervisorId
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.SupervisorId)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenCreatingHeadquarterSuccessfully_ShouldCreateUserWithCorrectProperties()
        {
            // Arrange
            HqUser createdUser = null;
            var userManagerStore = CreateUserManagerMock(
                onCreateUser: user => createdUser = user,
                createResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Headquarter",
                Workspace = workspaceId,
                UserName = "testhq",
                Password = "Test@123",
                PersonName = "Test HQ",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                IsLockedByHeadquarters = true,
                IsLockedBySupervisor = false
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.UserName, Is.EqualTo("testhq"));
            Assert.That(createdUser.FullName, Is.EqualTo("Test HQ"));
            Assert.That(createdUser.Email, Is.EqualTo("test@example.com"));
            Assert.That(createdUser.PhoneNumber, Is.EqualTo("1234567890"));
            Assert.That(createdUser.IsLockedByHeadquaters, Is.True);
            Assert.That(createdUser.IsLockedBySupervisor, Is.False);
            Assert.That(createdUser.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task CreateUser_WhenCreatingSupervisorSuccessfully_ShouldCreateUserWithPasswordChangeRequired()
        {
            // Arrange
            HqUser createdUser = null;
            var userManagerStore = CreateUserManagerMock(
                onCreateUser: user => createdUser = user,
                createResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Supervisor",
                Workspace = workspaceId,
                UserName = "testsupervisor",
                Password = "Test@123",
                PersonName = "Test Supervisor"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.PasswordChangeRequired, Is.True);
        }

        [Test]
        public async Task CreateUser_WhenCreatingApiUserSuccessfully_ShouldCreateUserWithoutPasswordChangeRequired()
        {
            // Arrange
            HqUser createdUser = null;
            var userManagerStore = CreateUserManagerMock(
                onCreateUser: user => createdUser = user,
                createResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "ApiUser",
                Workspace = workspaceId,
                UserName = "testapiuser",
                Password = "Test@123",
                PersonName = "Test API User"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.PasswordChangeRequired, Is.False);
        }

        [Test]
        public async Task CreateUser_WhenCreatingInterviewerWithSupervisorSuccessfully_ShouldCreateUserWithWorkspaceAndSupervisor()
        {
            // Arrange
            var supervisorId = Guid.NewGuid();
            var supervisor = Mock.Of<HqUser>(s => 
                s.Id == supervisorId && 
                s.IsInRole(UserRoles.Supervisor) == true &&
                s.IsArchivedOrLocked == false);
            
            HqUser createdUser = null;
            var userManagerStore = CreateUserManagerMock(
                supervisorId: supervisorId,
                supervisor: supervisor,
                onCreateUser: user => createdUser = user,
                createResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Interviewer",
                Workspace = workspaceId,
                UserName = "testinterviewer",
                Password = "Test@123",
                PersonName = "Test Interviewer",
                SupervisorId = supervisorId
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.Workspaces, Has.Count.EqualTo(1));
            Assert.That(createdUser.Workspaces.First().Workspace, Is.EqualTo(workspace));
            Assert.That(createdUser.Workspaces.First().Supervisor, Is.EqualTo(supervisor));
        }

        [Test]
        public async Task CreateUser_WhenPasswordValidationFails_ShouldAddPasswordModelError()
        {
            // Arrange
            var userManagerStore = CreateUserManagerMock(
                createResult: IdentityResult.Failed(new IdentityError 
                { 
                    Code = "PasswordTooShort", 
                    Description = "Password must be at least 6 characters" 
                }));
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Headquarter",
                Workspace = workspaceId,
                UserName = "testuser",
                Password = "123"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.Password)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenUserNameValidationFails_ShouldAddUserNameModelError()
        {
            // Arrange
            var userManagerStore = CreateUserManagerMock(
                createResult: IdentityResult.Failed(new IdentityError 
                { 
                    Code = "InvalidUserName", 
                    Description = "User name contains invalid characters" 
                }));
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Headquarter",
                Workspace = workspaceId,
                UserName = "test user",
                Password = "Test@123"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(controller.ModelState.IsValid, Is.False);
            Assert.That(controller.ModelState.ContainsKey(nameof(CreateUserModel.UserName)), Is.True);
        }

        [Test]
        public async Task CreateUser_WhenSuccessful_ShouldAddUserToRole()
        {
            // Arrange
            var addedToRole = false;
            var userManagerStore = CreateUserManagerMock(
                createResult: IdentityResult.Success,
                onAddToRole: (_, role) => addedToRole = role == "Supervisor");
                
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Supervisor",
                Workspace = workspaceId,
                UserName = "testsupervisor",
                Password = "Test@123"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(addedToRole, Is.True);
        }

        [Test]
        public async Task CreateUser_WhenCreatingObserverSuccessfully_ShouldCreateUser()
        {
            // Arrange
            HqUser createdUser = null;
            var userManagerStore = CreateUserManagerMock(
                onCreateUser: user => createdUser = user,
                createResult: IdentityResult.Success);
            var userManager = CreateHqUserManager(userManagerStore);
            var workspaceId = "primary";
            var workspace = new Workspace(workspaceId, "Primary", DateTime.UtcNow);
            var workspacesStorage = CreateWorkspacesStorage(workspaceId, workspace);

            var controller = CreateControllerForCreateUser(userManager: userManager, workspacesStorage: workspacesStorage);
            var model = new CreateUserModel
            {
                Role = "Observer",
                Workspace = workspaceId,
                UserName = "testobserver",
                Password = "Test@123",
                PersonName = "Test Observer"
            };

            // Act
            await controller.CreateUser(model);

            // Assert
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.UserName, Is.EqualTo("testobserver"));
            Assert.That(createdUser.PasswordChangeRequired, Is.True);
        }

        #endregion

        #region Helper Methods for CreateUser Tests

        private UsersController CreateControllerForCreateUser(
            HqUserManager userManager = null,
            IAuthorizedUser authorizedUser = null,
            IWorkspacesStorage workspacesStorage = null)
        {
            var controller = new UsersController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(u => u.IsAdministrator == true),
                userManager ?? CreateHqUserManager(CreateUserManagerMock()),
                Mock.Of<IPlainKeyValueStorage<ProfileSettings>>(),
                Mock.Of<UrlEncoder>(),
                Mock.Of<IOptions<HeadquartersConfig>>(),
                workspacesStorage ?? CreateWorkspacesStorage("primary", new Workspace("primary", "Primary", DateTime.UtcNow)),
                Mock.Of<ITokenProvider>(),
                new UsersManagementSettings(null));
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c => 
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r => r.Cookies == Mock.Of<IRequestCookieCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }

        private IUserStore<HqUser> CreateUserManagerMock(
            HqUser existingUser = null,
            Guid? supervisorId = null,
            HqUser supervisor = null,
            Action<HqUser> onCreateUser = null,
            IdentityResult createResult = null,
            Action<HqUser, string> onAddToRole = null)
        {
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.As<IUserPasswordStore<HqUser>>();
            userStore.As<IUserRoleStore<HqUser>>();

            // Setup FindByNameAsync
            userStore.Setup(u => u.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string username, CancellationToken _) =>
                {
                    if (existingUser != null && existingUser.UserName == username)
                        return existingUser;
                    return null;
                });

            // Setup FindByIdAsync for supervisor lookup
            if (supervisorId.HasValue)
            {
                userStore.Setup(u => u.FindByIdAsync(supervisorId.Value.FormatGuid(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(supervisor);
            }
            
            

            // Setup CreateAsync
            userStore.As<IUserPasswordStore<HqUser>>()
                .Setup(u => u.CreateAsync(It.IsAny<HqUser>(), It.IsAny<CancellationToken>()))
                .Callback<HqUser, CancellationToken>((user, _) => onCreateUser?.Invoke(user))
                .ReturnsAsync(createResult ?? IdentityResult.Success);

            // Setup AddToRoleAsync
            userStore.As<IUserRoleStore<HqUser>>()
                .Setup(u => u.AddToRoleAsync(It.IsAny<HqUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<HqUser, string, CancellationToken>((user, role, _) => onAddToRole?.Invoke(user, role))
                .Returns(Task.CompletedTask);

            return userStore.Object;
        }

        private IWorkspacesStorage CreateWorkspacesStorage(string workspaceId, Workspace workspace)
        {
            var storage = new Mock<IWorkspacesStorage>();
            storage.Setup(s => s.GetByIdAsync(workspaceId))
                .ReturnsAsync(workspace);
            return storage.Object;
        }

        #endregion

        #region Helper Methods for UpdatePassword Tests

        private UsersController CreateControllerForUpdatePassword(
            HqUserManager userManager = null,
            IAuthorizedUser authorizedUser = null,
            UsersManagementSettings usersManagementSettings = null)
        {
            var controller = new UsersController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(u => u.IsAdministrator == true),
                userManager ?? CreateHqUserManager(CreateUserManagerMockForUpdatePassword()),
                Mock.Of<IPlainKeyValueStorage<ProfileSettings>>(),
                Mock.Of<UrlEncoder>(),
                Mock.Of<IOptions<HeadquartersConfig>>(),
                Mock.Of<IWorkspacesStorage>(),
                Mock.Of<ITokenProvider>(),
                usersManagementSettings ?? new UsersManagementSettings(null));
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c => 
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r => r.Cookies == Mock.Of<IRequestCookieCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }

        private IUserStore<HqUser> CreateUserManagerMockForUpdatePassword(
            HqUser userToUpdate = null,
            bool checkPasswordResult = true,
            IdentityResult resetPasswordResult = null,
            IdentityResult updateUserResult = null)
        {
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.As<IUserPasswordStore<HqUser>>();

            // Setup FindByIdAsync
            userStore.Setup(u => u.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string userId, CancellationToken _) => userToUpdate);

            // Setup UpdateAsync
            userStore.Setup(u => u.UpdateAsync(It.IsAny<HqUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateUserResult ?? IdentityResult.Success);

            return userStore.Object;
        }

        #endregion

        private UsersController CreateController(
            IUserStore<HqUser> userManager = null,
            IAuthorizedUser authorizedUser = null)
        {
            var controller = new UsersController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                CreateHqUserManager(userManager),
                Mock.Of<IPlainKeyValueStorage<ProfileSettings>>(),
                Mock.Of<UrlEncoder>(),
                Mock.Of<IOptions<HeadquartersConfig>>(),
                null,
                null,
                new UsersManagementSettings(null));
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c => 
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r => r.Cookies == Mock.Of<IRequestCookieCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }

        private HqUserManager CreateHqUserManager(IUserStore<HqUser> userStore)
        {
            var normalizer = new Mock<ILookupNormalizer>();
            normalizer.Setup(x=> x.NormalizeName(It.IsAny<string>())).Returns((string name)=>name);
            
            var hqUserManager = new HqUserManager(
                userStore ?? new Mock<IUserStore<HqUser>>().As<IUserPasswordStore<HqUser>>().Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<HqUser>>(),
                new List<IUserValidator<HqUser>>(),
                new List<IPasswordValidator<HqUser>>(),
                normalizer.Object,
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<HqUserManager>>(),
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
