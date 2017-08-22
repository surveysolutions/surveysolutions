using System;
using System.Linq;
using Machine.Specifications;
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
    internal class when_load_speed_report_for_interviewers_by_week_period : SpeedReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, period: "w");

            var user = Create.Entity.UserDocument(supervisorId: supervisorId);

            interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(
                Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, 
                            supervisorId: supervisorId, timestamp: input.From.Date.AddHours(-15), timeSpanWithPreviousStatus: TimeSpan.FromMinutes(35)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey,
                            supervisorId: supervisorId, timestamp: input.From.Date.AddDays(1)),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed,
                            interviewerId: user.PublicKey, supervisorId: supervisorId, timestamp: input.From.Date.AddDays(5))
                    }), "2");

            quantityReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Count().ShouldEqual(1);

        It should_return_first_row_with_35_minutes_per_interview_at_first_period_and_null_minutes_per_interview_at_second = () =>
            result.Items.First().SpeedByPeriod.ShouldEqual(new double?[] { 35, null });

        It should_return_first_row_with_35_minutes_in_Total = () =>
            result.Items.First().Total.ShouldEqual(35);

        It should_return_first_row_with_35_minutesin_Average = () =>
           result.Items.First().Average.ShouldEqual(35);

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
