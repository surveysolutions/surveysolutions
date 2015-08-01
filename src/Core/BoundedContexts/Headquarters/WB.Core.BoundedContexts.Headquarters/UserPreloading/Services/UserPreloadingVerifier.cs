using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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

        private readonly UserPreloadingSettings userPreloadingSettings;

        private readonly ILogger logger;

        public UserPreloadingVerifier(ITransactionManagerProvider transactionManagerProvider, 
            IUserPreloadingService userPreloadingService, 
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, 
            IPlainTransactionManager plainTransactionManager, 
            UserPreloadingSettings userPreloadingSettings, 
            ILogger logger)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.userPreloadingService = userPreloadingService;
            this.userStorage = userStorage;
            this.plainTransactionManager = plainTransactionManager;
            this.userPreloadingSettings = userPreloadingSettings;
            this.logger = logger;
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
            try
            {
                this.ValidatePreloadedData(preloadingProcessDataToValidate,
                    preloadingProcessIdToValidate);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.FinishValidationProcessWithError(preloadingProcessIdToValidate, e.Message));
                return;
                
            }
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

            var validationFunctions = new[]
            {
                new PreloadedDataValidator(row => LoginNameUsedByExistingUser(activeUserNames, row),"PLU0001", u => u.Login),
                new PreloadedDataValidator(row => LoginDublicationInDataset(data, row), "PLU0002",u => u.Login),
                new PreloadedDataValidator(row =>LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(archivedInterviewerNamesMappedOnSupervisorName, row), "PLU0003",u => u.Login),
                new PreloadedDataValidator(row =>LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(archivedInterviewerNamesMappedOnSupervisorName, archivedSupervisorNames,row), "PLU0004", u => u.Login),
                new PreloadedDataValidator(LoginFormatVerification, "PLU0005", u => u.Login),
                new PreloadedDataValidator(PasswordFormatVerification, "PLU0006", u => u.Password),
                new PreloadedDataValidator(EmailFormatVerification, "PLU0007", u => u.Email),
                new PreloadedDataValidator(EmailFormatVerification, "PLU0007", u => u.Email),
                new PreloadedDataValidator(PhoneNumberFormatVerification, "PLU0008", u => u.PhoneNumber),
                new PreloadedDataValidator(RoleVerification, "PLU0009", u => u.Role),
                new PreloadedDataValidator(row =>SupervisorVerification(data, activeSupervisorNames, row), "PLU0010",u => u.Supervisor),
                new PreloadedDataValidator(SupervisorColumnMustBeEmptyForUserInSupervisorRole, "PLU0011",u => u.Supervisor),
            };

            for (int i = 0; i < validationFunctions.Length; i++)
            {
                this.ValidateEachRowInDataSet(data, processId, validationFunctions[i], i, validationFunctions.Length);
            }
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

        void ValidateEachRowInDataSet(
            IList<UserPreloadingDataRecord> data,
            string processId,
            PreloadedDataValidator validator,
            int indexOfCurrentVerification,
            int countOfVerifications)
        {
            int rowNumber = 1;
            var columnName = ((MemberExpression)validator.ValueSelector.Body).Member.Name;
            foreach (var userPreloadingDataRecord in data)
            {
                if (validator.ValidationFunction(userPreloadingDataRecord))
                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.PushVerificationError(processId, validator.Code,
                            rowNumber, columnName, validator.ValueSelector.Compile()(userPreloadingDataRecord)));

                rowNumber++;
            }
            int percents = (int)((((double) (indexOfCurrentVerification + 1))/countOfVerifications)*100);

            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => userPreloadingService.UpdateVerificationProgressInPercents(processId, percents));
        }

        class PreloadedDataValidator
        {
            public PreloadedDataValidator(
                Func<UserPreloadingDataRecord, bool> validationFunction, 
                string code, 
                Expression<Func<UserPreloadingDataRecord, string>> valueSelector)
            {
                this.ValidationFunction = validationFunction;
                this.Code = code;
                this.ValueSelector = valueSelector;
            }

            public Func<UserPreloadingDataRecord, bool> ValidationFunction { get; private set; }
            public string Code { get; private set; }
            public Expression<Func<UserPreloadingDataRecord, string>> ValueSelector { get; private set; }
        }
    }
}