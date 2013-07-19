using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.File;
using Main.Core.Services;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace Core.Supervisor.Denormalizer
{
    public class SupervisorFileStoreDenormalizer : FileStoreDenormalizer
    {
        private readonly ISynchronizationDataStorage syncStorage;
        public SupervisorFileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments, IFileStorageService storage, ISynchronizationDataStorage syncStorage)
            : base(attachments, storage)
        {
            this.syncStorage = syncStorage;
        }

        public override void PostSaveHandler(IPublishedEvent<FileUploaded> evnt)
        {
            this.syncStorage.SaveImage(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.Description,
                                             evnt.Payload.OriginalFile);
        }
    }
}
