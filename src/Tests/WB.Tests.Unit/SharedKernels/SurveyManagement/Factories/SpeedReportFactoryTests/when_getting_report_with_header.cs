using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [TestOf(typeof(SpeedReportFactory))]
    internal class when_getting_report_with_header : SpeedReportFactoryTestContext
    {
        [Test]
        public void should_properly_display_header()
        {
            var reportStartDate = new DateTime(2010, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var supervisorId = Id.g1;
            var input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, from: reportStartDate, period: "m", columnCount: 1);

            var interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            var averageForTargetSupervisor = 20;
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(interviewerId: Id.g2,
                            supervisorId: supervisorId,
                            timestamp: input.From.Date.AddHours(1),
                            status: InterviewExportedAction.Completed,
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(averageForTargetSupervisor)),
                        Create.Entity.InterviewCommentedStatus(interviewerId: Id.g3,
                            supervisorId: Id.g4,
                            timestamp: input.From.Date.AddHours(4),
                            status: InterviewExportedAction.Completed,
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(10)),
                    }), "2");

            var speedReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);

            // Act
            var speedByResponsibleReportView = speedReportFactory.GetReport(input);

            // Assert
            speedByResponsibleReportView.Headers.Second().ShouldEqual(reportStartDate.ToString("yyyy-MM-dd"));
        }
    }
}