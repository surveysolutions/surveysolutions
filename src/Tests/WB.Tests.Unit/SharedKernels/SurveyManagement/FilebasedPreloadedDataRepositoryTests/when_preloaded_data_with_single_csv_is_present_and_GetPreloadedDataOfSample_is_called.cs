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
    internal class when_preloaded_data_with_single_csv_is_present_and_GetPreloadedDataOfSample_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + csvFileId)).Returns(new string[] { csvFileName + ".tab" });

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(false);

            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactory.Setup(x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][] { new string[] { "q1" }, new string[] { "1" }, }));
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
        };

        Because of = () => result = filebasedPreloadedDataRepository.GetPreloadedDataOfSample(csvFileId);


        It should_first_pre_loaded_data_name_should_be_test_tab = () =>
            result.FileName.ShouldEqual("test.tab");

        It should_first_pre_loaded_data_has_one_row = () =>
            result.Content.Length.ShouldEqual(1);

        It should_first_pre_loaded_data_has_one_element_in_row = () =>
            result.Content[0].Length.ShouldEqual(1);

        It should_first_pre_loaded_data_has_one_element_in__first_row_be_equal_to_1 = () =>
           result.Content[0][0].ShouldEqual("1");

        It should_first_pre_loaded_data_header_has_one_column = () =>
           result.Header.Length.ShouldEqual(1);

        It should_first_pre_loaded_data_header_has_value_equal_to_q1 = () =>
           result.Header[0].ShouldEqual("q1");

        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static PreloadedDataByFile result;
        private static Mock<IArchiveUtils> archiveUtils;
        private static Mock<IRecordsAccessorFactory> recordsAccessorFactory;
        private static string csvFileName = "test";
        private static string preLoadedData = "PreLoadedData";
        private static string csvFileId = Guid.NewGuid().FormatGuid();
    }
}
