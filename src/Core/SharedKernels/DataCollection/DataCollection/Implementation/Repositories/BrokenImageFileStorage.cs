using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

public class BrokenImageFileStorage : InterviewFileStorage, IImageFileStorage, IBrokenImageFileStorage
{
    public BrokenImageFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath)
        : base(fileSystemAccessor, rootDirectoryPath)
    {
    }
        
    private readonly string brokenFolderName = "BrokenInterviewData";
    private readonly string imagesFolderName = "images";
    
    protected override string ContentType => "image/jpg";
    
    protected override string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory)
    {
        return fileSystemAccessor.CombinePath(baseDirectory, brokenFolderName, imagesFolderName, interviewId.FormatGuid());
    }
    
    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new System.NotImplementedException();
    }
}
