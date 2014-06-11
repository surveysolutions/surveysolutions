using System;
using System.Collections.Concurrent;
using System.Threading;
using Main.Core;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.SyncCacher;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.UI.Capi.Syncronization
{
    public class SyncPackageApplier : ISyncPackageApplier
    {
        private static ConcurrentDictionary<Guid, bool> itemsInProcess = new ConcurrentDictionary<Guid, bool>();
        private const int CountOfAttempt = 200;

        private ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        private ISyncCacher syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();


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
                if (!this.syncCacher.DoesCachedItemExist(itemKey))
                    isAppliedSuccesfully = true;
                else
                {
                    var item = this.syncCacher.LoadItem(itemKey);

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        string content = PackageHelper.DecompressString(item);
                        var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

                        NcqrsEnvironment.Get<ICommandService>()
                            .Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));

                        this.syncCacher.DeleteItem(itemKey);

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