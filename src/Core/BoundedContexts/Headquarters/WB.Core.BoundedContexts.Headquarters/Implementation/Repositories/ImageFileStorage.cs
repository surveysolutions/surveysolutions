using System;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class ImageFileStorage : InterviewFileStorage, IImageFileStorage
    {
        private const string DataDirectoryName = "InterviewData";

        public ImageFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath) 
            : base(fileSystemAccessor, rootDirectoryPath)
        {
        }
        
        protected override string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory)
        {
            return fileSystemAccessor.CombinePath(baseDirectory, DataDirectoryName, interviewId.FormatGuid());
        }

        protected override string GetContentType(string filename) => ContentTypeHelper.GetImageContentType(filename);
    }
}
