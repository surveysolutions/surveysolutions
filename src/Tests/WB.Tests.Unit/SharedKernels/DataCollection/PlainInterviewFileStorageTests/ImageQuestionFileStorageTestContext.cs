using System.IO;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    [NUnit.Framework.TestOf(typeof(ImageFileStorage))]
    class ImageQuestionFileStorageTestContext
    {
        protected static ImageFileStorage CreatePlainFileRepository(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new ImageFileStorage(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, Options.Create(new FileStorageConfig()));
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
