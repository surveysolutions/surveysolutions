using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.UserPreloadingServiceTests
{
    [TestFixture]
    public class UserPreloadingServiceTests
    {
        [Test]
        public void
            CreateUserPreloadingProcess_When_DataReader_cant_be_create_Then_UserPreloadingException_should_be_thrown()
        {
            var recordsAccessorFactoryMock = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactoryMock.Setup(
                x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Throws<NullReferenceException>();

            var userPreloadingService =
                this.CreateUserPreloadingService(recordsAccessorFactory: recordsAccessorFactoryMock.Object);

            Assert.Catch<UserPreloadingException>(
                () => userPreloadingService.CreateUserPreloadingProcess(new MemoryStream(), "aaa"));
        }

        [Test]
        public void
            CreateUserPreloadingProcess_When_record_count_is_10K_Then_UserPreloadingException_should_be_thrown()
        {
            var recordsAccessorFactoryMock = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactoryMock.Setup(
                x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>())).Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[11111][]));

            var userPreloadingService =
                this.CreateUserPreloadingService(recordsAccessorFactory: recordsAccessorFactoryMock.Object);

            Assert.Catch<UserPreloadingException>(
                () => userPreloadingService.CreateUserPreloadingProcess(new MemoryStream(), "aaa"));
        }

        [Test]
        public void
            CreateUserPreloadingProcess_When_header_contains_invalid_columns_Then_UserPreloadingException_should_be_thrown()
        {
            var recordsAccessorFactoryMock = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactoryMock.Setup(
                x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>())).Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][]{new string[]{"nastya"} }));

            var userPreloadingService =
                this.CreateUserPreloadingService(recordsAccessorFactory: recordsAccessorFactoryMock.Object);

            Assert.Catch<UserPreloadingException>(
                () => userPreloadingService.CreateUserPreloadingProcess(new MemoryStream(), "aaa"));
        }

        [Test]
        public void
            CreateUserPreloadingProcess_When_data_format_is_valid_Then_preloading_process_should_be_stored()
        {
            var recordsAccessorFactoryMock = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactoryMock.Setup(
                x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][] {new[] {"Login"}, new[] {"nastya"}}));
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();

            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage, recordsAccessorFactory: recordsAccessorFactoryMock.Object);
            var fileName = "aaa";

            var processId = userPreloadingService.CreateUserPreloadingProcess(new MemoryStream(), fileName);

            Assert.That(userPreloadingProcessStorage.GetById(processId).FileName, Is.EqualTo(fileName));
        }

        [Test]
        public void
            FinishValidationProcess_When_process_is_in_validating_state_Then_validation_process_should_be_finished()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.Validating }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.FinishValidationProcess(processId);

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.Validated));
        }

        [Test]
        public void
            FinishValidationProcessWithError_When_process_is_in_validating_state_Then_validation_process_should_be_finished_with_error()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.Validating }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.FinishValidationProcessWithError(processId,"error");

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.ValidationFinishedWithError));
        }

        [Test]
        public void
            FinishPreloadingProcess_When_process_is_in_creating_users_state_Then_process_should_be_finished()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.CreatingUsers }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.FinishPreloadingProcess(processId);

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.Finished));
        }

        [Test]
        public void
            FinishPreloadingProcessWithError_When_process_is_in_creating_users_state_Then_process_should_be_finished_with_error()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.CreatingUsers }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.FinishPreloadingProcessWithError(processId, "error");

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.FinishedWithError));
        }


        [Test]
        public void
            EnqueueForValidation_When_process_is_in_Uploaded_state_Then_process_should_be_ready_for_validation()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.Uploaded }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.EnqueueForValidation(processId);

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.ReadyForValidation));
        }

        [Test]
        public void
            EnqueueForUserCreation_When_process_is_in_Validated_state_Then_process_should_be_ready_for_user_creation()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.Validated }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.EnqueueForUserCreation(processId);

            Assert.That(userPreloadingProcessStorage.GetById(processId).State, Is.EqualTo(UserPrelodingState.ReadyForUserCreation));
        }

        [Test]
        public void
            DeletePreloadingProcess_When_process_is_in_Validated_state_Then_process_should_be_deleted()
        {
            var processId = "aaa";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId, State = UserPrelodingState.Validated }, processId);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            userPreloadingService.DeletePreloadingProcess(processId);

            Assert.That(userPreloadingProcessStorage.GetById(processId), Is.Null);
        }

        [Test]
        public void
            DeQueuePreloadingProcessIdReadyToBeValidated_When_2_processes_in_ready_to_be_validated_state_are_present_Then_oldest_process_id_should_be_returned()
        {
            var processId1 = "aaa";
            var processId2 = "bbb";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId1, State = UserPrelodingState.ReadyForValidation, LastUpdateDate = DateTime.Now }, processId1);
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId2, State = UserPrelodingState.ReadyForValidation, LastUpdateDate = DateTime.Now }, processId2);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            var result = userPreloadingService.DeQueuePreloadingProcessIdReadyToBeValidated();

            Assert.That(result, Is.EqualTo(processId1));
        }

        [Test]
        public void
            DeQueuePreloadingProcessIdReadyToCreateUsers_When_2_processes_in_ReadyForUserCreation_state_are_present_Then_oldest_process_id_should_be_returned()
        {
            var processId1 = "aaa";
            var processId2 = "bbb";
            var userPreloadingProcessStorage = new TestPlainStorage<UserPreloadingProcess>();
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId1, State = UserPrelodingState.ReadyForUserCreation, LastUpdateDate = DateTime.Now }, processId1);
            userPreloadingProcessStorage.Store(new UserPreloadingProcess() { UserPreloadingProcessId = processId2, State = UserPrelodingState.ReadyForUserCreation, LastUpdateDate = DateTime.Now }, processId2);
            var userPreloadingService =
                this.CreateUserPreloadingService(userPreloadingProcessStorage: userPreloadingProcessStorage);

            var result = userPreloadingService.DeQueuePreloadingProcessIdReadyToCreateUsers();

            Assert.That(result, Is.EqualTo(processId1));
        }

        private UserPreloadingService CreateUserPreloadingService(IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage=null,
            IRecordsAccessorFactory recordsAccessorFactory = null)
        {
            return new UserPreloadingService(userPreloadingProcessStorage?? new InMemoryPlainStorageAccessor<UserPreloadingProcess>(),
                recordsAccessorFactory ?? Mock.Of<IRecordsAccessorFactory>(),
                new UserPreloadingSettings(5, 5, 12, 1, 10000, 100, 100));
        }
    }
}