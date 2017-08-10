using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    internal class when_load_speed_report_for_interviewers_where_one_out_of_three_periods_is_in_future : SpeedReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, columnCount: 3,
                from: now.AddDays(-1));

            quantityReportFactory = CreateSpeedReportFactory();
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_2_periods = () =>
            result.DateTimeRanges.Length.ShouldEqual(2);

        It should_return_today_as_a_period = () =>
            result.DateTimeRanges.First().From.Date.ShouldEqual(now.Date);

        It should_return_yesterday_as_a_period = () =>
            result.DateTimeRanges.Second().From.Date.ShouldEqual(now.Date.AddDays(-1));

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static DateTime now = DateTime.Now;
    }
}