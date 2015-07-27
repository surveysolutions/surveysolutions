using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    public class BatchUserCreator : IJob
    {
        private readonly IUserPreloadingService userPreloadingService;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;
        private readonly ILogger logger;

        protected readonly IPasswordHasher passwordHasher;

        ITransactionManager TransactionManager
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManager>(); }
        }

        private readonly IPlainTransactionManager PlainTransactionManager;

        public BatchUserCreator(IUserPreloadingService userPreloadingService, ILogger logger,
            ICommandService commandService, IQueryableReadSideRepositoryReader<UserDocument> userStorage,
            IPasswordHasher passwordHasher, IPlainTransactionManager plainTransactionManager)
        {
            this.userPreloadingService = userPreloadingService;
            this.logger = logger;
            this.commandService = commandService;

            this.userStorage = userStorage;
            this.passwordHasher = passwordHasher;
            this.PlainTransactionManager = plainTransactionManager;
        }

        public void Execute(IJobExecutionContext context)
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
            foreach (var userPreloadingDataRecord in data)
            {
                var role = ParseUserRole(userPreloadingDataRecord.Role);
                TransactionManager.ExecuteInQueryTransaction(
                    () => commandService.Execute(new CreateUserCommand(Guid.NewGuid(), userPreloadingDataRecord.Login,
                        passwordHasher.Hash(userPreloadingDataRecord.Password), userPreloadingDataRecord.Email,
                        new[] {role},
                        false,
                        false, GetSupervisorForUserIfNeeded(userPreloadingDataRecord, role),
                        userPreloadingDataRecord.FullName, userPreloadingDataRecord.PhoneNumber)));

                userPreloadingService.IncreaseCountCreateUsers(id);
            }
        }

        private UserRoles ParseUserRole(string role)
        {
            if (role.ToLower() == "supervisor")
                return UserRoles.Supervisor;
            if (role.ToLower() == "interviewer")
                return UserRoles.Operator;

            return UserRoles.Undefined;
        }

        private UserLight GetSupervisorForUserIfNeeded(UserPreloadingDataRecord dataRecord, UserRoles role)
        {
            if (role != UserRoles.Operator)
                return null;
            
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