using System;
using System.Collections.Generic;
using CAPI.Android.Core.Model;
using CAPI.Android.Services;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization.Push
{
    public class PushDataProcessor
    {
        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;
        public PushDataProcessor(IChangeLogManipulator changelog, ICommandService commandService)
        {
            this.changelog = changelog;
            this.commandService = commandService;
        }

        public IList<SyncPackage> GetChuncks()
        {
            var chunks = changelog.GetClosedDraftChunksIds();
            var retval = new List<SyncPackage>();
            foreach (var chunk in chunks)
            {
                retval.Add(new SyncPackage()
                    {
                        Id = chunk.Key,
                        IsErrorOccured = false,
                        ItemsContainer =
                            new List<SyncItem>()
                                {
                                    new SyncItem()
                                        {
                                            Content = changelog.GetDraftRecordContent(chunk.Key),
                                            IsCompressed = true,
                                            ItemType = SyncItemType.Questionnare,
                                            Id = chunk.Value
                                        }
                                }
                    });
            }
            return retval;
        }

        public void DeleteInterview(Guid chunckId, Guid itemId)
        {
            new CleanUpExecutor(CapiApplication.Kernel.Get<IChangeLogManipulator>()).DeleteInterveiw(itemId);
        }
    }

}