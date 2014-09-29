using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainInterviewFileStorageTests
{
    [Subject(typeof(PlainInterviewFileStorage))]
    class PlainInterviewFileStorageTestContext
    {
        protected static PlainInterviewFileStorage CreatePlainFileRepository(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new PlainInterviewFileStorage(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, "", "SYNC");
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return result;
        }
    }
}
