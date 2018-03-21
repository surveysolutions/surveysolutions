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
    internal class when_preloaded_data_with_single_csv_is_present_and_GetPreloadedDataOfSample_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFileExtension(Moq.It.IsAny<string>())).Returns(".tab");
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + csvFileId, Moq.It.IsAny<bool>())).Returns(new string[] { csvFileName + ".tab" });

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(false);

            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            recordsAccessorFactory.Setup(x => x.CreateRecordsAccessor(Moq.It.IsAny<Stream>(), Moq.It.IsAny<string>()))
                .Returns(Mock.Of<IRecordsAccessor>(_ => _.Records == new string[][] { new string[] { "q1" }, new string[] { "1" }, }));
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataOfSample(csvFileId);


        [NUnit.Framework.Test] public void should_first_pre_loaded_data_name_should_be_test_tab () =>
            result.FileName.Should().Be("test.tab");

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_row () =>
            result.Content.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_element_in_row () =>
            result.Content[0].Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_has_one_element_in__first_row_be_equal_to_1 () =>
           result.Content[0][0].Should().Be("1");

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_header_has_one_column () =>
           result.Header.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_pre_loaded_data_header_has_value_equal_to_q1 () =>
           result.Header[0].Should().Be("q1");

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
