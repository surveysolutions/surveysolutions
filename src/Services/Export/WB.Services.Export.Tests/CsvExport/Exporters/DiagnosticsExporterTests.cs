using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestFixture]
    internal class DiagnosticsExporterTests
    {
        [Test]
        public async Task when_exporting_interview_diagnostics()
        {
            var interviewId1 = Guid.Parse("11111111111111111111111111111111");
            var interviewId2 = Guid.Parse("22222222222222222222222222222222");
            var interviewId3 = Guid.Parse("33333333333333333333333333333333");

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(new[] { "1.tab", "2.txt" });

            List<InterviewDiagnosticsInfo> diagnosticsInfo = new List<InterviewDiagnosticsInfo>
            {
                Create.InterviewDiagnosticsInfo(
                    interviewId1,
                    interviewKey: "key1",
                    status: InterviewStatus.InterviewerAssigned,
                    responsibleName : "int1",
                    numberOfInterviewers: 1,
                    numberRejectionsBySupervisor: 2,
                    numberRejectionsByHq: 3,
                    numberValidQuestions: 4,
                    numberInvalidEntities: 5,
                    numberUnansweredQuestions: 6,
                    numberCommentedQuestions: 7,
                    interviewDuration: 100000),
                Create.InterviewDiagnosticsInfo(
                    interviewId2,
                    interviewKey: "key2",
                    status: InterviewStatus.RejectedByHeadquarters,
                    responsibleName : "int2",
                    numberOfInterviewers: 9,
                    numberRejectionsBySupervisor: 8,
                    numberRejectionsByHq: 7,
                    numberValidQuestions: 6,
                    numberInvalidEntities: 5,
                    numberUnansweredQuestions: 4,
                    numberCommentedQuestions: 3,
                    interviewDuration: 20000000),
                Create.InterviewDiagnosticsInfo(
                    interviewId3,
                    interviewKey: "key3",
                    status: InterviewStatus.RejectedBySupervisor,
                    responsibleName : "int1",
                    numberOfInterviewers: 2,
                    numberRejectionsBySupervisor: 4,
                    numberRejectionsByHq: 6,
                    numberValidQuestions: 8,
                    numberInvalidEntities: 9,
                    numberUnansweredQuestions: 1,
                    numberCommentedQuestions: 2,
                    interviewDuration: 1758237000),
            };

            var hqApi = new Mock<IHeadquartersApi>();
            hqApi.SetupIgnoreArgs(x => x.GetInterviewDiagnosticsInfoBatchAsync(null))
                .ReturnsAsync(diagnosticsInfo.ToArray);

            var fileData = new List<string[]>();

            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((f, data, d) =>
                {
                    fileData.AddRange(data);
                });

            var exporter = Create.DiagnosticsExporter(csvWriter: csvWriterMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object,
                headquartersApi: Create.TenantHeadquartersApi(hqApi.Object));


            await exporter.ExportAsync(new List<Guid>() { interviewId1, interviewId2, interviewId3 }, "", Create.Tenant(), new Progress<int>(), CancellationToken.None);

            Assert.That(fileData.Count, Is.EqualTo(3 /*interviews*/ + 1 /*header*/));
            Assert.That(fileData[0], Is.EqualTo(new[] { "interview__key", "interview__id", "interview__status", "responsible", "interviewers", "rejections__sup", "rejections__hq", "entities__errors", "questions__comments", "interview__duration" }));
            Assert.That(fileData[1], Is.EqualTo(new[] { "key1", interviewId1.ToString() ,"InterviewerAssigned", "int1", "1", "2", "3", "5", "7", "00.00:00:00" }));
            Assert.That(fileData[2], Is.EqualTo(new[] { "key2", interviewId2.ToString(), "RejectedByHeadquarters", "int2", "9", "8", "7", "5", "3", "00.00:00:02" }));
            Assert.That(fileData[3], Is.EqualTo(new[] { "key3", interviewId3.ToString(), "RejectedBySupervisor", "int1", "2", "4", "6", "9", "2", "00.00:02:55" }));
        }
    }
}
