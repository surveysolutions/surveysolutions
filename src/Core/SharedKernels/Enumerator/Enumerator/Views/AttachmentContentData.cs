using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentContentData : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public byte[] Content { get; set; }
    }
}