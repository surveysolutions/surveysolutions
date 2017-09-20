using System;
using System.Linq;
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
    class when_load_speed_report_for_interviewers_by_week_period_and_interview_completed_earlier_then_assigned : SpeedReportFactoryTestContext
    {
        [SetUp]
        public void Establish()
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, period: "w");

            var user = Create.Entity.UserDocument(supervisorId: supervisorId);

            interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, 
                            supervisorId: supervisorId, timestamp: input.From.Date.AddHours(-1), timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35))
                    }), "2");

            quantityReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);
        }

        [Test]
        public void should_return_one_row()
        {
            result = quantityReportFactory.Load(input);
            result.Items.Count().ShouldEqual(1);
        }

        [Test]
        public void should_return_first_row_with_positive_35_minutes_per_interview_at_first_period_and_null_minutes_per_interview_at_second()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().SpeedByPeriod.ShouldEqual(new double?[] {35});
        }

        [Test]
        public void should_return_first_row_with_positive_35_minutes_in_Total()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().Total.ShouldEqual(35);
        }

        [Test]
        public void should_return_first_row_with_positive_35_minutesin_Average()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().Average.ShouldEqual(35);
        }

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewSummary> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
