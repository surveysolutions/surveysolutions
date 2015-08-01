using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable;
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

        private readonly IPlainTransactionManager plainTransactionManager;

        private readonly ITransactionManagerProvider transactionManagerProvider;

        private readonly IUserPreloadingService userPreloadingService;

        private  readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;

        public UserPreloadingVerifier(ITransactionManagerProvider transactionManagerProvider, 
            IUserPreloadingService userPreloadingService, 
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, 
            IPlainTransactionManager plainTransactionManager)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.userPreloadingService = userPreloadingService;
            this.userStorage = userStorage;
            this.plainTransactionManager = plainTransactionManager;
        }

        public void VerifyProcessFromReadyToBeVerifiedQueue()
        {
            string preloadingProcessIdToValidate = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                userPreloadingService.DeQueuePreloadingProcessIdReadyToBeValidated());

            if (string.IsNullOrEmpty(preloadingProcessIdToValidate))
                return;

            var preloadingProcessDataToValidate =
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToValidate)
                            .UserPrelodingData.ToList());

            this.ValidatePreloadedData(preloadingProcessDataToValidate,
                preloadingProcessIdToValidate);

            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => userPreloadingService.FinishValidationProcess(preloadingProcessIdToValidate));
        }

        private void ValidatePreloadedData(IList<UserPreloadingDataRecord> data, string processId)
        {
            var activeUserNames = TransactionManager.ExecuteInQueryTransaction(() =>

                userStorage.Query(
                    _ =>
                        _.Where(u => !u.IsArchived)
                            .Select(u => u.UserName.ToLower())).ToHashSet()
                );

            var activeSupervisorNames = TransactionManager.ExecuteInQueryTransaction(() =>

                userStorage.Query(
                    _ =>
                        _.Where(u => !u.IsArchived && u.Roles.Contains(UserRoles.Supervisor))
                            .Select(u => u.UserName.ToLower())).ToHashSet()
                );

            var archivedSupervisorNames = TransactionManager.ExecuteInQueryTransaction(() =>

                userStorage.Query(
                    _ =>
                        _.Where(u => u.IsArchived && u.Roles.Contains(UserRoles.Supervisor))
                            .Select(u => u.UserName.ToLower())).ToHashSet()
                );

            var archivedInterviewerNamesMappedOnSupervisorName = TransactionManager.ExecuteInQueryTransaction(() =>
                userStorage.Query(
                    _ =>
                        _.Where(u => u.IsArchived && u.Roles.Contains(UserRoles.Operator))
                            .Select(
                                u => new {u.UserName, SupervisorName = u.Supervisor == null ? "" : u.Supervisor.Name}))
                    .ToDictionary(u => u.UserName.ToLower(), u => u.SupervisorName.ToLower())
                );

            this.ValidateEachRowInDataSet(data, processId, (userPreloadingDataRecord) => LoginNameUsedByExistingUser(activeUserNames, userPreloadingDataRecord), "PLU0001", u => u.Login);
            this.ValidateEachRowInDataSet(data, processId, (userPreloadingDataRecord) => LoginDublicationInDataset(data, userPreloadingDataRecord), "PLU0002", u => u.Login);
            this.ValidateEachRowInDataSet(data, processId, (userPreloadingDataRecord) => LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(archivedInterviewerNamesMappedOnSupervisorName, userPreloadingDataRecord), "PLU0003", u => u.Login);
            this.ValidateEachRowInDataSet(data, processId, (userPreloadingDataRecord) => LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(archivedInterviewerNamesMappedOnSupervisorName, archivedSupervisorNames, userPreloadingDataRecord), "PLU0004", u => u.Login);
            this.ValidateEachRowInDataSet(data, processId, LoginFormatVerification, "PLU0005",  u => u.Login);
            this.ValidateEachRowInDataSet(data, processId, PasswordFormatVerification, "PLU0006", u => u.Password);
            this.ValidateEachRowInDataSet(data, processId, EmailFormatVerification, "PLU0007", u => u.Email);
            this.ValidateEachRowInDataSet(data, processId, PhoneNumberFormatVerification, "PLU0008",  u => u.PhoneNumber);
            this.ValidateEachRowInDataSet(data, processId, RoleVerification, "PLU0009", u => u.Role);
            this.ValidateEachRowInDataSet(data, processId, (userPreloadingDataRecord) => SupervisorVerification(data, activeSupervisorNames, userPreloadingDataRecord), "PLU0010", u => u.Supervisor);
            this.ValidateEachRowInDataSet(data, processId, SupervisorColumnMustBeEmptyForUserInSupervisorRole, "PLU0011",
                u => u.Supervisor);
        }

        private bool SupervisorColumnMustBeEmptyForUserInSupervisorRole(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            if (role != UserRoles.Supervisor)
                return false;

            return !string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor);
        }

        private bool PasswordFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Password))
                return true;

            if (userPreloadingDataRecord.Password.Length > 100)
                return true;

            var regExp = new Regex("^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$");
            return !regExp.IsMatch(userPreloadingDataRecord.Password);
        }

        private bool LoginFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var regExp = new Regex("^[a-zA-Z0-9_]{3,15}$");
            return !regExp.IsMatch(userPreloadingDataRecord.Login);
        }

        private bool EmailFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Email))
                return false;

            var regExp = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return !regExp.IsMatch(userPreloadingDataRecord.Email);
        }

        private bool PhoneNumberFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.PhoneNumber))
                return false;

            var regExp = new Regex(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return !regExp.IsMatch(userPreloadingDataRecord.PhoneNumber);
        }

        private bool LoginNameUsedByExistingUser(HashSet<string> activeUserNames, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return activeUserNames.Contains(userPreloadingDataRecord.Login.ToLower());
        }

        private bool LoginDublicationInDataset(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return data.Count(row => row.Login.ToLower() == userPreloadingDataRecord.Login) > 1;
        }

        bool LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(
            Dictionary<string, string> archivedInterviewerNamesMappedOnSupervisorName,
            UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var desiredRole = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            if (desiredRole != UserRoles.Operator)
                return false;

            if (!archivedInterviewerNamesMappedOnSupervisorName.ContainsKey(userPreloadingDataRecord.Login.ToLower()))
                return false;

            if (archivedInterviewerNamesMappedOnSupervisorName[userPreloadingDataRecord.Login.ToLower()] !=
                userPreloadingDataRecord.Supervisor.ToLower())
                return true;

            return false;
        }

        bool LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(
            Dictionary<string, string> archivedInterviewerNamesMappedOnSupervisorName,
            HashSet<string> archivedSupervisorNames, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var desiredRole = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);

            switch (desiredRole)
            {
                case UserRoles.Operator:
                    return archivedSupervisorNames.Contains(userPreloadingDataRecord.Login.ToLower());
                case UserRoles.Supervisor:
                    return
                        archivedInterviewerNamesMappedOnSupervisorName.ContainsKey(
                            userPreloadingDataRecord.Login.ToLower());
                default:
                    return false;
            }
        }

        private void ValidateEachRowInDataSet(
            IList<UserPreloadingDataRecord> data,
            string processId,
            Func<UserPreloadingDataRecord, bool> validation,
            string code,
            Expression<Func<UserPreloadingDataRecord, string>> cellFunc)
        {
            int rowNumber = 1;
            var columnName=   ((MemberExpression)cellFunc.Body).Member.Name;
            foreach (var userPreloadingDataRecord in data)
            {
                if (validation(userPreloadingDataRecord))
                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.PushVerificationError(processId, code,
                            rowNumber, columnName, cellFunc.Compile()(userPreloadingDataRecord)));

                rowNumber++;
            }
        }

        private bool RoleVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            return role == UserRoles.Undefined;
        }

        private bool SupervisorVerification(IList<UserPreloadingDataRecord> data,
            HashSet<string> activeSupervisors, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            var role = userPreloadingService.GetUserRoleFromDataRecord(userPreloadingDataRecord);
            if (role != UserRoles.Operator)
                return false;

            if (string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor))
                return true;

            var supervisorNameLowerCase = userPreloadingDataRecord.Supervisor.ToLower();
            if (activeSupervisors.Contains(supervisorNameLowerCase))
            {
                return false;
            }

            var supervisorsToPreload =
                data.Where(u => u.Login.ToLower() == supervisorNameLowerCase).ToArray();

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