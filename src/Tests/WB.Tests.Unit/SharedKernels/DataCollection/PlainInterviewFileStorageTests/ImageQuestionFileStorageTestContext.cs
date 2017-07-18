using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    [Subject(typeof(ImageFileStorage))]
    class ImageQuestionFileStorageTestContext
    {
        protected static ImageFileStorage CreatePlainFileRepository(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new ImageFileStorage(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, "");
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
