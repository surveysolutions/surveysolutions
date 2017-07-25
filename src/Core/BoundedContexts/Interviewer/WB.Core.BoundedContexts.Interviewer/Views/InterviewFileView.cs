using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewFileView : IFileView, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public byte[] File { get; set; }
    }
}