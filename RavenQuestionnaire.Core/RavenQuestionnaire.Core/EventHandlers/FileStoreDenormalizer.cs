using System;
using System.IO;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.File;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.EventHandlers
{
    /// <summary>
    /// Class handles file changes events.
    /// </summary>
    public class FileStoreDenormalizer : IEventHandler<FileUploaded>, IEventHandler<FileDeleted>
    {
        private IDenormalizerStorage<FileDescription> attachments;
        private IFileStorageService storage;
        public FileStoreDenormalizer(IDenormalizerStorage<FileDescription> attachments, IFileStorageService storage)
        {
            this.attachments = attachments;
            this.storage = storage;
        }
        protected MemoryStream FromBase64(string text)
        {
            byte[] raw = Convert.FromBase64String(text);
            return new MemoryStream(raw);
        }
        #region Implementation of IEventHandler<in ImageUpdated>

        public void Handle(IPublishedEvent<FileUploaded> evnt)
        {

            var fileDescription = new FileDescription()
                                      {
                                          PublicKey = evnt.Payload.PublicKey.ToString(),
                                          //  Content = original,
                                          Description = evnt.Payload.Description,
                                          Title = evnt.Payload.Title
                                      };
            attachments.Store(fileDescription, evnt.Payload.PublicKey);
            using (var original = FromBase64(evnt.Payload.OriginalFile))
            {
                fileDescription.Content = original;
                storage.StoreFile(fileDescription);
                fileDescription.Content = null;
            }
        }

        #endregion

        #region Implementation of IEventHandler<in FileDeleted>

        public void Handle(IPublishedEvent<FileDeleted> evnt)
        {
            attachments.Remove(evnt.Payload.PublicKey);
            storage.DeleteFile(evnt.Payload.PublicKey.ToString());
        }

        #endregion
    }
}
