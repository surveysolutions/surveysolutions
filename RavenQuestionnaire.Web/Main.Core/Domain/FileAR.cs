using System;
using Main.Core.Events.File;
using Ncqrs.Domain;

namespace Main.Core.Domain
{
    public class FileAR : AggregateRootMappedByConvention
    {
        public FileAR()
        {
        }

        public FileAR(Guid publicKey, string title, string description, string originalFile)
            : base(publicKey)
        {
            this.ApplyEvent(
                new FileUploaded
                    {
                       PublicKey = publicKey, Title = title, Description = description, OriginalFile = originalFile 
                    });
        }

        protected void OnFileUploaded(FileUploaded e)
        {
        }
    }
}