using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class ImageInterviewS3FileStorage : InterviewS3FileStorage, IImageFileStorage
    {
        public ImageInterviewS3FileStorage(IExternalFileStorage externalFileStorage, IFileSystemAccessor fileSystemAccessor)
            :base( externalFileStorage, fileSystemAccessor)
        {
        }

        protected override string GetInterviewDirectoryPath(Guid interviewId) => $"images/{interviewId.FormatGuid()}";
    }
}
