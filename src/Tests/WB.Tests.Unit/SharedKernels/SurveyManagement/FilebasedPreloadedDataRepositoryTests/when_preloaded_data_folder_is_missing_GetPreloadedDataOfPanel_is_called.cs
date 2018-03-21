using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_folder_is_missing_GetPreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.OpenOrCreateFile(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(CreateStream());
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataOfPanel(Guid.NewGuid().FormatGuid());

        [NUnit.Framework.Test] public void should_result_has_0_elements () =>
            result.Length.Should().Be(0);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile[] result;
    }
}
