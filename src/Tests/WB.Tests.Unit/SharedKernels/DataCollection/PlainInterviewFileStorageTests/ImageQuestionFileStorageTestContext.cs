using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    [Subject(typeof(ImageQuestionFileStorage))]
    class ImageQuestionFileStorageTestContext
    {
        protected static ImageQuestionFileStorage CreatePlainFileRepository(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new ImageQuestionFileStorage(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, "");
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
