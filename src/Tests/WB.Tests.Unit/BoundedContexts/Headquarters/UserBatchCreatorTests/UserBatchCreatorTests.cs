using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.UserBatchCreatorTests
{
    [TestFixture]
    internal class UserBatchCreatorTests
    {
        [SetUp]
        public void SetupTests()
        {
            var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            Setup.InstanceToMockedServiceLocator(Mock.Of<IPlainTransactionManager>());
        }

        [TearDown]
        public void CleanTests()
        {
            Setup.InstanceToMockedServiceLocator<IPlainTransactionManager>(null);
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_Then_one_supervisor_should_be_created()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var commantService = new Mock<IIdentityManager>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commantService.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            commantService.Verify(x =>
                x.CreateUser(
                    Moq.It.Is<ApplicationUser>(c => c.UserName == supervisorName),
                    Moq.It.IsAny<string>(),
                    UserRoles.Supervisor), Times.Once);

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_supervisor_should_be_unarchived_and_updated()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var identityManager = new Mock<IIdentityManager>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: identityManager.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            identityManager.Verify(x => x.ArchiveUsersAsync(Moq.It.IsAny<Guid[]>(), false));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_Then_one_interviewer_should_be_created()
        {
            var interviewerName = "inter";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor:"tttt"));
            var identityManager = new Mock<IIdentityManager>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: identityManager.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            identityManager.Verify(
                x => x.CreateUser(Moq.It.Is<ApplicationUser>(c => c.UserName == interviewerName), Moq.It.IsAny<string>(), UserRoles.Interviewer));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_interviewer_should_be_unarchived_and_updated()
        {
            var interviewerName = "inter";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor: "tttt"));
            var identityManager = new Mock<IIdentityManager>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);
            var userStorage = new TestPlainStorage<UserDocument>();
            userStorage.Store(Create.Entity.UserDocument(userName: interviewerName, isArchived: true, supervisorId:Guid.NewGuid()), "id");

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: identityManager.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            identityManager.Verify(x => x.ArchiveUsersAsync(Moq.It.IsAny<Guid[]>(), false));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }


        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_but_command_execution_throws_an_exception_Then_process_should_be_finished_with_error()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var identityManager = new Mock<IIdentityManager>();
            identityManager.Setup(x => x.CreateUser(Moq.It.IsAny<ApplicationUser>(), Moq.It.IsAny<string>(), Moq.It.IsAny<UserRoles>()))
                .Throws<NullReferenceException>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: identityManager.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcessWithError(userPreloadingProcess.UserPreloadingProcessId, Moq.It.IsAny<string>()));
        }

        private UserBatchCreator CreateUserBatchCreator(
           IUserPreloadingService userPreloadingService = null,
           IIdentityManager identityManager = null)
        {
            return new UserBatchCreator(
                userPreloadingService ?? Mock.Of<IUserPreloadingService>(),
                Mock.Of<ILogger>(),
                Mock.Of<IIdentityManager>() ?? identityManager);
        }

        private Mock<IUserPreloadingService> CreateUserPreloadingServiceMock(UserPreloadingProcess userPreloadingProcess, UserRoles role = UserRoles.Interviewer)
        {
            var UserPreloadingProcessIdQueue = new Queue<string>();
            UserPreloadingProcessIdQueue.Enqueue(userPreloadingProcess.UserPreloadingProcessId);
            UserPreloadingProcessIdQueue.Enqueue(null);

            var userPreloadingServiceMock = new Mock<IUserPreloadingService>();
            userPreloadingServiceMock.Setup(x => x.DeQueuePreloadingProcessIdReadyToCreateUsers()).Returns(UserPreloadingProcessIdQueue.Dequeue);
            userPreloadingServiceMock.Setup(x => x.GetPreloadingProcesseDetails(userPreloadingProcess.UserPreloadingProcessId))
                .Returns(userPreloadingProcess);

            userPreloadingServiceMock.Setup(x => x.GetUserRoleFromDataRecord(Moq.It.IsAny<UserPreloadingDataRecord>()))
                .Returns(role);

            return userPreloadingServiceMock;
        }


    }
}