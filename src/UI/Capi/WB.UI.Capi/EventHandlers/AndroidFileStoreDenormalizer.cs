using Main.Core.Events.File;

using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.UI.Capi.EventHandlers
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