using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserPreloadingCleaner : IUserPreloadingCleaner
    {
        private readonly IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage;

        private readonly UserPreloadingSettings userPreloadingSettings;

        private readonly IPlainTransactionManager plainTransactionManager;

        public UserPreloadingCleaner(
            IPlainTransactionManager plainTransactionManager, 
            UserPreloadingSettings userPreloadingSettings, 
            IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage)
        {
            this.plainTransactionManager = plainTransactionManager;
            this.userPreloadingSettings = userPreloadingSettings;
            this.userPreloadingProcessStorage = userPreloadingProcessStorage;
        }

        public void CleanUpInactiveUserPreloadingProcesses()
        {
            var processeIdsToClean =
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingProcessStorage.Query(_ => _.OrderBy(p => p.LastUpdateDate)
                        .Where(
                            p =>
                                p.LastUpdateDate <
                                DateTime.Now.AddDays(
                                    -userPreloadingSettings.HowOldInDaysProcessShouldBeInOrderToBeCleaned))
                        .Select(p => p.UserPreloadingProcessId)
                        .ToArray()));

            foreach (var processeIdToClean in processeIdsToClean)
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingProcessStorage.Remove(processeIdToClean));
            }
        }
    }
}