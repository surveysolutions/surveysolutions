using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestOf(typeof(InterviewActionsExporter))]
    internal class WhenExportingInterviewActions
    {
        private const string InterviewKey = "11-22-33-44";

        [SetUp]
        public void Setup()
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });


            var commentedStatuses = new List<InterviewSummary>
            {
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.Created),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.SupervisorAssigned),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.InterviewerAssigned),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.Completed),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.RejectedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.Completed),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.Restarted),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.InterviewerAssigned),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.Completed),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.ApprovedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.RejectedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.ApprovedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.ApprovedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
                Create.InterviewSummary(key: InterviewKey, interviewId: interviewId, status: InterviewExportedAction.UnapprovedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
            };

            foreach (var commentedStatus in commentedStatuses)
            {
                commentedStatus.Timestamp = new DateTime(2017, 12, 31, 14, 45, 30);
            }

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireId);
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);

            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((f, data, d) =>
                {
                    fileData.AddRange(data);
                });

            var hqApi = new Mock<IHeadquartersApi>();
            hqApi.SetupIgnoreArgs(x => x.GetInterviewSummariesBatchAsync(null))
                .ReturnsAsync(commentedStatuses);

            actionsExporter = Create.InterviewActionsExporter(csvWriter: csvWriterMock.Object,
                tenantApi: Create.TenantHeadquartersApi(hqApi.Object));
        }

        [TearDown]
        public void Cleanup()
        {
            fileData.Clear();
        }

        [Test]
        public async Task should_export_14_rows_for_statuses_and_the_header()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData.Count, Is.EqualTo(14 /*statuses*/ + 1 /*header*/));
        }

        [Test]
        public async Task should_record_specified_headers()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[0], Is.EqualTo(new []{ "interview__key", "interview__id", "date", "time", "action",
                "originator", "role", "responsible__name", "responsible__role"}));
        }

        [Test]
        public async Task should_record_specified_1_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[1], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222","2017-12-31", "14:45:30", "12",
                "inter", "Interviewer", "inter", "Interviewer"}));
        }

        [Test]
        public async Task should_record_specified_2_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[2], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "0",
                "inter", "Interviewer", "supervisor", "Supervisor" }));
        }

        [Test]
        public async Task should_record_specified_3_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[3], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "1",
                "inter", "Interviewer", "inter", "Interviewer" }));
        }

        [Test]
        public async Task should_record_specified_4_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[4], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "3",
                "inter", "Interviewer", "supervisor", "Supervisor" }));
        }


        [Test]
        public async Task should_record_specified_5_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[5], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "7",
                "supervisor", "Supervisor", "inter", "Interviewer" }));
        }

        [Test]
        public async Task should_record_specified_6_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[6], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "3",
                "inter", "Interviewer", "supervisor", "Supervisor" }));
        }

        [Test]
        public async Task should_record_specified_7_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[7], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "4",
                "inter", "Interviewer", "inter", "Interviewer" }));
        }

        [Test]
        public async Task should_record_specified_8_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[8], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "1",
                "inter", "Interviewer", "inter", "Interviewer" }));
        }

        [Test]
        public async Task should_record_specified_9_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());
            Assert.That(fileData[9], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "3",
                "inter", "Interviewer", "supervisor", "Supervisor" }));
        }

        [Test]
        public async Task should_record_specified_10_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[10], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30",  "5",
                "supervisor", "Supervisor", "any headquarters", "Headquarter" }));
        }

        [Test]
        public async Task should_record_specified_11_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[11], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "8",
                "hq", "Headquarter", "supervisor", "Supervisor" }));
        }

        [Test]
        public async Task should_record_specified_12_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[12], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "5",
                "supervisor", "Supervisor", "any headquarters", "Headquarter" }));
        }

        [Test]
        public async Task should_record_specified_13_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[13], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "6",
                "hq", "Headquarter", "", "" }));
        }

        [Test]
        public async Task should_record_specified_14_row()
        {
            await actionsExporter.ExportAsync(tenant, questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[14], Is.EqualTo(new[] { InterviewKey, "22222222222222222222222222222222", "2017-12-31", "14:45:30", "11",
                "hq", "Headquarter", "supervisor", "Supervisor" }));
        }

        private static InterviewActionsExporter actionsExporter;
        private static string questionnaireId = "questionnaire";
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static long questionnaireVersion = 3;
        private static string fileName = "1.tab";
        private static List<string[]> fileData = new List<string[]>();
        private static readonly QuestionnaireId questionnaireIdentity = new QuestionnaireId(questionnaireId);
        private static TenantInfo tenant = Create.Tenant();
    }
}
