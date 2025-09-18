using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;

public class S3BrokenImageFileStorage : S3ImageFileStorage, IBrokenImageFileStorage
{
    public S3BrokenImageFileStorage(IExternalFileStorage externalFileStorage, IFileSystemAccessor fileSystemAccessor)
        :base(externalFileStorage, fileSystemAccessor)
    {
    }
        
    protected override string GetInterviewDirectoryPath(Guid interviewId) => $"images/broken/{interviewId.FormatGuid()}";
        
    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
