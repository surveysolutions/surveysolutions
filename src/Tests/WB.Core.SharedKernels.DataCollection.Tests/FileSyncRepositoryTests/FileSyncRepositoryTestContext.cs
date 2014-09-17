using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.FileSyncRepositoryTests
{
    [Subject(typeof(FileSyncRepository))]
    internal class FileSyncRepositoryTestContext
    {
        protected static FileSyncRepository CreateFileSyncRepository(IPlainFileRepository plainFileRepository = null, IFileSystemAccessor fileSystemAccessor=null)
        {
            return new FileSyncRepository(plainFileRepository ?? Mock.Of<IPlainFileRepository>(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), "");
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
