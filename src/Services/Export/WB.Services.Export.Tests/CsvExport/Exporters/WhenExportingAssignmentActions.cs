using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Assignment;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.User;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestOf(typeof(AssignmentActionsExporter))]
    internal class WhenExportingAssignmentActions
    {
        [SetUp]
        public void Setup()
        {
            dbContext = Create.TenantDbContext();

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(new[] {fileName, "2.txt"});

            var timestampUtc = new DateTime(2019, 9, 20, 11, 15, 30, DateTimeKind.Utc);

            var assignmentActions = new List<AssignmentAction>
            {
                Create.AssignmentAction(1, AssignmentId, timestampUtc, AssignmentExportedAction.Created, headquarters,
                    supervisor, comment: "created"),
                Create.AssignmentAction(2, AssignmentId, timestampUtc, AssignmentExportedAction.QuantityChanged,
                    headquarters, supervisor, oldValue: "1", newValue: "20"),
                Create.AssignmentAction(3, AssignmentId, timestampUtc, AssignmentExportedAction.Reassigned, supervisor,
                    interviewer, comment: "do it"),
                Create.AssignmentAction(4, AssignmentId, timestampUtc, AssignmentExportedAction.ReceivedByTablet,
                    supervisor, interviewer),
                Create.AssignmentAction(5, AssignmentId, timestampUtc, AssignmentExportedAction.AudioRecordingChanged,
                    headquarters, interviewer, "0", "1"),
                Create.AssignmentAction(6, AssignmentId, timestampUtc, AssignmentExportedAction.Archived, headquarters,
                    interviewer),
                Create.AssignmentAction(7, AssignmentId, timestampUtc, AssignmentExportedAction.Unarchived,
                    headquarters, interviewer),
                Create.AssignmentAction(8, AssignmentId, timestampUtc, AssignmentExportedAction.WebModeChanged,
                    headquarters, interviewer, "0", "1"),
                Create.AssignmentAction(9, AssignmentId, timestampUtc, AssignmentExportedAction.Deleted, headquarters,
                    interviewer),
            };

            foreach (var assignmentAction in assignmentActions)
                dbContext.Add(assignmentAction);
            dbContext.Assignments.Add(new Assignment.Assignment()
            {
                Id = AssignmentId,
                QuestionnaireId = QuestionnaireId
            });
            dbContext.SaveChanges();

            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((f, data, d) => { fileData.AddRange(data); });

            var userStorage = Mock.Of<IUserStorage>(s =>
                s.GetUserNameAsync(headquarters) == Task.FromResult("headquarters")
                && s.GetUserNameAsync(supervisor) == Task.FromResult("supervisor")
                && s.GetUserNameAsync(interviewer) == Task.FromResult("interviewer")
                && s.GetUserRoleAsync(headquarters) == Task.FromResult(UserRoles.Headquarter)
                && s.GetUserRoleAsync(supervisor) == Task.FromResult(UserRoles.Supervisor)
                && s.GetUserRoleAsync(interviewer) == Task.FromResult(UserRoles.Interviewer)
            );

            actionsExporter = Create.AssignmentActionsExporter(csvWriter: csvWriterMock.Object,
                userStorage: userStorage,
                dbContext: dbContext);
        }

        [TearDown]
        public void Cleanup()
        {
            fileData.Clear();
            dbContext.Dispose();
        }

        [Test]
        public async Task should_export_10_rows_for_statuses_and_the_header()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData.Count, Is.EqualTo(9 /*statuses*/ + 1 /*header*/));
        }

        [Test]
        public async Task should_record_specified_headers()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[0], Is.EqualTo(new[]
            {
                "assignment__id", "date", "time", "action",
                "originator", "role", "responsible__name", "responsible__role", "old__value", "new__value", "comment"
            }));
        }

        [Test]
        public async Task should_record_info_about_create()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[1],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "1", "headquarters", "3", "supervisor", "2",
                    null, null, "created"
                }));
        }

        [Test]
        public async Task should_record_about_quantity_change()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[2],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "8", "headquarters", "3", "supervisor", "2", "1",
                    "20", null
                }));
        }

        [Test]
        public async Task should_record__about_reassign()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[3],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "7", "supervisor", "2", "interviewer", "1", null,
                    null, "do it"
                }));
        }

        [Test]
        public async Task should_record_about_ReceivedByTablet()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[4],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "4", "supervisor", "2", "interviewer", "1", null,
                    null, null
                }));
        }


        [Test]
        public async Task should_record_about_AudioRecordingChanged()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[5],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "6", "headquarters", "3", "interviewer", "1",
                    "0", "1", null
                }));
        }

        [Test]
        public async Task should_record_about_archive()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[6],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "2", "headquarters", "3", "interviewer", "1",
                    null, null, null
                }));
        }

        [Test]
        public async Task should_record_about_unarchive()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[7],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "5", "headquarters", "3", "interviewer", "1",
                    null, null, null
                }));
        }

        [Test]
        public async Task should_record_about_web_mode_change()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);

            Assert.That(fileData[8],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "9", "headquarters", "3", "interviewer", "1",
                    "0", "1", null
                }));
        }

        [Test]
        public async Task should_record_about_delete()
        {
            await actionsExporter.ExportAsync(assignmentIds, tenant, "", new ExportProgress(), CancellationToken.None);
            Assert.That(fileData[9],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "3", "headquarters", "3", "interviewer", "1",
                    null, null, null
                }));
        }

        [Test]
        public async Task should_be_able_to_export_all()
        {
            await actionsExporter.ExportAllAsync(tenant, new QuestionnaireId(QuestionnaireId), "", new ExportProgress(),
                CancellationToken.None);
            Assert.That(fileData[1],
                Is.EqualTo(new[]
                {
                    AssignmentId.ToString(), "2019-09-20", "11:15:30", "1", "headquarters", "3", "supervisor", "2",
                    null, null, "created"
                }));
        }

        private static int AssignmentId = 77;
        private static readonly Guid headquarters = Id.g1;
        private static readonly Guid supervisor = Id.g2;
        private static readonly Guid interviewer = Id.g3;
        private static readonly List<int> assignmentIds = new List<int> {AssignmentId};
        private static TenantDbContext dbContext;
        private static IAssignmentActionsExporter actionsExporter;
        private static string fileName = "1.tab";
        private static readonly List<string[]> fileData = new List<string[]>();
        private static readonly TenantInfo tenant = Create.Tenant();
        private string QuestionnaireId = Id.gA.ToString("N") + "$56";
    }
}
