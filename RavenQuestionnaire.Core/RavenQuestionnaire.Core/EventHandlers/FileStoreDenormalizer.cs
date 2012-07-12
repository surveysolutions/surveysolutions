using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class FileStoreDenormalizer : IEventHandler<ImageUploaded>
    {
        private IFileStorageService service;
        public FileStoreDenormalizer(IFileStorageService service)
        {
            this.service = service;
        }

        #region Implementation of IEventHandler<in ImageUpdated>

        public void Handle(IPublishedEvent<ImageUploaded> evnt)
        {
            using (var original = FromBase64(evnt.Payload.OriginalImage))
            {
                service.StoreFile(evnt.Payload.FileName, original);
            }
            using (var thumb = FromBase64(evnt.Payload.ThumbnailImage))
            {
                service.StoreFile(evnt.Payload.ThumbName, thumb);
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
