using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using Ninject;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingVerifier : IJob
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

        public UserPreloadingVerifier(
            ITransactionManagerProvider transactionManagerProvider)
        {   this.transactionManagerProvider = transactionManagerProvider;
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            try
            {
                var userPreloadingService = ServiceLocator.Current.GetInstance<IUserPreloadingService>();
                var userStorage = ServiceLocator.Current.GetInstance<IQueryableReadSideRepositoryReader<UserDocument>>();
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
                    this.ValidatePreloadedData(userPreloadingService, userStorage, preloadingProcessDataToValidate,
                        preloadingProcessIdToValidate);
                });

                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.FinishValidationProcess(preloadingProcessIdToValidate));
            }
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
            }
        }

        void ValidatePreloadedData(IUserPreloadingService userPreloadingService,
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data,
            string processId)
        {
            ValidateRow(userPreloadingService, userStorage, data, processId, LoginNameTakenByExistingUser, "PLU0001", "Login", u => u.Login);
            ValidateRow(userPreloadingService, userStorage, data, processId, LoginDublicationInDataset, "PLU0002", "Login", u => u.Login);
            ValidateRow(userPreloadingService, userStorage, data, processId, LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam, "PLU0003", "Login", u => u.Login);
            ValidateRow(userPreloadingService, userStorage, data, processId, LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole, "PLU0004", "Login", u => u.Login);
            ValidateRow(userPreloadingService, data, processId, LoginFormatVerification, "PLU0005", "Login", u => u.Login);
            ValidateRow(userPreloadingService, data, processId, PasswordFormatVerification, "PLU0006", "Password", u => u.Password);
            ValidateRow(userPreloadingService, data, processId, EmailFormatVerification, "PLU0007", "Email", u => u.Email);
            ValidateRow(userPreloadingService, data, processId, PhoneNumberFormatVerification, "PLU0008", "PhoneNumber", u => u.PhoneNumber);
            ValidateRow(userPreloadingService, data, processId, RoleVerification, "PLU0009", "Role", u => u.Role);
            ValidateRow(userPreloadingService, userStorage, data, processId, SupervisorVerification, "PLU0010", "Supervisor", u => u.Supervisor);
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

        private bool LoginNameTakenByExistingUser(IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return
                    userStorage.Query(
                        _ =>
                            _.Where(
                                u => !u.IsArchived && u.UserName.ToLower() == userPreloadingDataRecord.Login.ToLower()))
                        .Any();
        }

        private bool LoginDublicationInDataset(IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
        {
            return data.Count(row => row.Login.ToLower() == userPreloadingDataRecord.Login) > 1;
        }

        private bool LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data, 
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

        private bool LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
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

        private void ValidateRow(
            IUserPreloadingService userPreloadingService,
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, 
            IList<UserPreloadingDataRecord> data, 
            string processId, 
            Func<IQueryableReadSideRepositoryReader<UserDocument>, IList<UserPreloadingDataRecord>,UserPreloadingDataRecord, bool> validation, 
            string code, 
            string columnName,
            Func<UserPreloadingDataRecord, string> cellFunc)
        {
            int rowNumber = 1;
            
            foreach (var userPreloadingDataRecord in data)
            {
                if (validation(userStorage, data, userPreloadingDataRecord))

                    PlainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.PushVerificationError(processId, code,
                            rowNumber, columnName, cellFunc(userPreloadingDataRecord)));

                rowNumber++;
            }
        }

        private void ValidateRow(
            IUserPreloadingService userPreloadingService,
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
            var role = ParseUserRole(userPreloadingDataRecord.Role);
            return role == UserRoles.Undefined;
        }

        private bool SupervisorVerification(IQueryableReadSideRepositoryReader<UserDocument> userStorage, IList<UserPreloadingDataRecord> data, UserPreloadingDataRecord userPreloadingDataRecord)
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