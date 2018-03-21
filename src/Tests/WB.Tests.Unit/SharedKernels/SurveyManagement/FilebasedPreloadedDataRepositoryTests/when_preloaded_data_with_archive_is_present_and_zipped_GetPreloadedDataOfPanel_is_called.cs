using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_archive_is_present_and_zipped_GetPreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists("PreLoadedData\\" + archiveId)).Returns(true);

            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId, Moq.It.IsAny<bool>())).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(preLoadedData + "\\" + archiveId)).Returns(new string[] { archiveName });
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(archiveName, Moq.It.IsAny<bool>()))
                .Returns(new string[0]);

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(true);

            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataOfPanel(archiveId);

        [NUnit.Framework.Test] public void should_result_has_0_elements () =>
            result.Length.Should().Be(0);

        [NUnit.Framework.Test] public void should_archive_be_unziped_once () =>
            archiveUtils.Verify(x => x.Unzip(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Once);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile[] result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string archiveName = "test";
        private static string preLoadedData = "PreLoadedData";
        private static string archiveId = Guid.NewGuid().FormatGuid();
    }
}
