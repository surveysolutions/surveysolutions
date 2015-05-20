using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_storing_new_preloaded_data_file : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.OpenOrCreateFile(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(CreateStream());
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object);
        };

        Because of = () => result = filebasedPreloadedDataRepository.Store(CreateStream(), "fileName.zip");

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private It should_directory_with_result_in_name_be_created = () =>
            fileSystemAccessor.Verify(x => x.CreateDirectory(string.Format(@"PreLoadedData\{0}",result)), Times.Once);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static string result;
    }
}
