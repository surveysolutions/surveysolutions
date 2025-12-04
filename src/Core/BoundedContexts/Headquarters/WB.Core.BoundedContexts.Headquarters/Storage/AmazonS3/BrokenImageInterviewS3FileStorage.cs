using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;

public class BrokenImageInterviewS3FileStorage : InterviewS3FileStorage, IBrokenImageFileStorage
{
    public BrokenImageInterviewS3FileStorage(IExternalFileStorage externalFileStorage, IFileSystemAccessor fileSystemAccessor)
        :base(externalFileStorage, fileSystemAccessor)
    {
    }
        
    protected override string GetInterviewDirectoryPath(Guid interviewId) => $"broken_interview_data/images/{interviewId.FormatGuid()}";
    
    protected override string GetContentType(string filename) => ContentTypeHelper.GetImageContentType(filename);
        
    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
