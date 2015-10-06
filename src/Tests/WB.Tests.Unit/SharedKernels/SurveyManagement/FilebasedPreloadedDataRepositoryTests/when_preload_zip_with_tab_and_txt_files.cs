using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preload_zip_with_tab_and_txt_files : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            var pathToParentDirectoryOfZipFile = preLoadedData + "\\" + archiveId;
            var pathToUnzippedDirectory = pathToParentDirectoryOfZipFile + "\\Unzipped";

            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(pathToParentDirectoryOfZipFile)).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(pathToParentDirectoryOfZipFile)).Returns(new string[] { archiveName});
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(pathToUnzippedDirectory))
                .Returns(new [] { tabFileName, txtFileName });
            fileSystemAccessor.Setup(x => x.GetFileExtension(tabFileName)).Returns(".tab");
            fileSystemAccessor.Setup(x => x.GetFileExtension(txtFileName)).Returns(".txt");
            archiveUtils=new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(true);

            recordsAccessorFactory=new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactory.Setup(x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][] { new string[] { "q1" }, new string[] { "1" }, }));
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
        };

        Because of = () => result = filebasedPreloadedDataRepository.GetPreloadedDataOfPanel(archiveId);

        It should_result_has_2_elements = () =>
            result.Length.ShouldEqual(2);

        It should_first_pre_loaded_data_name_should_be_1_tab = () =>
            result[0].FileName.ShouldEqual(tabFileName);

        It should_first_pre_loaded_data_has_one_row = () =>
            result[0].Content.Length.ShouldEqual(1);

        It should_first_pre_loaded_data_has_one_element_in_row = () =>
            result[0].Content[0].Length.ShouldEqual(1);

        It should_first_pre_loaded_data_has_one_element_in__first_row_be_equal_to_1 = () =>
           result[0].Content[0][0].ShouldEqual("1");

        It should_first_pre_loaded_data_header_has_one_column= () =>
           result[0].Header.Length.ShouldEqual(1);

        It should_first_pre_loaded_data_header_has_value_equal_to_q1 = () =>
           result[0].Header[0].ShouldEqual("q1");

        It should_second_pre_loaded_data_name_should_be_2_txt = () =>
           result[1].FileName.ShouldEqual(txtFileName);

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile[] result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string archiveName="test";
        private static string preLoadedData = "PreLoadedData";
        private static string tabFileName = "1.tab";
        private static string txtFileName = "2.txt";
        private static string archiveId = Guid.NewGuid().FormatGuid();
    }
}
