using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.File;

namespace RavenQuestionnaire.Core.EventHandlers
{
    /// <summary>
    /// Class handles file changes events.
    /// </summary>
    public class FileStoreDenormalizer : IEventHandler<FileUploaded>, IEventHandler<FileDeleted>
    {
        private IDenormalizerStorage<FileDescription> attachments;
        public FileStoreDenormalizer(IDenormalizerStorage<FileDescription> attachments)
        {
            this.attachments = attachments;
        }

        #region Implementation of IEventHandler<in ImageUpdated>

        public void Handle(IPublishedEvent<FileUploaded> evnt)
        {
            attachments.Store(
                new FileDescription()
                    {
                        Description = evnt.Payload.Description,
                        PublicKey = evnt.Payload.PublicKey.ToString(),
                        Title = evnt.Payload.Title
                    }, evnt.Payload.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in FileDeleted>

        public void Handle(IPublishedEvent<FileDeleted> evnt)
        {
            attachments.Remove(evnt.Payload.PublicKey);
        }

        #endregion
    }
}
