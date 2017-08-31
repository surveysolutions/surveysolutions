using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [TestOf(typeof(SpeedReportFactory))]
    internal class when_calculating_grand_averagel_with_supervisor_id_provided : SpeedReportFactoryTestContext
    {
        [Test]
        public void should_restrict_by_supervisorId()
        {
            var supervisorId = Id.g1;
            var input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, from: new DateTime(2010, 6, 10, 0, 0, 0, DateTimeKind.Utc), period: "d", columnCount: 1);

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            var averageForTargetSupervisor = 20;
            interviewStatuses.Store(
                Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId,
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
            var speedByResponsibleReportView = speedReportFactory.Load(input);

            // Assert
            Assert.That(speedByResponsibleReportView.TotalRow.SpeedByPeriod.First(), Is.EqualTo(averageForTargetSupervisor), 
                () => "Average should be calculated only for selected supervisor");
        }
    }
}