using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_storing_new_preloaded_data_file : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.OpenOrCreateFile(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(CreateStream());
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.StorePanelData(CreateStream(), "fileName.zip");

        [NUnit.Framework.Test] public void should_return_not_null_result () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_directory_with_result_in_name_be_created () =>
            fileSystemAccessor.Verify(x => x.CreateDirectory(string.Format(@"PreLoadedData\{0}",result)), Times.Once);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static string result;
    }
}
