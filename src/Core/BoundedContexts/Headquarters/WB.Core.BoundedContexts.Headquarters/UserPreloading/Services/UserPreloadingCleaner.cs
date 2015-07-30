using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserPreloadingCleaner : IUserPreloadingCleaner
    {
        private readonly IUserPreloadingService userPreloadingService;

        private readonly IPlainTransactionManager plainTransactionManager;

        public UserPreloadingCleaner(IUserPreloadingService userPreloadingService, IPlainTransactionManager plainTransactionManager)
        {
            this.userPreloadingService = userPreloadingService;
            this.plainTransactionManager = plainTransactionManager;
        }

        public void CleanUpInactiveUserPreloadingProcesses()
        {
            var processesToClean =
                this.plainTransactionManager.ExecuteInPlainTransaction(() => userPreloadingService.GetPreloadingProcesses()
                    .Where(p => p.LastUpdateDate < DateTime.Now.AddDays(-1))
                    .ToArray());

            foreach (var userPreloadingProcess in processesToClean)
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.DeletePreloadingProcess(userPreloadingProcess.UserPreloadingProcessId));
            }
        }
    }
}