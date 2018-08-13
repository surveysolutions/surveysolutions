using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport
{
    [TestFixture]
    internal class DiagnosticsExporterTests
    {
        [Test]
        public void when_exporting_interview_diagnostics()
        {
            var interviewId1 = Guid.Parse("11111111111111111111111111111111");
            var interviewId2 = Guid.Parse("22222222222222222222222222222222");
            var interviewId3 = Guid.Parse("33333333333333333333333333333333");

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<bool>())).Returns(new[] { "1.tab", "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(Path.Combine);

            var diagnosticsInfo = new List<InterviewDiagnosticsInfo>
            {
                Create.Entity.InterviewDiagnosticsInfo(
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
                Create.Entity.InterviewDiagnosticsInfo(
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
                    interviewDuration: 200000),
                Create.Entity.InterviewDiagnosticsInfo(
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

            var diagnosticsFactory = new Mock<IInterviewDiagnosticsFactory>();
            diagnosticsFactory.Setup(x => x.GetByBatchIds(It.IsAny<IEnumerable<Guid>>())).Returns(diagnosticsInfo);

            var fileData = new List<string[]>();

            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((f, data, d) =>
                {
                    fileData.AddRange(data);
                });

            var exporter = Create.Service.DiagnisticsExporter(csvWriter: csvWriterMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object,
                diagnosticsFactory: diagnosticsFactory.Object);


            exporter.Export(new List<Guid>() { interviewId1, interviewId2, interviewId3 }, "", new Progress<int>(), CancellationToken.None);

            Assert.That(fileData.Count, Is.EqualTo(3 /*interviews*/ + 1 /*header*/));
            Assert.That(fileData[0], Is.EqualTo(new[] { "interview__key", "interview_status", "responsible", "n_of_Interviewers", "n_rejections_by_supervisor", "n_rejections_by_hq", "n_entities_errors", "n_questions_comments", "interview_duration" }));
            Assert.That(fileData[1], Is.EqualTo(new[] { "key1", "InterviewerAssigned", "int1", "1", "2", "3", "5", "7", "00:00:00.0100000" }));
            Assert.That(fileData[2], Is.EqualTo(new[] { "key2", "RejectedByHeadquarters", "int2", "9", "8", "7", "5", "3", "00:00:00.0200000" }));
            Assert.That(fileData[3], Is.EqualTo(new[] { "key3", "RejectedBySupervisor", "int1", "2", "4", "6", "9", "2", "00:02:55.8237000" }));
        }
    }
}
