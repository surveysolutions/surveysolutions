using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

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
        public async Task CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_Then_one_supervisor_should_be_created()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var userManager = new Mock<TestHqUserManager>();

            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator = CreateUserBatchCreator(userPreloadingServiceMock.Object, userManager.Object);

            await userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync();

            userManager.Verify(x =>
                x.CreateUserAsync(
                    Moq.It.Is<HqUser>(c => c.UserName == supervisorName),
                    Moq.It.IsAny<string>(),
                    UserRoles.Supervisor), Times.Once);

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public async Task
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_supervisor_should_be_unarchived_and_updated()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var userManager = new Mock<TestHqUserManager>();
            userManager.Setup(x => x.FindByNameAsync("tttt")).ReturnsAsync(Create.Entity.HqUser(role: UserRoles.Supervisor, isArchived: true));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: userManager.Object);

            await userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync();

            userManager.Verify(x => x.CreateUserAsync(Moq.It.Is<HqUser>(c => c.UserName == supervisorName), Moq.It.IsAny<string>(), UserRoles.Supervisor));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public async Task
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_Then_one_interviewer_should_be_created()
        {
            var interviewerName = "inter";
            var supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor:"tttt"));
            var userManager = new Mock<TestHqUserManager>();
            userManager.Setup(x => x.FindByNameAsync("tttt")).ReturnsAsync(Create.Entity.HqUser(role: UserRoles.Supervisor, userId: supervisorId));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: userManager.Object);

            await userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync();

            userManager.Verify(x => x.CreateUserAsync(Moq.It.Is<HqUser>(c => c.UserName == interviewerName), Moq.It.IsAny<string>(), UserRoles.Interviewer));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public async Task
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_interviewer_should_be_unarchived_and_updated()
        {
            var interviewerName = "inter";
            var supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor: "tttt"));
            var userManager = new Mock<TestHqUserManager>();
            userManager.Setup(x => x.FindByNameAsync("inter")).ReturnsAsync(Create.Entity.HqUser(userName: interviewerName, isArchived: true, supervisorId: supervisorId));
            userManager.Setup(x => x.FindByNameAsync("tttt")).ReturnsAsync(Create.Entity.HqUser(role: UserRoles.Supervisor, userId: supervisorId));

            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);
            var userBatchCreator = CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: userManager.Object);

            await userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync();

            userManager.Verify(x => x.UpdateUserAsync(Moq.It.Is<HqUser>(c => c.UserName == interviewerName), Moq.It.IsAny<string>()));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }


        [Test]
        public async Task
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_but_command_execution_throws_an_exception_Then_process_should_be_finished_with_error()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var userManager = new Mock<TestHqUserManager>();
            userManager.Setup(x => x.CreateUserAsync(Moq.It.IsAny<HqUser>(), Moq.It.IsAny<string>(), Moq.It.IsAny<UserRoles>()))
                .Throws<NullReferenceException>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, identityManager: userManager.Object);

            await userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync();

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcessWithError(userPreloadingProcess.UserPreloadingProcessId, Moq.It.IsAny<string>()));
        }

        private UserBatchCreator CreateUserBatchCreator(
           IUserPreloadingService userPreloadingService = null,
           HqUserManager identityManager = null)
        {
            Setup.InstanceToMockedServiceLocator(identityManager ?? Mock.Of<TestHqUserManager>());

            return new UserBatchCreator(
                userPreloadingService ?? Mock.Of<IUserPreloadingService>(),
                Mock.Of<ILogger>());
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