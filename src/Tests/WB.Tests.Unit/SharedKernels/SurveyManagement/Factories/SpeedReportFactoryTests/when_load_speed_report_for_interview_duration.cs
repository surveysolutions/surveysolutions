using System;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [TestFixture]
    internal class when_load_speed_report_for_interview_duration : SpeedReportFactoryTestContext
    {
        [Test]
        public void should_return_one_record()
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, period: "w",
                from: DateTime.Now);

            var user = Create.Entity.UserDocument(supervisorId: supervisorId);

            interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, supervisorId: supervisorId,
                            timestamp: input.From.Date.AddDays(-3),
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(35)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, supervisorId: supervisorId,
                            timestamp: input.From.Date.AddDays(-2),
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(25)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, supervisorId: supervisorId,
                            timestamp: input.From.Date.AddDays(1), timeSpanWithPreviousStatus: TimeSpan.FromMinutes(15))
                    }), "2");

            quantityReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);


            //Act
            result = quantityReportFactory.Load(input);

            //Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().SpeedByPeriod.ShouldEqual(new double?[] { 35 });
            result.Items.First().Total.ShouldEqual(35);
            result.Items.First().Average.ShouldEqual(35);
        }
        
        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewSummary> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
