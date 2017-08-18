using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    internal class when_load_overall_speed_report_for_interviewers_by_week_period : SpeedReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateSpeedBetweenStatusesByInterviewersReportInputModel(supervisorId: supervisorId, period: "w");
            var initialStatusChangeDate = input.From.Date.AddHours(-1);

            var user = Create.Entity.UserDocument(supervisorId: supervisorId);

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId, 
                questionnaireVersion: input.QuestionnaireVersion, statuses: new[]
                {
                    Create.Entity.InterviewCommentedStatus(timestamp: initialStatusChangeDate)
                }), "1");

            interviewStatusTimeSpans = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusTimeSpans.Store(
                Create.Entity.InterviewStatusTimeSpans(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    timeSpans: new[]
                    {
                        Create.Entity.TimeSpanBetweenStatuses(interviewerId: user.PublicKey,
                            supervisorId:supervisorId,
                            timestamp: initialStatusChangeDate, 
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(35)),
                          Create.Entity.TimeSpanBetweenStatuses(interviewerId: Guid.NewGuid(),
                            supervisorId:Guid.NewGuid(),
                            timestamp: initialStatusChangeDate,
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(35))
                    }), "2");

            quantityReportFactory = CreateSpeedReportFactory(interviewStatusTimeSpans: interviewStatusTimeSpans,
                interviewStatuses: interviewStatuses);
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Should().HaveCount(1);

        It should_return_first_row_with_positive_35_minutes_per_interview_at_first_period_and_null_minutes_per_interview_at_second = () =>
            result.Items.First().SpeedByPeriod.Should().Equal(new double?[] { 35, null });

        It should_return_first_row_with_positive_35_minutes_in_Total = () =>
            result.Items.First().Total.Should().Be(35);

        It should_return_first_row_with_positive_35_minutesin_Average = () =>
           result.Items.First().Average.Should().Be(35);

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedBetweenStatusesByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewStatusTimeSpans> interviewStatusTimeSpans;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
