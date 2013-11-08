using System;
using System.IO;
using Main.Core.Documents;
using Main.Core.Events.File;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.EventHandlers
{
    /// <summary>
    /// Class handles file changes events.
    /// </summary>
    public abstract class FileStoreDenormalizer : 
        IEventHandler<FileUploaded>, 
        IEventHandler<FileDeleted>
    {
        private readonly IReadSideRepositoryWriter<FileDescription> attachments;

        public FileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments)
        {
            this.attachments = attachments;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<FileUploaded> evnt)
        {
            var fileDescription = new FileDescription
                {
                    FileName = evnt.Payload.PublicKey.ToString(),
                    // Content = original,
                    Description = evnt.Payload.Description,
                    Title = evnt.Payload.Title
                };
            this.attachments.Store(fileDescription, evnt.Payload.PublicKey);
            using (MemoryStream original = this.FromBase64(evnt.Payload.OriginalFile))
            {
                fileDescription.Content = original;
                fileDescription.Content = null;
            }
            PostSaveHandler(evnt);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<FileDeleted> evnt)
        {
            this.attachments.Remove(evnt.Payload.PublicKey);
        }

        public abstract void PostSaveHandler(IPublishedEvent<FileUploaded> evnt);

        protected MemoryStream FromBase64(string text)
        {
            byte[] raw = Convert.FromBase64String(text);
            return new MemoryStream(raw);
        }
    }
}