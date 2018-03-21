using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_archive_is_present_with_one_csv_file_GetPreloadedDataMetaInformationForPanelData_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists("PreLoadedData\\" + archiveId)).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFileExtension(tabFileName)).Returns(".tab");
            fileSystemAccessor.Setup(x => x.GetFileExtension(fileNameWithoutExtension)).Returns("");
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId, Moq.It.IsAny<bool>())).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(archiveName, Moq.It.IsAny<bool>()))
                .Returns(new string[0]);

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(true);
            archiveUtils.Setup(x => x.GetArchivedFileNamesAndSize(Moq.It.IsAny<string>()))
                .Returns(new Dictionary<string, long>() { { tabFileName, 20 },{fileNameWithoutExtension,1} });
            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(archiveId);

        [NUnit.Framework.Test] public void should_result_has_info_about_2_elements () =>
            result.FilesMetaInformation.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_result_has_info_about_first_element_with_name_1_tab () =>
          result.FilesMetaInformation[0].FileName.Should().Be(tabFileName);

        [NUnit.Framework.Test] public void should_first_element_be_marked_and_CanBeHandled () =>
         result.FilesMetaInformation[0].CanBeHandled.Should().Be(true);

        [NUnit.Framework.Test] public void should_result_has_info_about_second_element_with_name_nastya () =>
         result.FilesMetaInformation[1].FileName.Should().Be(fileNameWithoutExtension);

        [NUnit.Framework.Test] public void should_second_element_be_marked_and_CanBeHandled () =>
        result.FilesMetaInformation[1].CanBeHandled.Should().Be(false);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedContentMetaData result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string archiveName = "test";
        private static string preLoadedData = "PreLoadedData";
        private static string archiveId = Guid.NewGuid().FormatGuid();
        private static string tabFileName = "1.tab";
        private static string fileNameWithoutExtension = "nastya";
    }
}
