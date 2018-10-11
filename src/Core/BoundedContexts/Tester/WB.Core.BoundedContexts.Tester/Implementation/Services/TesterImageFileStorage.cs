using System;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterImageFileStorage : TesterBaseFileStorage, IImageFileStorage
    {
        protected override string DataDirectoryName => "InterviewData";
        protected override string EntityDirectoryName => "TempInterviewData";

        public TesterImageFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath) 
            : base(fileSystemAccessor, rootDirectoryPath)
        {
        }

        public string GetPath(Guid interviewId, string filename = null)
        {
            throw new NotImplementedException();
        }
    }
}
