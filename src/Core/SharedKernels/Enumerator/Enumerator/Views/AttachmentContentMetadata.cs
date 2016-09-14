using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentContentMetadata : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }
    }
}