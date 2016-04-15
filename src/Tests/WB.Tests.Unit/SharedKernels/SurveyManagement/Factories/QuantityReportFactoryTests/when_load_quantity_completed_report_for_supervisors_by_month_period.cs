using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_completed_report_for_supervisors_by_month_period : QuantityReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateQuantityBySupervisorsReportInputModel(period: "m");
            input.InterviewStatuses = new[] {InterviewExportedAction.Completed};
            var user = Guid.NewGuid();

            interviewStatusTimeSpansStorage = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusTimeSpansStorage.Store(
                Create.InterviewStatusTimeSpans(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    timeSpans: new[]
                    {
                        Create.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddHours(1), endStatus:InterviewExportedAction.Completed),
                        Create.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(2), endStatus:InterviewExportedAction.Completed),
                        Create.TimeSpanBetweenStatuses(supervisorId: user,
                            timestamp: input.From.Date.AddMonths(-2), endStatus:InterviewExportedAction.Completed)
                    }), "2");

            quantityReportFactory = CreateQuantityReportFactory(interviewStatusTimeSpansStorage: interviewStatusTimeSpansStorage);
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Count().ShouldEqual(1);

        It should_return_first_row_with_1_interview_at_first_period_and_zero_interviews_at_second = () =>
            result.Items.First().QuantityByPeriod.ShouldEqual(new long[] { 1, 0 });

        It should_return_first_row_with_1_in_Total = () =>
            result.Items.First().Total.ShouldEqual(1);

        It should_return_first_row_with_0_5_in_Average = () =>
           result.Items.First().Average.ShouldEqual(0.5);

        It should_return_total_row_with_1_interview_at_first_period_and_zero_interviews_at_second = () =>
          result.TotalRow.QuantityByPeriod.ShouldEqual(new long[] { 1, 0 });

        It should_return_total_row_with_1_in_Total = () =>
            result.TotalRow.Total.ShouldEqual(1);

        It should_return_total_row_with_0_5_in_Average = () =>
           result.TotalRow.Average.ShouldEqual(0.5);

        private static QuantityReportFactory quantityReportFactory;
        private static QuantityBySupervisorsReportInputModel input;
        private static QuantityByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}