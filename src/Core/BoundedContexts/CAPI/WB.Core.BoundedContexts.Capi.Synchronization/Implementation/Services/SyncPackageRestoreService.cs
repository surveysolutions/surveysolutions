using System;
using System.Collections.Concurrent;
using System.Threading;
using Main.Core;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services
{
    public class SyncPackageRestoreService : ISyncPackageRestoreService
    {
        private static ConcurrentDictionary<Guid, bool> itemsInProcess = new ConcurrentDictionary<Guid, bool>();
        private const int CountOfAttempt = 200;

        private readonly ILogger logger;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;


        public SyncPackageRestoreService(ILogger logger, ICapiSynchronizationCacheService capiSynchronizationCacheService)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
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
                Thread.Sleep(1000);
                i++;
            }
            return true;
        }

        private void ReleaseItem(Guid id)
        {
            bool dummyBool;
            itemsInProcess.TryRemove(id, out dummyBool);
        }

        public bool CheckAndApplySyncPackage(Guid itemKey)
        {
            if (!this.WaitUntilItemCanBeProcessed(itemKey))
                return false;

            bool isAppliedSuccesfully = false;
            try
            {
                if (!this.capiSynchronizationCacheService.DoesCachedItemExist(itemKey))
                    isAppliedSuccesfully = true;
                else
                {
                    var item = this.capiSynchronizationCacheService.LoadItem(itemKey);

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        string content = PackageHelper.DecompressString(item);
                        var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

                        NcqrsEnvironment.Get<ICommandService>()
                            .Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));

                        this.capiSynchronizationCacheService.DeleteItem(itemKey);

                        isAppliedSuccesfully = true;
                    }
                }
            }
            catch (Exception e)
            {
                //if state is saved as event but denormalizer failed we won't delete file
                this.logger.Error("Error occured during applying interview after synchronization", e);
                isAppliedSuccesfully = false;
            }
            finally
            {
                this.ReleaseItem(itemKey);
            }

            return isAppliedSuccesfully;
        }
    }
}