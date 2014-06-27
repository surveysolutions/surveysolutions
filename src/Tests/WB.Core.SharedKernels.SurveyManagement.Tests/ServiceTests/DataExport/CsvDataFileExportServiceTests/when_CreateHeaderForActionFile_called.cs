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
    internal class when_CreateHeaderForActionFile_called : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvWriterServiceMock = new Mock<ICsvWriterService>();
            csvDataFileExportService = CreateCsvDataFileExportService(null, csvWriterServiceMock.Object);
        };

        Because of = () =>
            csvDataFileExportService.CreateHeaderForActionFile(filePath);

        It should_write_Id_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("Id"), Times.Once);

        It should_write_Action_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("Action"), Times.Once);

        It should_write_Originator_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("Originator"), Times.Once);

        It should_write_Role_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("Role"), Times.Once);

        It should_write_Date_once = () =>
          csvWriterServiceMock.Verify(x => x.WriteField("Date"), Times.Once);

        It should_write_Time_once = () =>
          csvWriterServiceMock.Verify(x => x.WriteField("Time"), Times.Once);

        private static CsvDataFileExportService csvDataFileExportService;
        private static Mock<ICsvWriterService> csvWriterServiceMock;
        private static readonly string filePath = "file path";
    }
}
