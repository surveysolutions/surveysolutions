using System;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SyncPackageRestoreService : ISyncPackageRestoreService
    {
        private readonly ILogger logger;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;

        private static readonly NamedLocker Namedlocker = new NamedLocker();
        private readonly ISerializer serializer;
        private readonly ICommandService commandService;

        public SyncPackageRestoreService(ILogger logger,
            ICapiSynchronizationCacheService capiSynchronizationCacheService,
            ISerializer serializer,
            ICommandService commandService)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
            this.serializer = serializer;
            this.commandService = commandService;
        }

        public void CheckAndApplySyncPackage(Guid itemKey)
        {
            string key = itemKey.FormatGuid();
            lock (Namedlocker.GetLock(key))
            {
                try
                {
                    if (this.capiSynchronizationCacheService.DoesCachedItemExist(itemKey))
                    {
                        var item = this.capiSynchronizationCacheService.LoadItem(itemKey);

                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            var interview = this.serializer.Deserialize<InterviewSynchronizationDto>(item);

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
            }
        }
    }
}