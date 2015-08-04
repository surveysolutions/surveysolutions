using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_archive_is_present_with_one_csv_file_GetPreloadedDataMetaInformationForPanelData_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists("PreLoadedData\\" + archiveId)).Returns(true);

            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId)).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(archiveName))
                .Returns(new string[0]);

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(true);
            archiveUtils.Setup(x => x.GetArchivedFileNamesAndSize(Moq.It.IsAny<string>()))
                .Returns(new Dictionary<string, long>() { { "1.tab", 20 },{"nastya",1} });
            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
        };

        Because of = () => result = filebasedPreloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(archiveId);

        It should_result_has_info_about_2_elements = () =>
            result.FilesMetaInformation.Length.ShouldEqual(2);

        It should_result_has_info_about_first_element_with_name_1_tab = () =>
          result.FilesMetaInformation[0].FileName.ShouldEqual("1.tab");

        It should_first_element_be_marked_and_CanBeHandled = () =>
         result.FilesMetaInformation[0].CanBeHandled.ShouldEqual(true);

        It should_result_has_info_about_second_element_with_name_nastya = () =>
         result.FilesMetaInformation[1].FileName.ShouldEqual("nastya");

        It should_second_element_be_marked_and_CanBeHandled = () =>
        result.FilesMetaInformation[1].CanBeHandled.ShouldEqual(false);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedContentMetaData result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string archiveName = "test";
        private static string preLoadedData = "PreLoadedData";
        private static string archiveId = Guid.NewGuid().FormatGuid();
    }
}
