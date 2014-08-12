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
    internal class when_AddRecord_called_for_interview_export_view : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvWriterServiceMock=new Mock<ICsvWriterService>();
            csvDataFileExportService = CreateCsvDataFileExportService(null, csvWriterServiceMock.Object);
        };

        Because of = () =>
            csvDataFileExportService.AddRecord(
                CreateInterviewDataExportLevelView(new[]
                {
                    CreateInterviewDataExportRecord(recordId,new[] { CreateExportedQuestion(new[] { answer }) }, new[] { reference },
                        new[] { parentLevel })
                }), "file path");

        It should_write_record_id_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(recordId), Times.Once);

        It should_write_reference_value_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(reference), Times.Once);

        It should_write_answer_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(answer), Times.Once);

        It should_write_parent_level_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(parentLevel), Times.Once);

        private static CsvDataFileExportService csvDataFileExportService;
        private static Mock<ICsvWriterService> csvWriterServiceMock;
        private static readonly string recordId = "record Id";
        private static readonly string reference = "reference";
        private static readonly string answer = "answer";
        private static readonly string parentLevel = "parent level";
    }
}
