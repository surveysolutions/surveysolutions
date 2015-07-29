using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserBatchCreator : IUserBatchCreator
    {
        private readonly IUserPreloadingService userPreloadingService;

        private readonly ICommandService commandService;

        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;

        private readonly ILogger logger;

        protected readonly IPasswordHasher passwordHasher;

        private readonly ITransactionManagerProvider transactionManagerProvider;

        IPlainTransactionManager PlainTransactionManager
        {
            get { return ServiceLocator.Current.GetInstance<IPlainTransactionManager>(); }
        }

        ITransactionManager TransactionManager
        {
            get { return transactionManagerProvider.GetTransactionManager(); }
        }

        public UserBatchCreator(IUserPreloadingService userPreloadingService, ICommandService commandService, IQueryableReadSideRepositoryReader<UserDocument> userStorage, ILogger logger, IPasswordHasher passwordHasher, ITransactionManagerProvider transactionManagerProvider)
        {
            this.userPreloadingService = userPreloadingService;
            this.commandService = commandService;
            this.userStorage = userStorage;
            this.logger = logger;
            this.passwordHasher = passwordHasher;
            this.transactionManagerProvider = transactionManagerProvider;
        }

        public void CreateUsersFromReadyToBeCreatedQueue()
        {
            string preloadingProcessIdToCreate =
                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.DeQueuePreloadingProcessIdReadyToCreateUsers());

            if (string.IsNullOrEmpty(preloadingProcessIdToCreate))
                return;

            var preloadingProcessDataToCreate =
                PlainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToCreate)
                            .UserPrelodingData.ToList());

            try
            {
                this.CreateUsersFromPreloadedData(preloadingProcessDataToCreate, preloadingProcessIdToCreate);
            }
            catch (Exception e)
            {
                logger.Error(
                    string.Format("preloading process with id {0} finished with error", preloadingProcessIdToCreate),
                    e);

                PlainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        userPreloadingService.FinishPreloadingProcessWithError(preloadingProcessIdToCreate,
                            e.Message));
                return;
            }
            PlainTransactionManager.ExecuteInPlainTransaction(
                () => userPreloadingService.FinishPreloadingProcess(preloadingProcessIdToCreate));
        }


        private void CreateUsersFromPreloadedData(IList<UserPreloadingDataRecord> data, string id)
        {
            var supervisorsToCreate = data.Where(row => row.Role.ToLower() == "supervisor").ToArray();

            foreach (var supervisorToCreate in supervisorsToCreate)
            {
                TransactionManager.ExecuteInQueryTransaction(
                    () => CreateSupervisorOrUnarchiveAndUpdate(supervisorToCreate));

                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncreaseCountCreateUsers(id));
            }

            var interviewersToCreate = data.Where(row => row.Role.ToLower() == "interviewer").ToArray();

            foreach (var interviewerToCreate in interviewersToCreate)
            {
                TransactionManager.ExecuteInQueryTransaction(
                    () => CreateInterviewerOrUnarchiveAndUpdate(interviewerToCreate));

                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncreaseCountCreateUsers(id));
            }
        }


        private void CreateSupervisorOrUnarchiveAndUpdate(UserPreloadingDataRecord supervisorToCreate)
        {
            var archivedSupervisor =
                userStorage.Query(
                    _ =>
                        _.FirstOrDefault(u => u.UserName.ToLower() == supervisorToCreate.Login.ToLower() && u.IsArchived));
            if (archivedSupervisor == null)
            {
                commandService.Execute(new CreateUserCommand(Guid.NewGuid(), supervisorToCreate.Login,
                    passwordHasher.Hash(supervisorToCreate.Password), supervisorToCreate.Email,
                    new[] { UserRoles.Supervisor },
                    false,
                    false, null,
                    supervisorToCreate.FullName, supervisorToCreate.PhoneNumber));
                return;
            }

            if (!archivedSupervisor.Roles.Contains(UserRoles.Supervisor))
                throw new ArgumentException(
                    String.Format("archived user '{0}' is in role '{1}' but must be in role supervisor",
                        archivedSupervisor.UserName, string.Join(",", archivedSupervisor.Roles)));

            commandService.Execute(new UnarchiveUserCommand(archivedSupervisor.PublicKey));
            commandService.Execute(new ChangeUserCommand(archivedSupervisor.PublicKey, supervisorToCreate.Email, false,
                false, passwordHasher.Hash(supervisorToCreate.Password), supervisorToCreate.FullName,
                supervisorToCreate.PhoneNumber, archivedSupervisor.PublicKey));
        }

        void CreateInterviewerOrUnarchiveAndUpdate(UserPreloadingDataRecord interviewerToCreate)
        {
            var archivedInterviewers =
                userStorage.Query(
                    _ =>
                        _.FirstOrDefault(
                            u => u.UserName.ToLower() == interviewerToCreate.Login.ToLower() && u.IsArchived));

            var supervisor = GetSupervisorForUserIfNeeded(interviewerToCreate);

            if (archivedInterviewers == null)
            {
                commandService.Execute(new CreateUserCommand(Guid.NewGuid(), interviewerToCreate.Login,
                    passwordHasher.Hash(interviewerToCreate.Password), interviewerToCreate.Email,
                    new[] { UserRoles.Operator },
                    false,
                    false, supervisor,
                    interviewerToCreate.FullName, interviewerToCreate.PhoneNumber));
                return;
            }

            if (!archivedInterviewers.Roles.Contains(UserRoles.Operator))
                throw new ArgumentException(
                    String.Format("archived user '{0}' is in role '{1}' but must be in role interviewer",
                        archivedInterviewers.UserName, string.Join(",", archivedInterviewers.Roles)));

            commandService.Execute(new UnarchiveUserCommand(archivedInterviewers.PublicKey));
            commandService.Execute(new ChangeUserCommand(archivedInterviewers.PublicKey, interviewerToCreate.Email,
                false,
                false, passwordHasher.Hash(interviewerToCreate.Password), interviewerToCreate.FullName,
                interviewerToCreate.PhoneNumber, archivedInterviewers.PublicKey));
        }

        private UserLight GetSupervisorForUserIfNeeded(UserPreloadingDataRecord dataRecord)
        {
            var supervisor =
                userStorage.Query(_ => _.FirstOrDefault(u => u.UserName.ToLower() == dataRecord.Supervisor.ToLower()));

            if (supervisor == null)
                return null;

            if (!supervisor.Roles.Contains(UserRoles.Supervisor))
                return null;

            return new UserLight(supervisor.PublicKey, supervisor.UserName);
        }
    }
}