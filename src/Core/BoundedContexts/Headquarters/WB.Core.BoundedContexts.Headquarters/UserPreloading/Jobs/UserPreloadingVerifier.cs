using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingVerifier : IJob
    {
        private readonly IUserPreloadingService userPreloadingService;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;

        private  readonly IPlainTransactionManager PlainTransactionManager;

        ITransactionManager TransactionManager
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManager>(); }
        }


        public UserPreloadingVerifier(IUserPreloadingService userPreloadingService, 
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, IPlainTransactionManager plainTransactionManager)
        {
            this.userPreloadingService = userPreloadingService;
            this.userStorage = userStorage;
            this.PlainTransactionManager = plainTransactionManager;
        }

        public void Execute(IJobExecutionContext context)
        {
            string preloadingProcessIdToValidate = PlainTransactionManager.ExecuteInPlainTransaction(() =>
                userPreloadingService.DeQueuePreloadingProcessIdReadyToBeValidated());

            if (string.IsNullOrEmpty(preloadingProcessIdToValidate))
                return;

            var preloadingProcessDataToValidate =
                PlainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToValidate)
                            .UserPrelodingData.ToList());

            TransactionManager.ExecuteInQueryTransaction(() =>
            {
                this.ValidatePreloadedData(preloadingProcessDataToValidate, preloadingProcessIdToValidate);
            });

            PlainTransactionManager.ExecuteInPlainTransaction(
                () => userPreloadingService.FinishValidationProcess(preloadingProcessIdToValidate));
        }

        void ValidatePreloadedData(IList<UserPreloadingDataRecord> data, string processId)
        {

            ValidateRow(data, processId, LoginNameTakenByExistingUser, "PLU0001",
                "Login is taken by an existing user", "Login");
            ValidateRow(data, processId, LoginDublicationInDataset, "PLU0002", "Login duplication in the file",
                "Login");
            ValidateRow(data, processId, LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam, "PLU0003",
                "Login of archive user can't be reused because it belongs to other team", "Login");
            ValidateRow(data, processId, LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole, "PLU0004",
                "Login of archive user can't be reused because it exists in other role", "Login");
            ValidateRow(data, processId, LoginFormatVerification, "PLU0005",
                "Login needs to be between 3 and 15 characters and contains only letters, digits and underscore symbol",
                "Login");
            ValidateRow(data, processId, PasswordFormatVerification, "PLU0006",
                "Password must contain at least one number, one upper case character and one lower case character",
                "Password");
            ValidateRow(data, processId, EmailFormatVerification, "PLU0007", "Email is invalid", "Email");
            ValidateRow(data, processId, PhoneNumberFormatVerification, "PLU0008", "Phone number is invalid",
                "PhoneNumber");
            ValidateRow(data, processId, RoleVerification, "PLU0009",
                "Role is invalid. 'Supervisor' or 'Interviewer' is valid values.", "Role");
            ValidateRow(data, processId, SupervisorVerification, "PLU0010",
                "Supervisor doesn't exist in the file or in the existing teams", "Supervisor");
        }

        private bool PasswordFormatVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Password))
                return true;

            if (userPreloadingDataRecord.Password.Length > 100)
                return true;

            var regExp = new Regex("^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$");
            return !regExp.IsMatch(userPreloadingDataRecord.Password);
        }

        private bool LoginFormatVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var regExp=new Regex("^[a-zA-Z0-9_]{3,15}$");
            return !regExp.IsMatch(userPreloadingDataRecord.Login);
        }

        private bool EmailFormatVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Email))
                return false;

            var regExp = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return !regExp.IsMatch(userPreloadingDataRecord.Email);
        }

        private bool PhoneNumberFormatVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.PhoneNumber))
                return false;

            var regExp = new Regex(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return !regExp.IsMatch(userPreloadingDataRecord.PhoneNumber);
        }

        private bool LoginNameTakenByExistingUser(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return
                    userStorage.Query(
                        _ =>
                            _.Where(
                                u => !u.IsArchived && u.UserName.ToLower() == userPreloadingDataRecord.Login.ToLower()))
                        .Any();
        }

        private bool LoginDublicationInDataset(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return data.Count(row => row.Login.ToLower() == userPreloadingDataRecord.Login) > 1;
        }

        private bool LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(IList<UserPreloadingDataRecord> data,
            UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var desiredRole = ParseUserRole(userPreloadingDataRecord.Role);
            if (desiredRole != UserRoles.Operator)
                return false;

            var archivedUsersWithTheSameLogin = userStorage.Query(
                _ =>
                    _.Where(
                        u =>
                            u.Supervisor != null && u.IsArchived &&
                            u.UserName.ToLower() == userPreloadingDataRecord.Login.ToLower()));

            if (!archivedUsersWithTheSameLogin.Any())
                return false;

            foreach (var userDocument in archivedUsersWithTheSameLogin)
            {
                if (userDocument.Supervisor.Name.ToLower() != userPreloadingDataRecord.Supervisor.ToLower())
                    return true;
            }

            return false;
        }

        private bool LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var desiredRole = ParseUserRole(userPreloadingDataRecord.Role);

            var archivedUsersWithTheSameLogin = userStorage.Query(
                _ =>
                    _.Where(
                        u => u.IsArchived &&
                            u.UserName.ToLower() == userPreloadingDataRecord.Login.ToLower()));

            if (!archivedUsersWithTheSameLogin.Any())
                return false;

            foreach (var userDocument in archivedUsersWithTheSameLogin)
            {
                if (!userDocument.Roles.Contains(desiredRole))
                    return true;
            }

            return false;
        }

        private void ValidateRow(IList<UserPreloadingDataRecord> data, string processId, Func<IList<UserPreloadingDataRecord>,UserPreloadingDataRecord, bool> validation, string code, string message, string columnName)
        {
            int rowNumber = 1;
            foreach (var userPreloadingDataRecord in data)
            {
                if (validation(data, userPreloadingDataRecord))
                        userPreloadingService.PushVerificationError(processId, code,
                            message,
                            rowNumber, columnName, userPreloadingDataRecord.Login);

                rowNumber++;
            }
        }

        private bool RoleVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = ParseUserRole(userPreloadingDataRecord.Role);
            return role == UserRoles.Undefined;
        }

        private bool SupervisorVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = ParseUserRole(userPreloadingDataRecord.Role);
            if (role != UserRoles.Operator)
                return false;

            if (string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor))
                return true;

            var storedSupervisor =
                userStorage.Query(
                    _ => _.Where(u => u.UserName.ToLower() == userPreloadingDataRecord.Supervisor.ToLower()));

            if (storedSupervisor.Any())
                return false;

            var supervisorsToPreload =
                data.Where(u => u.Login.ToLower() == userPreloadingDataRecord.Supervisor.ToLower()).ToArray();
            
            if (!supervisorsToPreload.Any())
                return true;

            foreach (var supervisorToPreload in supervisorsToPreload)
            {
                var possibleSupervisorRole = ParseUserRole(supervisorToPreload.Role);
                if (possibleSupervisorRole == UserRoles.Supervisor)
                    return false;
            }

            return true;
        }

        private UserRoles ParseUserRole(string role)
        {
            if (role.ToLower() == "supervisor")
                return UserRoles.Supervisor;
            if (role.ToLower() == "interviewer")
                return UserRoles.Operator;

            return UserRoles.Undefined;
        }
    }
}