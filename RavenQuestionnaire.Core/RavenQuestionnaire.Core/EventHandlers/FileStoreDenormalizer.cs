using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.File;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class FileStoreDenormalizer : IEventHandler<FileUploaded>, IEventHandler<FileDeleted>
    {
   //     private IFileStorageService service;
        private IDenormalizerStorage<FileDescription> attachments;
        public FileStoreDenormalizer(/*IFileStorageService service,*/ IDenormalizerStorage<FileDescription> attachments)
        {
        //    this.service = service;
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
