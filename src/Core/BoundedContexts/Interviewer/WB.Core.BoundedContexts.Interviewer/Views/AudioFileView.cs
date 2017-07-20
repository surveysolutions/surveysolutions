using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AudioFileView : IFileView, IPlainStorageEntity
    {
        [PrimaryKey, AutoIncrement]
        public string Id { get; set; }
        public byte[] File { get; set; }
    }
}