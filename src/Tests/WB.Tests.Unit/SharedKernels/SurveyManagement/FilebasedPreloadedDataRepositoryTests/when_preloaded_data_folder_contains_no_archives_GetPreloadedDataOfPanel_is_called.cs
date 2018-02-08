using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_folder_contains_no_archives_GetPreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository();
        };

        Because of = () => result = filebasedPreloadedDataRepository.GetPreloadedDataOfPanel();

        It should_result_has_0_elements = () =>
            result.Length.ShouldEqual(0);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile[] result;
    }
}
