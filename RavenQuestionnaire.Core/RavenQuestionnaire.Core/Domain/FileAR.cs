using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Domain
{
    public class FileAR : AggregateRootMappedByConvention
    {
        private FileDescription innerDocument;
        public FileAR()
        {
        }
        public FileAR(Guid publicKey, string title, string description, string originalImage, int originalWidth, 
            int originalHeight, int thumbWidth, int thumbHeight, string originalFile, string thumbFile, string thumbnailImage)
            : base(publicKey)
        {
        }
    }
}
