using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    [Subject(typeof(FilebasedPreloadedDataRepository))]
    internal class FilebasedPreloadedDataRepositoryTestContext
    {
        protected static FilebasedPreloadedDataRepository CreateFilebasedPreloadedDataRepository(
            IFileSystemAccessor fileSystemAccessor = null, IArchiveUtils archiveUtils = null,
            IRecordsAccessorFactory recordsAccessorFactory = null)
        {
            return new FilebasedPreloadedDataRepository(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), "",
                archiveUtils ?? Mock.Of<IArchiveUtils>(), recordsAccessorFactory ?? Mock.Of<IRecordsAccessorFactory>());
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>()))
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
