using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<UsersImportJob>();

        private IPlainTransactionManager transactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();

        private IUserPreloadingService importUsersService => ServiceLocator.Current
            .GetInstance<IUserPreloadingService>();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                bool hasUsersToImport;
                do
                {
                    hasUsersToImport = this.transactionManager.ExecuteInPlainTransaction(() => this
                        .importUsersService.ImportFirstUserAndReturnIfHasMoreUsersToImportAsync().Result);

                } while (hasUsersToImport);
            }
            catch (Exception ex)
            {
                this.logger.Error($"User import job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }
}