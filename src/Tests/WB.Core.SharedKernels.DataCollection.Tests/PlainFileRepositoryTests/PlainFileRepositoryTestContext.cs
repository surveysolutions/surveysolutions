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

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    [Subject(typeof(PlainFileRepository))]
    class PlainFileRepositoryTestContext
    {
        protected static PlainFileRepository CreatePlainFileRepository(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new PlainFileRepository(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, "");
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
