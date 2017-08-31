using System;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_supervisors_by_month_period : QuantityReportFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            input = CreateQuantityBySupervisorsReportInputModel(period: "m");

            var user = Guid.NewGuid();

            interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(
                Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(supervisorId: user,
                            timestamp: input.From.Date.AddHours(-1)),
                        Create.Entity.InterviewCommentedStatus(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(-2)),
                        Create.Entity.InterviewCommentedStatus(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(2))
                    }), "2");

            quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = quantityReportFactory.Load(input);

        [NUnit.Framework.Test] public void should_return_one_row () =>
            result.Items.Should().HaveCount(1);

        [NUnit.Framework.Test] public void should_return_first_row_with_0_interview_at_first_period_and_1_interviews_at_second () =>
            result.Items.First().QuantityByPeriod.Should().Equal(0, 1);

        [NUnit.Framework.Test] public void should_return_first_row_with_1_in_Total () =>
            result.Items.First().Total.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_row_with_0_5_in_Average () =>
           result.Items.First().Average.Should().Be(0.5);

        [NUnit.Framework.Test] public void should_return_total_row_with_0_interview_at_first_period_and_1_interviews_at_second () =>
          result.TotalRow.QuantityByPeriod.Should().Equal(0, 1);

        [NUnit.Framework.Test] public void should_return_total_row_with_1_in_Total () =>
            result.TotalRow.Total.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_total_row_with_0_5_in_Average () =>
           result.TotalRow.Average.Should().Be(0.5);

        private static QuantityReportFactory quantityReportFactory;
        private static QuantityBySupervisorsReportInputModel input;
        private static QuantityByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatuses;
    }
}
