using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using Ninject;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    public class BatchUserCreator : IJob
    {
        private readonly ILogger logger;

        protected readonly IPasswordHasher passwordHasher;

        ITransactionManager TransactionManager
        {
            get { return transactionManagerProvider.GetTransactionManager(); }
        }

        private readonly ITransactionManagerProvider transactionManagerProvider;

        IPlainTransactionManager PlainTransactionManager
        {
            get { return ServiceLocator.Current.GetInstance<IPlainTransactionManager>(); }
        }


        public BatchUserCreator(
            ILogger logger,
            IPasswordHasher passwordHasher, 
            ITransactionManagerProvider transactionManagerProvider)
        {
            this.logger = logger;
            this.passwordHasher = passwordHasher;
            this.transactionManagerProvider = transactionManagerProvider;
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            try
            {
                var userPreloadingService = ServiceLocator.Current.GetInstance<IUserPreloadingService>();
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
                    this.CreateUsersFromPreloadedData(userPreloadingService, preloadingProcessDataToCreate, preloadingProcessIdToCreate);
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
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
            }
        }

        private void CreateUsersFromPreloadedData(IUserPreloadingService userPreloadingService, IList<UserPreloadingDataRecord> data, string id)
        {
            var commandService = ServiceLocator.Current.GetInstance<ICommandService>();
            var supervisorsToCreate = data.Where(row => row.Role.ToLower() == "supervisor").ToArray();

            foreach (var supervisorToCreate in supervisorsToCreate)
            {
                TransactionManager.ExecuteInQueryTransaction(
                  () => commandService.Execute(new CreateUserCommand(Guid.NewGuid(), supervisorToCreate.Login,
                      passwordHasher.Hash(supervisorToCreate.Password), supervisorToCreate.Email,
                      new[] { UserRoles.Supervisor  },
                      false,
                      false, null,
                      supervisorToCreate.FullName, supervisorToCreate.PhoneNumber)));

                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncreaseCountCreateUsers(id));
            }
            var interviewersToCreate = data.Where(row => row.Role.ToLower() == "interviewer").ToArray();
            foreach (var interviewerToCreate in interviewersToCreate)
            {
                TransactionManager.ExecuteInQueryTransaction(
                    () => commandService.Execute(new CreateUserCommand(Guid.NewGuid(), interviewerToCreate.Login,
                        passwordHasher.Hash(interviewerToCreate.Password), interviewerToCreate.Email,
                        new[] { UserRoles.Operator},
                        false,
                        false, GetSupervisorForUserIfNeeded(interviewerToCreate),
                        interviewerToCreate.FullName, interviewerToCreate.PhoneNumber)));


                PlainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncreaseCountCreateUsers(id));
            }
        }

        private UserLight GetSupervisorForUserIfNeeded(UserPreloadingDataRecord dataRecord)
        {
            var userStorage = ServiceLocator.Current.GetInstance<IQueryableReadSideRepositoryReader<UserDocument>>();
            
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