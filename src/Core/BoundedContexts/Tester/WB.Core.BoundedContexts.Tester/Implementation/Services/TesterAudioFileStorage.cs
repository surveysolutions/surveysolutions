using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterAudioFileStorage : TesterBaseFileStorage, IAudioFileStorage, IPlainFileCleaner
    {
        protected override string DataDirectoryName => "InterviewData";
        protected override string EntityDirectoryName => "TempAudioInterviewData";

        public TesterAudioFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath) 
            : base(fileSystemAccessor, rootDirectoryPath)
        {
        }
    }
}