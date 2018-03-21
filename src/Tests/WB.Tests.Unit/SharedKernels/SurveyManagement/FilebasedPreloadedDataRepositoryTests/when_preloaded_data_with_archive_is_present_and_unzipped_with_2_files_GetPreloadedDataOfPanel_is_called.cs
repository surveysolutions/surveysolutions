using System;
using System.IO;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_archive_is_present_and_unzipped_with_2_files_GetPreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
         [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId, Moq.It.IsAny<bool>())).Returns(new string[] { archiveName + ".zip" });
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(preLoadedData + "\\" + archiveId)).Returns(new string[] { archiveName});
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + archiveId + "\\Unzipped", Moq.It.IsAny<bool>()))
                .Returns(new string[] { "1.tab", "2.tab" });
            fileSystemAccessor.Setup(x => x.GetFileExtension(Moq.It.IsAny<string>())).Returns(".tab");
            archiveUtils=new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(true);

            recordsAccessorFactory=new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactory.Setup(x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][] { new string[] { "q1" }, new string[] { "1" }, }));
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataOfPanel(archiveId);

        [NUnit.Framework.Test] public void should_result_has_2_elements () =>
            result.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_name_should_be_1_tab () =>
            result[0].FileName.Should().Be("1.tab");

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_row () =>
            result[0].Content.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_element_in_row () =>
            result[0].Content[0].Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_element_in__first_row_be_equal_to_1 () =>
           result[0].Content[0][0].Should().Be("1");

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_header_has_one_column() =>
           result[0].Header.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_header_has_value_equal_to_q1 () =>
           result[0].Header[0].Should().Be("q1");

        [NUnit.Framework.Test] public void should_second_pre_loaded_data_name_should_be_2_tab () =>
           result[1].FileName.Should().Be("2.tab");

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
