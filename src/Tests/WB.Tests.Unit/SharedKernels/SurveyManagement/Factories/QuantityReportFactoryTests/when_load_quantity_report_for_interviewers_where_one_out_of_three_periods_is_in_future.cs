using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_interviewers_where_one_out_of_three_periods_is_in_future : QuantityReportFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            input = CreateQuantityByInterviewersReportInputModel(supervisorId: supervisorId, columnCount: 3,
                from: now.AddDays(1));

            quantityReportFactory = CreateQuantityReportFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = quantityReportFactory.Load(input);

        [NUnit.Framework.Test] public void should_return_2_periods() =>
            result.DateTimeRanges.Should().HaveCount(2);

        [NUnit.Framework.Test] public void should_return_today_as_a_period () =>
            result.DateTimeRanges[1].From.Date.Should().Be(now.Date);

        [NUnit.Framework.Test] public void should_return_yesterday_as_a_period () =>
            result.DateTimeRanges[0].From.Date.Should().Be(now.Date.AddDays(-1));

        private QuantityReportFactory quantityReportFactory;
        private QuantityByInterviewersReportInputModel input;
        private QuantityByResponsibleReportView result;
        private readonly Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private DateTime now = DateTime.Now;
    }
}