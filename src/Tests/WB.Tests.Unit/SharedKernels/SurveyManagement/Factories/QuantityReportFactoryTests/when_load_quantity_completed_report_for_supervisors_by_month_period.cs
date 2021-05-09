using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_completed_report_for_supervisors_by_month_period : QuantityReportFactoryTestContext
    {
        private QuantityBySupervisorsReportInputModel input;

        private QuantityReportFactory quantityReportFactory;
        private QuantityByResponsibleReportView result;

        [OneTimeSetUp]
        public void Context()
        {
            input = CreateQuantityBySupervisorsReportInputModel("m");
            input.InterviewStatuses = new[] {InterviewExportedAction.Completed};
            var user = Guid.NewGuid();

            var interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(timestamp: input.From.AddMonths(-2)),
                    },
                    timeSpans: new[]
                    {
                        Create.Entity.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddHours(-1), endStatus: InterviewExportedAction.Completed),
                        Create.Entity.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(-2), endStatus: InterviewExportedAction.Completed),
                        Create.Entity.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(2), endStatus: InterviewExportedAction.Completed)
                    }), "2");

            quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);

            result = quantityReportFactory.Load(input);
        }

        [Test]
        public void should_return_one_row()
        {
            result.Items.Should().HaveCount(1);
        }

        [Test]
        public void should_return_first_row_with_0_interview_at_first_period_and_1_interviews_at_second()
        {
            result.Items.First().QuantityByPeriod.Should().Equal(0, 1);
        }

        [Test]
        public void should_return_first_row_with_1_in_Total()
        {
            Assert.That(result.Items.First().Total, Is.EqualTo(1));
        }

        [Test]
        public void should_return_first_row_with_0_5_in_Average()
        {
            Assert.That(result.Items.First().Average, Is.EqualTo(0.5));
        }

        [Test]
        public void should_return_total_row_with_0_interview_at_first_period_and_1_interviews_at_second()
        {
            result.TotalRow.QuantityByPeriod.Should().Equal(0, 1);
        }

        [Test]
        public void should_return_total_row_with_1_in_Total()
        {
            Assert.That(result.TotalRow.Total, Is.EqualTo(1));
        }

        [Test]
        public void should_return_total_row_with_0_5_in_Average()
        {
            Assert.That(result.TotalRow.Average, Is.EqualTo(0.5));
        }
    }
}
