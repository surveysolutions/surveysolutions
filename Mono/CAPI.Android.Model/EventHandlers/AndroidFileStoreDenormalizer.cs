using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.File;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class AndroidFileStoreDenormalizer : FileStoreDenormalizer
    {
        public AndroidFileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments) 
            : base(attachments)
        {
        }

        public override void PostSaveHandler(IPublishedEvent<FileUploaded> evnt)
        {
            
        }
    }
}