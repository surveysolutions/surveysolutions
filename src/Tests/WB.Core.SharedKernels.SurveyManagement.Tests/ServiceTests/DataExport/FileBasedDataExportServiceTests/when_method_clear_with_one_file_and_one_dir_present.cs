using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    class when_method_clear_with_one_file_and_one_dir_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            filebaseExportDataAccessorMock = new Mock<IFilebasedExportedDataAccessor>();

            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(filebasedExportedDataAccessor: filebaseExportDataAccessorMock.Object);
        };
        
        Because of = () => fileBasedDataExportRepositoryWriter.Clear();

        It should_data_folder_be_cleared = () =>
            filebaseExportDataAccessorMock.Verify(x => x.CleanExportDataFolder(), Times.Once());

        It should_file_folder_be_cleared = () =>
            filebaseExportDataAccessorMock.Verify(x => x.CleanExportDataFolder(), Times.Once());

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Mock<IFilebasedExportedDataAccessor> filebaseExportDataAccessorMock;
    }
}
