using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

public class BrokenImageFileStorage : ImageFileStorage, IBrokenImageFileStorage
{
    public BrokenImageFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath)
        : base(fileSystemAccessor, rootDirectoryPath)
    {
    }
        
    private readonly string brokenFolderName = "Broken";
    
    protected override string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory=null)
    {
        return fileSystemAccessor.CombinePath(baseDirectory ?? basePath, brokenFolderName, interviewId.FormatGuid());
    }
    
    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new System.NotImplementedException();
    }
}
