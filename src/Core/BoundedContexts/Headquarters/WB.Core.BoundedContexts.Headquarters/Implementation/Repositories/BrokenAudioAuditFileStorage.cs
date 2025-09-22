using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public class BrokenAudioAuditFileStorage : InterviewFileStorage, IAudioAuditFileStorage, IBrokenAudioAuditFileStorage
{
    public BrokenAudioAuditFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath) 
        : base(fileSystemAccessor, rootDirectoryPath)
    {
    }
    
    private readonly string brokenFolderName = "BrokenInterviewData";
    private readonly string imagesFolderName = "audio_audit";
    
    protected override string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory)
    {
        return fileSystemAccessor.CombinePath(baseDirectory, brokenFolderName, imagesFolderName, interviewId.FormatGuid());
    }


    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
    {
        throw new NotImplementedException();
    }
}
