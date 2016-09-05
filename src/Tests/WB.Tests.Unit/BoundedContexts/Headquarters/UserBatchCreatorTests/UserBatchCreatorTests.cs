using System;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

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
            var commantService = new Mock<ICommandService>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commandService: commantService.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            commantService.Verify(
                x =>
                    x.Execute(Moq.It.Is<CreateUserCommand>(c => c.UserName == supervisorName && c.Roles.Contains(UserRoles.Supervisor)), Moq.It.IsAny<string>()));

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_supervisor_should_be_unarchived_and_updated()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var commantService = new Mock<ICommandService>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);
            var userStorage = new TestPlainStorage<UserDocument>();
            userStorage.Store(Create.Entity.UserDocument(userName: supervisorName, isArchived: true), "id");

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commandService: commantService.Object, userStorage: userStorage);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            commantService.Verify(x => x.Execute(Moq.It.IsAny<UnarchiveUserAndUpdateCommand>(), Moq.It.IsAny<string>()));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_Then_one_interviewer_should_be_created()
        {
            var interviewerName = "inter";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor:"tttt"));
            var commantService = new Mock<ICommandService>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commandService: commantService.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            commantService.Verify(
                x =>
                    x.Execute(Moq.It.Is<CreateUserCommand>(c => c.UserName == interviewerName && c.Roles.Contains(UserRoles.Interviewer)), Moq.It.IsAny<string>()));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }

        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_interviewer_is_present_in_the_dataset_and_the_user_in_present_in_the_system_as_archived_Then_one_interviewer_should_be_unarchived_and_updated()
        {
            var interviewerName = "inter";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor: "tttt"));
            var commantService = new Mock<ICommandService>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Interviewer);
            var userStorage = new TestPlainStorage<UserDocument>();
            userStorage.Store(Create.Entity.UserDocument(userName: interviewerName, isArchived: true, supervisorId:Guid.NewGuid()), "id");

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commandService: commantService.Object, userStorage: userStorage);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            commantService.Verify(x => x.Execute(Moq.It.IsAny<UnarchiveUserAndUpdateCommand>(), Moq.It.IsAny<string>()));
            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
        }


        [Test]
        public void
            CreateUsersFromReadyToBeCreatedQueue_When_one_user_in_role_supervisor_is_present_in_the_dataset_but_command_execution_throws_an_exception_Then_process_should_be_finished_with_error()
        {
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName));
            var commantService = new Mock<ICommandService>();
            commantService.Setup(x => x.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>()))
                .Throws<NullReferenceException>();
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userBatchCreator =
                CreateUserBatchCreator(userPreloadingServiceMock.Object, commandService: commantService.Object);

            userBatchCreator.CreateUsersFromReadyToBeCreatedQueue();

            userPreloadingServiceMock.Verify(x => x.FinishPreloadingProcessWithError(userPreloadingProcess.UserPreloadingProcessId, Moq.It.IsAny<string>()));
        }

        private UserBatchCreator CreateUserBatchCreator(
           IUserPreloadingService userPreloadingService = null,
           ICommandService commandService=null,
           IPlainStorageAccessor<UserDocument> userStorage = null)
        {
            return new UserBatchCreator(
                userPreloadingService ?? Mock.Of<IUserPreloadingService>(),
                commandService??Mock.Of<ICommandService>(),
                userStorage ?? Mock.Of<IPlainStorageAccessor<UserDocument>>(),
                Mock.Of<ILogger>(),
                Mock.Of<IPasswordHasher>());
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