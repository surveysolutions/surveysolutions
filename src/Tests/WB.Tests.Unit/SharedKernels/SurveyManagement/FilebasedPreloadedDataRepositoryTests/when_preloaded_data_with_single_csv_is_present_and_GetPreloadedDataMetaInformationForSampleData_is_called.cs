using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_preloaded_data_with_single_csv_is_present_and_GetPreloadedDataMetaInformationForSampleData_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists("PreLoadedData\\" + csvFileId)).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFileExtension(Moq.It.IsAny<string>())).Returns(".tab");
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(preLoadedData + "\\" + csvFileId, Moq.It.IsAny<bool>())).Returns(new string[] { csvFileName + ".tab" });

            archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.IsZipFile(Moq.It.IsAny<string>())).Returns(false);
            recordsAccessorFactory = new Mock<IRecordsAccessorFactory>();
            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object, archiveUtils.Object, recordsAccessorFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = filebasedPreloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(csvFileId);

        [NUnit.Framework.Test] public void should_result_has_info_about_1_elements () =>
            result.FilesMetaInformation.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_result_has_info_about_first_element_with_name_test_tab () =>
          result.FilesMetaInformation[0].FileName.Should().Be("test.tab");

        [NUnit.Framework.Test] public void should_first_element_be_marked_and_CanBeHandled () =>
         result.FilesMetaInformation[0].CanBeHandled.Should().Be(true);

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
