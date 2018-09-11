using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.InterviewActionsExporterTests
{
    [TestOf(typeof(InterviewActionsExporter))]
    internal class WhenExportingInterviewActions
    {
        [SetUp]
        public void Setup()
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<bool>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns<string, string>(Path.Combine);

            var interviewStatuses = new TestInMemoryWriter<InterviewSummary>();

            var commentedStatuses = new[]
            {
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Created),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.SupervisorAssigned),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.RejectedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Restarted),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.ApprovedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.RejectedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.ApprovedBySupervisor, originatorName: "supervisor", originatorRole: UserRoles.Supervisor),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.ApprovedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
                Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.UnapprovedByHeadquarter, originatorName: "hq", originatorRole: UserRoles.Headquarter),
            };

            commentedStatuses.ForEach(x => x.Timestamp = new DateTime(2017, 12, 31, 14, 45, 30));

            interviewStatuses.Store(
                Create.Entity.InterviewSummary(interviewId: interviewId, key: "11-22-33-44", questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion, statuses: commentedStatuses),
                interviewId.FormatGuid());


            var questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure(questionnaireId, questionnaireVersion);
            var headerStructureForLevel = Create.Entity.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);

            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((f, data, d) =>
                {
                    fileData.AddRange(data);
                });

            actionsExporter = Create.Service.InterviewActionsExporter(csvWriter: csvWriterMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object,
                interviewStatuses: interviewStatuses,
                questionnaireExportStructure: questionnaireExportStructure);
        }

        [TearDown]
        public void Cleanup()
        {
            fileData.Clear();
        }

        [Test]
        public void should_export_14_rows_for_statuses_and_the_header()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData.Count, Is.EqualTo(14 /*statuses*/ + 1 /*header*/));
        }

        [Test]
        public void should_record_specified_headers()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[0], Is.EqualTo(new []{ "interview__id", "interview__key", "Action", "Originator", "Role", "ResponsibleName", "ResponsibleRole", "Date", "Time" }));
        }

        [Test]
        public void should_record_specified_1_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[1], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "Created", "inter", "Interviewer", "inter", "Interviewer", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_2_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[2], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "SupervisorAssigned", "inter", "Interviewer", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_3_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[3], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "InterviewerAssigned", "inter", "Interviewer", "inter", "Interviewer", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_4_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[4], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "Completed", "inter", "Interviewer", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }


        [Test]
        public void should_record_specified_5_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[5], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "RejectedBySupervisor", "supervisor", "Supervisor", "inter", "Interviewer", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_6_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[6], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "Completed", "inter", "Interviewer", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_7_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[7], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "Restarted", "inter", "Interviewer", "inter", "Interviewer", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_8_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[8], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "InterviewerAssigned", "inter", "Interviewer", "inter", "Interviewer", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_9_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());
            Assert.That(fileData[9], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "Completed", "inter", "Interviewer", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_10_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[10], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "ApprovedBySupervisor", "supervisor", "Supervisor", Strings.AnyHeadquarters, "Headquarter", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_11_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[11], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "RejectedByHeadquarter", "hq", "Headquarter", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_12_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[12], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "ApprovedBySupervisor", "supervisor", "Supervisor", Strings.AnyHeadquarters, "Headquarter", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_13_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[13], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "ApprovedByHeadquarter", "hq", "Headquarter", "", "", "2017-12-31", "14:45:30" }));
        }

        [Test]
        public void should_record_specified_14_row()
        {
            actionsExporter.Export(questionnaireIdentity, new List<Guid> { interviewId }, "", new Progress<int>());

            Assert.That(fileData[14], Is.EqualTo(new[] { "22222222222222222222222222222222", "11-22-33-44", "UnapprovedByHeadquarter", "hq", "Headquarter", "supervisor", "Supervisor", "2017-12-31", "14:45:30" }));
        }

        private static InterviewActionsExporter actionsExporter;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static long questionnaireVersion = 3;
        private static string fileName = "1.tab";
        private static List<string[]> fileData = new List<string[]>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
    }
}
