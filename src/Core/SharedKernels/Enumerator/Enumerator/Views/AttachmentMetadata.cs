using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentMetadata : IPlainStorageEntity
    {
        public string Id { get; set; }

        public string AttachmentContentId { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }
    }
}