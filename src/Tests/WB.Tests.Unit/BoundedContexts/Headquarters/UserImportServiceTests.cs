using System;
using System.IO;
using System.Linq;
using System.Text;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestFixture]
    internal class UserImportServiceTests
    {
        [Test]
        public void When_verification_errors_more_than_max_allowed_errors_Then_UserPreloadingException_should_be_thrown()
        {
            //arrange
            var usersToImport = new UserToImport[10001];
            for (int i = 0; i <= 10000; i++)
                usersToImport[i] = Create.Entity.UserToImport("");

            var userImportService = CreateUserImportService(null, usersToImport);

            //act
            var exception = Assert.Catch<PreloadingException>(() => userImportService
                .VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray());

            //assert
            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void When_missing_required_columns_Then_UserPreloadingException_should_be_thrown()
        {
            //arrange
            var csvReader = Mock.Of<ICsvReader>(x => x.ReadHeader(It.IsAny<Stream>(), It.IsAny<string>()) == new string[0]);

            var userImportService = Create.Service.UserImportService(csvReader: csvReader);

            //act
            var exception = Assert.Catch<PreloadingException>(() => userImportService
                .VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray());

            //assert
            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void When_job_by_previous_import_is_not_completed_Then_UserPreloadingException_should_be_thrown()
        {
            //arrange
            var scheduler = Mock.Of<IScheduler>(x =>
                x.GetCurrentlyExecutingJobs() == new[]
                {
                    Mock.Of<IJobExecutionContext>(y =>
                        y.JobDetail ==
                        Mock.Of<IJobDetail>(z => z.Key == new JobKey("Import users job", "Import users")))
                });

            var usersImportTask = new UsersImportTask(scheduler);

            var userImportService = CreateUserImportServiceWithRepositories(usersImportTask: usersImportTask);

            //act
            var exception = Assert.Catch<PreloadingException>(() => userImportService
                .VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray());

            //assert
            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void GetAvaliableDataColumnNames_Then_list_of_expected_columns_is_returned()
        {
            var userImportService = CreateUserImportService(null);

            var columnList = userImportService.GetUserProperties();

            Assert.That(columnList,
                Is.EqualTo(new[] {"login", "password", "role", "supervisor", "fullname", "email", "phonenumber"}));
        }

        [Test]
        public void When_login_is_taken_by_existing_user_Then_record_verification_error_with_code_PLU0001()
        {
            //arrange
            var userName = "nastya";

            var userImportService = CreateUserImportService(
                new[] {Create.Entity.HqUser(userName: userName)},
                Create.Entity.UserToImport(userName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0001", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Login", errors[0].ColumnName);
            Assert.AreEqual(userName, errors[0].CellValue);
        }

        [Test]
        public void When_2_users_with_the_same_login_are_present_in_the_dataset_Then_record_verification_error_with_code_PLU0002()
        {
            //arrange
            var userName = "nastya";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(userName), Create.Entity.UserToImport(userName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(2, errors.Length);
            Assert.AreEqual("PLU0002", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Login", errors[0].ColumnName);
            Assert.AreEqual(userName, errors[0].CellValue);

            Assert.AreEqual("PLU0002", errors[1].Code);
            Assert.AreEqual(3, errors[1].RowNumber);
            Assert.AreEqual("Login", errors[1].ColumnName);
            Assert.AreEqual(userName, errors[1].CellValue);
        }

        [Test]
        public void When_login_is_taken_by_archived_interviewer_in_other_team_Then_record_verification_error_with_code_PLU0003()
        {
            //arrange
            var userName = "nastya";
            var supervisorName = "super";

            var userImportService = CreateUserImportService(
                new[]
                {
                    Create.Entity.HqUser(userName: userName, supervisorId: Guid.NewGuid(), isArchived: true),
                    Create.Entity.HqUser(userName: supervisorName, role: UserRoles.Supervisor)
                },
                Create.Entity.UserToImport(login: userName, supervisor: supervisorName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0003", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Login", errors[0].ColumnName);
            Assert.AreEqual(userName, errors[0].CellValue);
        }

        [Test]
        public void When_login_is_taken_by_user_in_other_role_Then_record_verification_error_with_code_PLU0004()
        {
            //arrange
            var userName = "nastya";

            var userImportService = CreateUserImportService(
                new[]
                {
                    Create.Entity.HqUser(userName: userName, isArchived: true)
                },
                Create.Entity.UserToImport(userName, role: "supervisor"));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0004", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Login", errors[0].ColumnName);
            Assert.AreEqual(userName, errors[0].CellValue);
        }

        [Test]
        public void When_users_login_contains_invalid_characted_Then_record_verification_error_with_code_PLU0005()
        {
            //arrange
            var userName = "na$tya";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(userName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0005", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Login", errors[0].ColumnName);
            Assert.AreEqual(userName, errors[0].CellValue);
        }

        [TestCase("")] //empty
        [TestCase("Q11w")] //less 10 
        [TestCase("QqQqQqQqQqQqQq")] //regexp
        [TestCase("A1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890a1234567890")] //more 100
        public void When_users_password_is_empty_Then_record_verification_error_with_code_PLU0006(string password)
        {
            //arrange
            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(password: password));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0006", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Password", errors[0].ColumnName);
            Assert.AreEqual(password, errors[0].CellValue);
        }

        [Test]
        public void When_users_email_contains_invalid_characted_Then_record_verification_error_with_code_PLU0007()
        {
            //arrange
            var email = "na$tya";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(email: email));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0007", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Email", errors[0].ColumnName);
            Assert.AreEqual(email, errors[0].CellValue);
        }

        [Test]
        public void When_users_phone_number_contains_invalid_characted_Then_record_verification_error_with_code_PLU0008()
        {
            //arrange
            var phoneNumber = "na$tya";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(phoneNumber: phoneNumber));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0008", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("PhoneNumber", errors[0].ColumnName);
            Assert.AreEqual(phoneNumber, errors[0].CellValue);
        }

        [Test]
        public void When_users_role_is_undefined_Then_record_verification_error_with_code_PLU0009()
        {
            //arrange
            var undefinedRole = "undefined role";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(role: undefinedRole));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0009", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Role", errors[0].ColumnName);
            Assert.AreEqual(undefinedRole, errors[0].CellValue);
        }

        [Test]
        public void When_user_in_role_interviewer_and_supervisor_not_found_Then_record_verification_error_with_code_PLU0010()
        {
            //arrange
            var interviewerName = "int";
            var supervisorName = "super";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(login: interviewerName, supervisor: supervisorName, role: "interviewer"));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0010", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Supervisor", errors[0].ColumnName);
            Assert.AreEqual(supervisorName, errors[0].CellValue);
        }

        [Test]
        public void When_user_in_role_interviewer_and_supervisor_is_empty_Then_record_verification_error_with_code_PLU0010()
        {
            //arrange
            var interviewerName = "int";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(login: interviewerName, role: "interviewer"));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0010", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Supervisor", errors[0].ColumnName);
            Assert.AreEqual("", errors[0].CellValue);
        }

        [Test]
        public void When_user_in_role_supervisor_has_not_empty_supervisor_column_Then_record_verification_error_with_code_PLU0011()
        {
            //arrange
            var supervisorName = "super";
            var supervisorCellValue = "super_test";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(login: supervisorName, supervisor: supervisorCellValue, role: "supervisor"));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0011", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("Supervisor", errors[0].ColumnName);
            Assert.AreEqual(supervisorCellValue, errors[0].CellValue);
        }

        [Test]
        public void when_person_full_name_has_more_than_allowed_length_Should_return_error()
        {
            //arrange
            var fullName = new string('a', 101);

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(fullName: fullName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0012", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("FullName", errors[0].ColumnName);
            Assert.AreEqual(fullName, errors[0].CellValue);
        }

        [Test]
        public void when_phone_number_more_than_allowed_length_Should_return_error()
        {
            //arrange
            var phone = new string('1', 16);

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(phoneNumber: phone));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0013", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("PhoneNumber", errors[0].ColumnName);
            Assert.AreEqual(phone, errors[0].CellValue);
        }

        [Test]
        public void when_person_full_name_has_illigal_characters_Should_return_error()
        {
            //arrange
            var fullName = "Имя 123";

            var userImportService = CreateUserImportService(null,
                Create.Entity.UserToImport(fullName: fullName));

            //act
            var errors = userImportService.VerifyAndSaveIfNoErrors(new byte[0], "file.txt").ToArray();

            //assert
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("PLU0014", errors[0].Code);
            Assert.AreEqual(2, errors[0].RowNumber);
            Assert.AreEqual("FullName", errors[0].ColumnName);
            Assert.AreEqual(fullName, errors[0].CellValue);
        }

        [Test]
        public void when_uploaded_file_contains_quot()
        {
            string data = @"login	password	email	fullname	phonenumber	role	supervisor
            LmdYkeTihXA	P@$$w0rd	mytest@email.com	bPVEbCTaOiR""jZNdZgAAHUMcGOVNBFI	112233	supervisor";

            var service = Create.Service.UserImportService(csvReader: new CsvReader());

            // Act
            TestDelegate act = () => service.VerifyAndSaveIfNoErrors(Encoding.UTF8.GetBytes(data), "file.txt").ToList();

            // Assert
            Assert.DoesNotThrow(act); 
        }

        private UserImportService CreateUserImportService(HqUser[] dbUsers = null, params UserToImport[] usersToImport)
            => this.CreateUserImportServiceWithRepositories(dbUsers: dbUsers, usersToImport: usersToImport);

        private UserImportService CreateUserImportServiceWithRepositories(
            IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository = null,
            IPlainStorageAccessor<UserToImport> importUsersRepository = null, IAuthorizedUser authorizedUser = null,
            HqUser[] dbUsers = null, UsersImportTask usersImportTask = null, params UserToImport[] usersToImport)
        {
            var csvReader = Create.Service.CsvReader(new[]
            {
                nameof(UserToImport.Login), nameof(UserToImport.Password),
                nameof(UserToImport.Role), nameof(UserToImport.Supervisor),
                nameof(UserToImport.FullName), nameof(UserToImport.Email),
                nameof(UserToImport.PhoneNumber)
            }.Select(x => x.ToLower()).ToArray(), usersToImport);

            var userStorage = Create.Storage.UserRepository(dbUsers ?? new HqUser[0]);

            return Create.Service.UserImportService(csvReader: csvReader, userStorage: userStorage, usersImportTask: usersImportTask);
        }
    }
}
