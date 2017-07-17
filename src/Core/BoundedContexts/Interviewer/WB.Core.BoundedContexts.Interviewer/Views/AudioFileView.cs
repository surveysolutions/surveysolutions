using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AudioFileView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public byte[] Content { get; set; }
    }
}