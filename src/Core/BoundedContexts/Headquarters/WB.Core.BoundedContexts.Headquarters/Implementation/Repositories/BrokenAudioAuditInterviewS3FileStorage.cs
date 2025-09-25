using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public class BrokenAudioAuditInterviewS3FileStorage : InterviewS3FileStorage, IAudioAuditFileStorage, IBrokenAudioAuditFileStorage
{
    private const string AudioAuditS3Folder = "broken_interview_data/audio_audit/";

    public BrokenAudioAuditInterviewS3FileStorage(IExternalFileStorage externalFileStorage, 
        IFileSystemAccessor fileSystemAccessor) 
        : base(externalFileStorage, fileSystemAccessor)
    {
    }
    
    protected override string GetInterviewDirectoryPath(Guid interviewId) => $"{AudioAuditS3Folder}{interviewId.FormatGuid()}";
    protected override string ContentType => "audio/mp4";

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
    {
        throw new NotImplementedException();
    }
}
