using SQLite.Net.Attributes;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentContent : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public long Size { get; set; }
    }
}