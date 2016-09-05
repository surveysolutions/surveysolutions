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

        private readonly IPlainStorageAccessor<UserDocument> userStorage;

        private readonly ILogger logger;

        protected readonly IPasswordHasher passwordHasher;

        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private bool IsWorking = false;

        public UserBatchCreator(
            IUserPreloadingService userPreloadingService, 
            ICommandService commandService,
            IPlainStorageAccessor<UserDocument> userStorage, 
            ILogger logger, 
            IPasswordHasher passwordHasher)
        {
            this.userPreloadingService = userPreloadingService;
            this.commandService = commandService;
            this.userStorage = userStorage;
            this.logger = logger;
            this.passwordHasher = passwordHasher;
        }

        public void CreateUsersFromReadyToBeCreatedQueue()
        {
            if (IsWorking)
                return;

            IsWorking = true;
            try
            {
                while (IsWorking)
                {
                    string preloadingProcessIdToCreate = this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.DeQueuePreloadingProcessIdReadyToCreateUsers());

                    if (string.IsNullOrEmpty(preloadingProcessIdToCreate))
                        break;

                    var preloadingProcessDataToCreate =
                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToCreate).UserPrelodingData.ToList());
                    try
                    {
                        this.CreateUsersFromPreloadedData(preloadingProcessDataToCreate, preloadingProcessIdToCreate);
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("preloading process with id {0} finished with error", preloadingProcessIdToCreate), e);

                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.FinishPreloadingProcessWithError(preloadingProcessIdToCreate, e.Message));
                        return;
                    }

                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.FinishPreloadingProcess(preloadingProcessIdToCreate));
                }

            }
            finally
            {
                IsWorking = false;
            }

        }

        private void CreateUsersFromPreloadedData(IList<UserPreloadingDataRecord> data, string id)
        {
            var supervisorsToCreate =
                data.Where(row => userPreloadingService.GetUserRoleFromDataRecord(row) == UserRoles.Supervisor)
                    .ToArray();

            foreach (var supervisorToCreate in supervisorsToCreate)
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => CreateSupervisorOrUnarchiveAndUpdate(supervisorToCreate));

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
            }

            var interviewersToCreate = data.Where(row => userPreloadingService.GetUserRoleFromDataRecord(row) == UserRoles.Interviewer).ToArray();

            foreach (var interviewerToCreate in interviewersToCreate)
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => CreateInterviewerOrUnarchiveAndUpdate(interviewerToCreate));

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
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
                throw new UserPreloadingException(
                    String.Format("archived user '{0}' is in role '{1}' but must be in role supervisor",
                        archivedSupervisor.UserName, string.Join(",", archivedSupervisor.Roles)));

            commandService.Execute(new UnarchiveUserAndUpdateCommand(archivedSupervisor.PublicKey,
                passwordHasher.Hash(supervisorToCreate.Password), supervisorToCreate.Email, supervisorToCreate.FullName,
                supervisorToCreate.PhoneNumber));
        }

        void CreateInterviewerOrUnarchiveAndUpdate(UserPreloadingDataRecord interviewerToCreate)
        {
            var archivedInterviewers =
                userStorage.Query(
                    _ =>
                        _.FirstOrDefault(
                            u => u.UserName.ToLower() == interviewerToCreate.Login.ToLower() && u.IsArchived));

            var supervisor = this.GetSupervisorForUser(interviewerToCreate);

            if (archivedInterviewers == null)
            {
                commandService.Execute(new CreateUserCommand(Guid.NewGuid(), interviewerToCreate.Login,
                    passwordHasher.Hash(interviewerToCreate.Password), interviewerToCreate.Email,
                    new[] { UserRoles.Interviewer },
                    false,
                    false, supervisor,
                    interviewerToCreate.FullName, interviewerToCreate.PhoneNumber));
                return;
            }

            if (!archivedInterviewers.Roles.Contains(UserRoles.Interviewer))
                throw new UserPreloadingException(
                    String.Format("archived user '{0}' is in role '{1}' but must be in role interviewer",
                        archivedInterviewers.UserName, string.Join(",", archivedInterviewers.Roles)));

            commandService.Execute(new UnarchiveUserAndUpdateCommand(archivedInterviewers.PublicKey,
                passwordHasher.Hash(interviewerToCreate.Password), interviewerToCreate.Email, interviewerToCreate.FullName,
                interviewerToCreate.PhoneNumber));
        }

        private UserLight GetSupervisorForUser(UserPreloadingDataRecord dataRecord)
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