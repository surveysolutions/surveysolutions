using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_interviewers_where_one_out_of_three_periods_is_in_future : QuantityReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateQuantityByInterviewersReportInputModel(supervisorId: supervisorId, columnCount: 3,
                from: now.AddDays(-1));

            quantityReportFactory = CreateQuantityReportFactory();
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);
        
        It should_return_2_periods= () =>
            result.DateTimeRanges.Length.ShouldEqual(2);

        It should_return_today_as_a_period = () =>
            result.DateTimeRanges[1].From.Date.ShouldEqual(now.Date);

        It should_return_yesterday_as_a_period = () =>
            result.DateTimeRanges[0].From.Date.ShouldEqual(now.Date.AddDays(-1));

        private static QuantityReportFactory quantityReportFactory;
        private static QuantityByInterviewersReportInputModel input;
        private static QuantityByResponsibleReportView result;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static DateTime now = DateTime.Now;
    }
}