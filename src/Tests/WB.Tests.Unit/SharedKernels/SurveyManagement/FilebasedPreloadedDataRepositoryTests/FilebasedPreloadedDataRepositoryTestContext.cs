using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    [Subject(typeof(FilebasedPreloadedDataRepository))]
    internal class FilebasedPreloadedDataRepositoryTestContext
    {
        protected static FilebasedPreloadedDataRepository CreateFilebasedPreloadedDataRepository(
            IArchiveUtils archiveUtils = null, string folderPath = "",
            IAssignmentsImportService assignmentsImportService = null)
        {
            return new FilebasedPreloadedDataRepository(folderPath,
                archiveUtils ?? Mock.Of<IArchiveUtils>(), 
                assignmentsImportService ?? Mock.Of<IAssignmentsImportService>());
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.MakeStataCompatibleFileName(Moq.It.IsAny<string>()))
                .Returns<string>(name => name);
            fileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>()))
               .Returns<string>(Path.GetFileName);
            return fileSystemAccessorMock;
        }

        protected static Stream CreateStream()
        {
            return new MemoryStream();
        }
    }
}
