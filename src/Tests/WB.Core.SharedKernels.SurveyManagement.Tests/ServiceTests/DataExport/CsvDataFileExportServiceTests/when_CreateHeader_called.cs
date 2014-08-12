using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.CsvDataFileExportServiceTests
{
    internal class when_CreateHeader_called : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvWriterServiceMock = new Mock<ICsvWriterService>();
            csvDataFileExportService = CreateCsvDataFileExportService(null, csvWriterServiceMock.Object);
        };

        Because of = () =>
            csvDataFileExportService.CreateHeader(
                CreateHeaderStructureForLevel(levelIdColumnName, new[] { referenceName },
                    new Dictionary<Guid, ExportedHeaderItem>() { { Guid.NewGuid(), CreateExportedHeaderItem(new[] { colName }) } },
                    new ValueVector<Guid>(new[] { Guid.NewGuid() })), filePath);

        It should_write_levelIdColumnName_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(levelIdColumnName), Times.Once);

        It should_write_referenceName_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(referenceName), Times.Once);

        It should_write_column_name_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(colName), Times.Once);

        It should_write_ParentId1_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("ParentId1"), Times.Once);

        private static CsvDataFileExportService csvDataFileExportService;
        private static Mock<ICsvWriterService> csvWriterServiceMock;
        private static readonly string levelIdColumnName = "level id col name";
        private static readonly string referenceName = "reference name";
        private static readonly string colName = "col name";
        private static readonly string filePath = "file path";
    }
}
