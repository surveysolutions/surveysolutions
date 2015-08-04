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
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_archive_is_present_and_unzipped_with_2_files_GetPreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId)).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(preLoadedData + "\\" + archiveId)).Returns(new string[] { archiveName});
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId + "\\Unzipped"))
                .Returns(new string[] { "1.tab", "2.tab" });
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
            result[0].FileName.ShouldEqual("1.tab");

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

        It should_second_pre_loaded_data_name_should_be_2_tab = () =>
           result[1].FileName.ShouldEqual("2.tab");

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile[] result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string archiveName="test";
        private static string preLoadedData = "PreLoadedData";
        private static string archiveId = Guid.NewGuid().FormatGuid();
    }
}
