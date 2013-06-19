// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileStoreDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Class handles file changes events.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Main.Core.Documents;
using Main.Core.Events.File;
using Main.Core.Services;
using Main.Core.Synchronization;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.EventHandlers
{
    /// <summary>
    /// Class handles file changes events.
    /// </summary>
    public class FileStoreDenormalizer : IEventHandler<FileUploaded>, IEventHandler<FileDeleted>
    {
        #region Fields

        private readonly IReadSideRepositoryWriter<FileDescription> attachments;
        private readonly IFileStorageService storage;
        private readonly ISynchronizationDataStorage syncStorage;

        #endregion

        #region Constructors and Destructors

        public FileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments, IFileStorageService storage, ISynchronizationDataStorage syncStorage)
        {
            this.attachments = attachments;
            this.storage = storage;
            this.syncStorage = syncStorage;
        }

        #endregion

        #region Public Methods and Operators

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
                this.storage.StoreFile(fileDescription);
                fileDescription.Content = null;
            }
            if (this.syncStorage != null)
                this.syncStorage.SaveImage(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.Description,
                                           evnt.Payload.OriginalFile);
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
            this.storage.DeleteFile(evnt.Payload.PublicKey.ToString());
        }

        #endregion

        #region Methods

        /// <summary>
        /// The from base 64.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The System.IO.MemoryStream.
        /// </returns>
        protected MemoryStream FromBase64(string text)
        {
            byte[] raw = Convert.FromBase64String(text);
            return new MemoryStream(raw);
        }

        #endregion
    }
}