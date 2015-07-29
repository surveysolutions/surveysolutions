using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserPreloadingVerifier : IUserPreloadingVerifier
    {
        ITransactionManager TransactionManager
        {
            get { return transactionManagerProvider.GetTransactionManager(); }
        }

        IPlainTransactionManager PlainTransactionManager
        {
            get { return ServiceLocator.Current.GetInstance<IPlainTransactionManager>(); }
        }

        private readonly ITransactionManagerProvider transactionManagerProvider;

        private readonly IUserPreloadingService userPreloadingService;

        private  readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;

        public UserPreloadingVerifier(ITransactionManagerProvider transactionManagerProvider, IUserPreloadingService userPreloadingService, IQueryableReadSideRepositoryReader<UserDocument> userStorage)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.userPreloadingService = userPreloadingService;
            this.userStorage = userStorage;
        }

        public void VerifyProcessFromReadyToBeVerifiedQueue()
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
                this.ValidatePreloadedData(preloadingProcessDataToValidate,
                    preloadingProcessIdToValidate);
            });

            PlainTransactionManager.ExecuteInPlainTransaction(
                () => userPreloadingService.FinishValidationProcess(preloadingProcessIdToValidate));
        }

        private void ValidatePreloadedData(IList<UserPreloadingDataRecord> data, string processId)
        {
            ValidateRow(data, processId, LoginNameTakenByExistingUser, "PLU0001", "Login", u => u.Login);
            ValidateRow(data, processId, LoginDublicationInDataset, "PLU0002", "Login", u => u.Login);
            ValidateRow(data, processId, LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam, "PLU0003", "Login", u => u.Login);
            ValidateRow(data, processId, LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole, "PLU0004", "Login", u => u.Login);
            ValidateRow(data, processId, LoginFormatVerification, "PLU0005", "Login", u => u.Login);
            ValidateRow(data, processId, PasswordFormatVerification, "PLU0006", "Password", u => u.Password);
            ValidateRow(data, processId, EmailFormatVerification, "PLU0007", "Email", u => u.Email);
            ValidateRow(data, processId, PhoneNumberFormatVerification, "PLU0008", "PhoneNumber", u => u.PhoneNumber);
            ValidateRow(data, processId, RoleVerification, "PLU0009", "Role", u => u.Role);
            ValidateRow(data, processId, SupervisorVerification, "PLU0010", "Supervisor", u => u.Supervisor);
            ValidateRow(data, processId, SupervisorColumnMustBeEmptyForUserInSupervisorRole, "PLU0011", "Supervisor", u => u.Supervisor);
        }

        private bool SupervisorColumnMustBeEmptyForUserInSupervisorRole(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            if (role != UserRoles.Supervisor)
                return false;

            return !string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor);
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
            var regExp = new Regex("^[a-zA-Z0-9_]{3,15}$");
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
            var desiredRole = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
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
            var desiredRole = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);

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

        private void ValidateRow(
            IList<UserPreloadingDataRecord> data,
            string processId,
            Func<IList<UserPreloadingDataRecord>, UserPreloadingDataRecord, bool> validation,
            string code,
            string columnName,
            Func<UserPreloadingDataRecord, string> cellFunc)
        {
            int rowNumber = 1;

            foreach (var userPreloadingDataRecord in data)
            {
                if (validation(data, userPreloadingDataRecord))
                    PlainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.PushVerificationError(processId, code,
                            rowNumber, columnName, cellFunc(userPreloadingDataRecord)));

                rowNumber++;
            }
        }

        private bool RoleVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            return role == UserRoles.Undefined;
        }

        private bool SupervisorVerification(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            if (role != UserRoles.Operator)
                return false;

            if (string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor))
                return true;

            var storedSupervisor =
                userStorage.Query(
                    _ => _.Where(u => u.UserName.ToLower() == userPreloadingDataRecord.Supervisor.ToLower()));

            if (storedSupervisor.Any(u => u.Roles.Contains(UserRoles.Supervisor)))
                return false;

            var supervisorsToPreload =
                data.Where(u => u.Login.ToLower() == userPreloadingDataRecord.Supervisor.ToLower()).ToArray();

            if (!supervisorsToPreload.Any())
                return true;

            foreach (var supervisorToPreload in supervisorsToPreload)
            {
                var possibleSupervisorRole = userPreloadingService.GetUserRoleFromDataRecord(supervisorToPreload);
                if (possibleSupervisorRole == UserRoles.Supervisor)
                    return false;
            }

            return true;
        }
    }
}