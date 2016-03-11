using System;
using SQLite.Net.Attributes;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentMetadata : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string AttachmentContentId { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }
    }
}