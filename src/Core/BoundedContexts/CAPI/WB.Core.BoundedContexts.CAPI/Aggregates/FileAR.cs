using System;
using Main.Core.Events.File;
using Ncqrs.Domain;

namespace WB.Core.BoundedContexts.Capi.Aggregates
{
    public class FileAR : AggregateRootMappedByConvention
    {
        public FileAR()
        {
        }

        public FileAR(Guid publicKey, string title, string description, string originalFile)
            : base(publicKey)
        {
            this.UploadFile(description, originalFile, publicKey, title);
        }

        public void UploadFile(string description, string originalFile, Guid publicKey, string title)
        {
            this.ApplyEvent(
                new FileUploaded
                {
                    PublicKey = publicKey,
                    Title = title,
                    Description = description,
                    OriginalFile = originalFile
                });
        }

        protected void OnFileUploaded(FileUploaded e)
        {
        }
    }
}