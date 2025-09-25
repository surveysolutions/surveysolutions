using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

public class BrokenAudioFileStorage : InterviewFileStorage, IAudioFileStorage, IBrokenAudioFileStorage
{
    public BrokenAudioFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath) 
        : base(fileSystemAccessor, rootDirectoryPath)
    {
    }
    
    private readonly string brokenFolderName = "BrokenInterviewData";
    private readonly string imagesFolderName = "audio";

    protected override string ContentType => "audio/mp4";
    
    protected override string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory)
    {
        return fileSystemAccessor.CombinePath(baseDirectory, brokenFolderName, imagesFolderName, interviewId.FormatGuid());
    }


    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}

