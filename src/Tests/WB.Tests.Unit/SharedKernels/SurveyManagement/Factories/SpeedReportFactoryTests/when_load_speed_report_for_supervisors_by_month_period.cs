using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [TestFixture]
    internal class when_load_speed_report_for_supervisors_by_month_period : SpeedReportFactoryTestContext
    {
        [SetUp]
        public void Establish()
        {
            input = CreateSpeedBySupervisorsReportInputModel(period: "m");

            var user = Create.Entity.UserDocument();

            interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            supervisorId: user.PublicKey, 
                            timestamp: input.From.Date.AddDays(-33), timeSpanWithPreviousStatus: TimeSpan.FromMinutes(35)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            supervisorId: user.PublicKey, timestamp: input.From.Date.AddDays(-11)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            supervisorId: user.PublicKey, timestamp: input.From.Date.AddDays(-2))
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
        public void should_return_first_row_with_35_minutes_per_interview_at_first_period_and_null_minutes_per_interview_at_second()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().SpeedByPeriod.ShouldEqual(new double?[] {35, null});
        }

        [Test]
        public void should_return_first_row_with_35_minutes_in_Total()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().Total.ShouldEqual(35);
        }

        [Test]
        public void should_return_first_row_with_35_minutes_in_Average()
        {
            result = quantityReportFactory.Load(input);
            result.Items.First().Average.ShouldEqual(35);
        }

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedBySupervisorsReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewSummary> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
