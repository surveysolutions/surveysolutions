using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.CsvDataFileExportServiceTests
{
    internal class when_AddActionRecords_called_for_list_of_actions : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvWriterServiceMock = new Mock<ICsvWriterService>();
            csvDataFileExportService = CreateCsvDataFileExportService(null, csvWriterServiceMock.Object);
        };

        Because of = () =>
            csvDataFileExportService.AddActionRecords(new []{CreateInterviewActionExportView(interviewId,InterviewExportedAction.Completed,originator,timeStamp,someRole)}, "file path");

        It should_write_interviewId_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(interviewId), Times.Once);

        It should_write_Completed_status_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(InterviewExportedAction.Completed), Times.Once);

        It should_write_originator_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField(originator), Times.Once);

        It should_write_date_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("04/18/1984"), Times.Once);

        It should_write_time_once = () =>
            csvWriterServiceMock.Verify(x => x.WriteField("06:10:02"), Times.Once);

        It should_write_role_once = () =>
          csvWriterServiceMock.Verify(x => x.WriteField(someRole), Times.Once);

        private static CsvDataFileExportService csvDataFileExportService;
        private static Mock<ICsvWriterService> csvWriterServiceMock;
        private static readonly string interviewId = "interview id";
        private static readonly string originator = "originator";
        private static readonly DateTime timeStamp = new DateTime(1984, 4, 18, 6, 10, 2, 33);
        private static readonly string someRole = "some role";
    }
}
