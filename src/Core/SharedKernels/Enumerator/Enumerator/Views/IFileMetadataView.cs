using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public interface IFileMetadataView : IPlainStorageEntity
    {
        string Id { get; set; }
        Guid InterviewId { get; set; }
        string FileId { get; set; }
        string FileName { get; set; }
        string ContentType { get; set; }
    }
}