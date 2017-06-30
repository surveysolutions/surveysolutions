using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.UserPreloadingVerifierTests
{
    [TestFixture]
    internal class UserPreloadingVerifierTests
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
            VerifyProcessFromReadyToBeVerifiedQueue_When_login_is_taken_by_existing_user_Then_record_verification_error_with_code_PLU0001()
        {
            var userName = "nastya";
            var userStorage = Create.Storage.UserRepository(Create.Entity.HqUser(userName: userName));
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(userName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object, userStorage: userStorage);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0001", 1, "Login", userName));
            userPreloadingServiceMock.Verify(x => x.UpdateVerificationProgressInPercents(userPreloadingProcess.UserPreloadingProcessId, Moq.It.IsAny<int>()));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_2_users_with_the_same_login_are_present_in_the_dataset_Then_record_verification_error_with_code_PLU0002()
        {
            var userName = "nastya";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: new []
            {Create.Entity.UserPreloadingDataRecord(userName), Create.Entity.UserPreloadingDataRecord(userName)});
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0002", 1, "Login", userName));
            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0002", 2, "Login", userName));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_login_is_taken_by_archived_interviewer_in_other_team_Then_record_verification_error_with_code_PLU0003()
        {
            var userName = "nastya";
            var supervisorName = "super";
            var userStorage = Create.Storage.UserRepository(Create.Entity.HqUser(userName: userName, supervisorId: Guid.NewGuid(), isArchived: true),
                Create.Entity.HqUser(userName: supervisorName));
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(login: userName, supervisor: supervisorName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object, userStorage: userStorage);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0003", 1, "Login", userName));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_login_is_taken_by_user_in_other_role_Then_record_verification_error_with_code_PLU0004()
        {
            var userName = "nastya";
            var userStorage = Create.Storage.UserRepository(Create.Entity.HqUser(userName: userName, isArchived: true, role: UserRoles.Supervisor));
            
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(userName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object, userStorage: userStorage);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0004", 1, "Login", userName));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_login_contains_invalid_characted_Then_record_verification_error_with_code_PLU0005()
        {
            var userName = "na$tya";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(userName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0005", 1, "Login", userName));
        }

        [TestCase("")]//empty
        [TestCase("Q11w")]//less 10 
        [TestCase("QqQqQqQqQqQqQq")]//regexp
        [TestCase("A1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890")]//more 100
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_password_is_empty_Then_record_verification_error_with_code_PLU0006(string password)
        {
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(password: password));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0006", 1, "Password", password));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_password_is_short_Then_record_verification_error_with_code_PLU0006()
        {
            var shortPassword = "Q11w";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(password: shortPassword));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0006", 1, "Password", shortPassword));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_email_contains_invalid_characted_Then_record_verification_error_with_code_PLU0007()
        {
            var email = "na$tya";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(email: email));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0007", 1, "Email", email));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_phone_number_contains_invalid_characted_Then_record_verification_error_with_code_PLU0008()
        {
            var phoneNumber = "na$tya";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(phoneNumber: phoneNumber));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0008", 1, "PhoneNumber", phoneNumber));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_users_role_is_undefined_Then_record_verification_error_with_code_PLU0009()
        {
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord());
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, role: 0);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0009", 1, "Role", "supervisor"));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_user_in_role_interviewer_has_supervisor_in_role_supervisor_Then_record_verification_error_with_code_PLU0010()
        {
            var interviewerName = "int";
            var supervisorName = "super";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:new []
            {
                Create.Entity.UserPreloadingDataRecord(login: interviewerName, supervisor: supervisorName),
                Create.Entity.UserPreloadingDataRecord(login: supervisorName, supervisor: interviewerName)
            }
        );

            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0010", 1, "Supervisor", supervisorName));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_user_in_role_supervisor_has_not_empty_supervisor_column_Then_record_verification_error_with_code_PLU0011()
        {
            var supervisorName = "super";
            var supervisorCellValue = "super_test";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords:
                Create.Entity.UserPreloadingDataRecord(login: supervisorName, supervisor: supervisorCellValue));

            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess, UserRoles.Supervisor);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0011", 1, "Supervisor", supervisorCellValue));
        }

        [Test]
        public void
            VerifyProcessFromReadyToBeVerifiedQueue_When_exception_happend_during_verification_Then_verification_should_be_finished_with_error()
        {
            var userName = "nastya";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(userName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);
            userPreloadingServiceMock.Setup(
                x =>
                    x.UpdateVerificationProgressInPercents(userPreloadingProcess.UserPreloadingProcessId,
                        Moq.It.IsAny<int>())).Throws<NullReferenceException>();

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.FinishValidationProcessWithError(userPreloadingProcess.UserPreloadingProcessId, Moq.It.IsAny<string>()));
        }

        [Test]
        public void when_person_full_name_has_more_than_allowed_length_Should_return_error()
        {
            var fullName = new string('a', 101);
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(fullName: fullName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0012", 1, "FullName", fullName));
        }

        [Test]
        public void when_person_full_name_has_illigal_characters_Should_return_error()
        {
            var fullName = "Имя 123";
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(fullName: fullName));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0014", 1, "FullName", fullName));
        }

        [Test]
        public void when_phone_number_more_than_allowed_length_Should_return_error()
        {
            var phone = new string('1', 16);
            var userPreloadingProcess = Create.Entity.UserPreloadingProcess(dataRecords: Create.Entity.UserPreloadingDataRecord(phoneNumber: phone));
            var userPreloadingServiceMock = CreateUserPreloadingServiceMock(userPreloadingProcess);

            var userPreloadingVerifier =
                CreateUserPreloadingVerifier(userPreloadingService: userPreloadingServiceMock.Object);

            userPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();

            userPreloadingServiceMock.Verify(x => x.PushVerificationError(userPreloadingProcess.UserPreloadingProcessId, "PLU0013", 1, "PhoneNumber", phone));
        }

        private UserPreloadingVerifier CreateUserPreloadingVerifier(
            IUserPreloadingService userPreloadingService = null,
            IUserRepository userStorage = null)
        {
            return
                new UserPreloadingVerifier(
                    userPreloadingService ?? Mock.Of<IUserPreloadingService>(),
                    userStorage ?? Mock.Of<IUserRepository>(),
                    Create.Entity.UserPreloadingSettings(), Mock.Of<ILogger>());
        }

        private Mock<IUserPreloadingService> CreateUserPreloadingServiceMock(UserPreloadingProcess userPreloadingProcess, UserRoles role = UserRoles.Interviewer)
        {
            var UserPreloadingProcessIdQueue = new Queue<string>();
            UserPreloadingProcessIdQueue.Enqueue(userPreloadingProcess.UserPreloadingProcessId);
            UserPreloadingProcessIdQueue.Enqueue(null);

            var userPreloadingServiceMock = new Mock<IUserPreloadingService>();
            userPreloadingServiceMock.Setup(x => x.DeQueuePreloadingProcessIdReadyToBeValidated()).Returns(UserPreloadingProcessIdQueue.Dequeue);
            userPreloadingServiceMock.Setup(x => x.GetPreloadingProcesseDetails(userPreloadingProcess.UserPreloadingProcessId))
                .Returns(userPreloadingProcess);

            userPreloadingServiceMock.Setup(x => x.GetUserRoleFromDataRecord(Moq.It.IsAny<UserPreloadingDataRecord>()))
                .Returns(role);

            return userPreloadingServiceMock;
        }
    }

}