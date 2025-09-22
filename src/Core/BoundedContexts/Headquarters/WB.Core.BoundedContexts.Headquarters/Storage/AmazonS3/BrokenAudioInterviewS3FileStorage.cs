using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;

public class BrokenAudioInterviewS3FileStorage : InterviewS3FileStorage, IAudioFileStorage, IBrokenAudioFileStorage
{
    private const string AudioAuditS3Folder = "broken_interview_data/audio/";

    public BrokenAudioInterviewS3FileStorage(IExternalFileStorage externalFileStorage, 
        IFileSystemAccessor fileSystemAccessor) 
        : base(externalFileStorage, fileSystemAccessor)
    {
    }
    
    protected override string GetInterviewDirectoryPath(Guid interviewId) => $"{AudioAuditS3Folder}{interviewId.FormatGuid()}";

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
