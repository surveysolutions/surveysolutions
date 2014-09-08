using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.File;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Denormalizer
{
    public class SupervisorFileStoreDenormalizer : FileStoreDenormalizer
    {
        private readonly ISynchronizationDataStorage syncStorage;

        public SupervisorFileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments, ISynchronizationDataStorage syncStorage)
            : base(attachments)
        {
            this.syncStorage = syncStorage;
        }

        public override void PostSaveHandler(IPublishedEvent<FileUploaded> evnt)
        {
            this.syncStorage.SaveImage(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.Description,
                                             evnt.Payload.OriginalFile, evnt.EventTimeStamp);
        }
    }
}
