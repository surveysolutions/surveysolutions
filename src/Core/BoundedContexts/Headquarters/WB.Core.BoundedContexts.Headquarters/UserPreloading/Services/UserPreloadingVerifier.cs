﻿using System;
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
        private bool IsWorking = false; //please use singleton injection

        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

        private readonly IUserPreloadingService userPreloadingService;

        private  readonly IPlainStorageAccessor<UserDocument> userStorage;

        private readonly UserPreloadingSettings userPreloadingSettings;

        private readonly ILogger logger;

        readonly Regex passwordValidationRegex;
        readonly Regex loginValidatioRegex;
        readonly Regex emailValidationRegex;
        readonly Regex phoneNumberValidationRegex;

        public UserPreloadingVerifier(
            IUserPreloadingService userPreloadingService,
            IPlainStorageAccessor<UserDocument> userStorage, 
            UserPreloadingSettings userPreloadingSettings, 
            ILogger logger)
        {
            this.userPreloadingService = userPreloadingService;
            this.userStorage = userStorage;
            this.userPreloadingSettings = userPreloadingSettings;
            this.logger = logger;
            this.passwordValidationRegex = new Regex(this.userPreloadingSettings.PasswordFormatRegex);
            this.loginValidatioRegex = new Regex(this.userPreloadingSettings.LoginFormatRegex);
            this.emailValidationRegex = new Regex(this.userPreloadingSettings.EmailFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            this.phoneNumberValidationRegex = new Regex(this.userPreloadingSettings.PhoneNumberFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

        public void VerifyProcessFromReadyToBeVerifiedQueue()
        {
            if (IsWorking)
                return;

            IsWorking = true;
            try
            {
                while (IsWorking)
                {
                    string preloadingProcessIdToValidate = this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.DeQueuePreloadingProcessIdReadyToBeValidated());

                    if (string.IsNullOrEmpty(preloadingProcessIdToValidate))
                        return;

                    var preloadingProcessDataToValidate = this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToValidate).UserPrelodingData.ToList());
                    try
                    {
                        this.ValidatePreloadedData(preloadingProcessDataToValidate, preloadingProcessIdToValidate);
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
            }
            finally
            {
                IsWorking = false;
            }


        }

        private void ValidatePreloadedData(IList<UserPreloadingDataRecord> data, string processId)
        {
            var activeUserNames = plainTransactionManager.ExecuteInPlainTransaction(() =>

            userStorage.Query(_ => _.Where(u => !u.IsArchived).Select(u => u.UserName.ToLower())).ToHashSet());

            var activeSupervisorNames = plainTransactionManager.ExecuteInPlainTransaction(() =>

                userStorage.Query(
                    _ =>
                        _.Where(u => !u.IsArchived && u.Roles.Contains(UserRoles.Supervisor))
                            .Select(u => u.UserName.ToLower()).Distinct()).ToHashSet()
                );

            var archivedSupervisorNames = plainTransactionManager.ExecuteInPlainTransaction(() =>

                userStorage.Query(
                    _ =>
                        _.Where(u => u.IsArchived && u.Roles.Contains(UserRoles.Supervisor))
                            .Select(u => u.UserName.ToLower())).ToHashSet()
                );

            var archivedInterviewerNamesMappedOnSupervisorName = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
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

            return !this.passwordValidationRegex.IsMatch(userPreloadingDataRecord.Password);
        }

        private bool LoginFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return !this.loginValidatioRegex.IsMatch(userPreloadingDataRecord.Login);
        }

        private bool EmailFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Email))
                return false;

            return !this.emailValidationRegex.IsMatch(userPreloadingDataRecord.Email);
        }

        private bool PhoneNumberFormatVerification(UserPreloadingDataRecord userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.PhoneNumber))
                return false;

            return !this.phoneNumberValidationRegex.IsMatch(userPreloadingDataRecord.PhoneNumber);
        }

        private bool LoginNameUsedByExistingUser(HashSet<string> activeUserNames, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return activeUserNames.Contains(userPreloadingDataRecord.Login.ToLower());
        }

        private bool LoginDublicationInDataset(IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return data.Count(row => row.Login.ToLower() == userPreloadingDataRecord.Login.ToLower()) > 1;
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
            var columnName = ((MemberExpression) validator.ValueSelector.Body).Member.Name;
            foreach (var userPreloadingDataRecord in data)
            {
                if (validator.ValidationFunction(userPreloadingDataRecord))
                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.PushVerificationError(processId, validator.Code,
                            rowNumber, columnName, validator.ValueSelector.Compile()(userPreloadingDataRecord)));

                if (rowNumber%userPreloadingSettings.NumberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress == 0)
                {
                    var part = (double) indexOfCurrentVerification + (double) rowNumber/data.Count;
                    int intermediatePercents = (int) ((part/countOfVerifications)*100);

                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () =>
                            userPreloadingService.UpdateVerificationProgressInPercents(processId, intermediatePercents));
                }
                rowNumber++;
            }

            int percents = (int) ((((double) (indexOfCurrentVerification + 1))/countOfVerifications)*100);
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