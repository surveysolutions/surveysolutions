using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class FileStoreDenormalizer : IEventHandler<ImageUploaded>
    {
        private IFileStorageService service;
        private IDenormalizerStorage<FileDescription> attachments;
        public FileStoreDenormalizer(IFileStorageService service, IDenormalizerStorage<FileDescription> attachments)
        {
            this.service = service;
            this.attachments = attachments;
        }

        #region Implementation of IEventHandler<in ImageUpdated>

        public void Handle(IPublishedEvent<ImageUploaded> evnt)
        {
            using (var original = FromBase64(evnt.Payload.OriginalImage))
            {
                var originalFile = new FileDescription()
                                       {
                                           PublicKey = evnt.Payload.ImagePublicKey,
                                           Content = original.ToArray(),
                                           Description = evnt.Payload.Description,
                                           Height = evnt.Payload.OriginalHeight,
                                           Title = evnt.Payload.Title,
                                           Width = evnt.Payload.OriginalWidth
                                       };
                service.StoreFile(originalFile);
                
            }
            using (var thumb = FromBase64(evnt.Payload.ThumbnailImage))
            {
                var tumbFile = new FileDescription()
                                   {
                                       PublicKey = evnt.Payload.ThumbPublicKey,
                                       Content = thumb.ToArray(),
                                       Description = evnt.Payload.Description,
                                       Height = evnt.Payload.ThumbHeight,
                                       Title = evnt.Payload.Title,
                                       Width = evnt.Payload.ThumbWidth
                                   };
                service.StoreFile(tumbFile);
                attachments.Store(tumbFile, evnt.Payload.ImagePublicKey);
            }
        }

        #endregion
        protected MemoryStream FromBase64(string text)
        {
            byte[] raw = Convert.FromBase64String(text);
            return new MemoryStream(raw);
        }
    }
}
