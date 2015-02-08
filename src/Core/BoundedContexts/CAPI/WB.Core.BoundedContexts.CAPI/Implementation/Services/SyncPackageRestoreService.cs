using System;
using System.Collections.Concurrent;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class SyncPackageRestoreService : ISyncPackageRestoreService
    {
        private static ConcurrentDictionary<Guid, bool> itemsInProcess = new ConcurrentDictionary<Guid, bool>();
        private const int CountOfAttempt = 200;

        private readonly ILogger logger;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;

        private readonly IJsonUtils jsonUtils;
        private readonly ICommandService commandService;
        private readonly IWaitService waitService;

        public SyncPackageRestoreService(ILogger logger, 
            ICapiSynchronizationCacheService capiSynchronizationCacheService, 
            IJsonUtils jsonUtils, 
            ICommandService commandService,
            IWaitService waitService)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
            this.jsonUtils = jsonUtils;
            this.commandService = commandService;
            this.waitService = waitService;
        }

        private bool WaitUntilItemCanBeProcessed(Guid id)
        {
            int i = 0;
            while (!itemsInProcess.TryAdd(id, true))
            {
                if (i > CountOfAttempt)
                {
                    return false;
                }
                waitService.WaitForSecond();
                i++;
            }
            return true;
        }

        private void ReleaseItem(Guid id)
        {
            bool dummyBool;
            itemsInProcess.TryRemove(id, out dummyBool);
        }

        public void CheckAndApplySyncPackage(Guid itemKey)
        {
            if (!this.WaitUntilItemCanBeProcessed(itemKey))
                throw new TimeoutException(string.Format("Can't put interview '{0}' in possessing line", itemKey));
            try
            {
                if (this.capiSynchronizationCacheService.DoesCachedItemExist(itemKey))
                {
                    var item = this.capiSynchronizationCacheService.LoadItem(itemKey);

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        var interview = this.jsonUtils.Deserialize<InterviewSynchronizationDto>(item);

                        this.commandService.Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));

                        this.capiSynchronizationCacheService.DeleteItem(itemKey);
                    }
                }
            }
            catch (Exception e)
            {
                //if state is saved as event but denormalizer failed we won't delete file
                this.logger.Error(string.Format("Error occured during applying interview after synchronization. Item key: {0}", itemKey), e);
                throw;
            }
            finally
            {
                this.ReleaseItem(itemKey);
            }
        }
    }
}