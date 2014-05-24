using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_single_csv_is_present_and_GetPreloadedDataMetaInformationForSampleData_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists("PreLoadedData\\" + csvFileId)).Returns(true);

            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + csvFileId)).Returns(new string[] { csvFileName + ".csv" });

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(false);
            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
        };

        Because of = () => result = filebasedPreloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(csvFileId);

        It should_result_has_info_about_1_elements = () =>
            result.FilesMetaInformation.Length.ShouldEqual(1);

        It should_result_has_info_about_first_element_with_name_test_csv = () =>
          result.FilesMetaInformation[0].FileName.ShouldEqual("test.csv");

        It should_first_element_be_marked_and_CanBeHandled = () =>
         result.FilesMetaInformation[0].CanBeHandled.ShouldEqual(true);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedContentMetaData result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string csvFileName = "test";
        private static string preLoadedData = "PreLoadedData";
        private static string csvFileId = Guid.NewGuid().FormatGuid();
    }
}
