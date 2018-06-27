using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AudioFileMetadataView : IFileMetadataView, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public Guid InterviewId { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
