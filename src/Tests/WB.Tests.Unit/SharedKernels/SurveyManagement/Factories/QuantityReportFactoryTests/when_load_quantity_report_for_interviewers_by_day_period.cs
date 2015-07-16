using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_interviewers_by_day_period : QuantityReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateQuantityByInterviewersReportInputModel(supervisorId: supervisorId);

            var user = Create.UserDocument(supervisorId: supervisorId);
            userDocuments=new TestInMemoryWriter<UserDocument>();
            userDocuments.Store(user, "1");

            interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(
                Create.InterviewStatuses(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.InterviewCommentedStatus(interviewerId: user.PublicKey,
                            timestamp: input.From.Date.AddHours(1)),
                        Create.InterviewCommentedStatus(interviewerId: user.PublicKey,
                            timestamp: input.From.Date.AddDays(2)),
                        Create.InterviewCommentedStatus(interviewerId: user.PublicKey,
                            timestamp: input.From.Date.AddDays(-2))
                    }), "2");

            quantityReportFactory = CreateQuantityReportFactory(userDocuments: userDocuments, interviewStatuses: interviewStatuses);
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Count().ShouldEqual(1);

        It should_return_first_row_with_1_interview_at_first_period_and_zero_interviews_at_second = () =>
            result.Items.First().QuantityByPeriod.ShouldEqual(new long[]{1,0});

        It should_return_first_row_with_1_in_Total = () =>
            result.Items.First().Total.ShouldEqual(1);

        It should_return_first_row_with_0_5_in_Average = () =>
           result.Items.First().Average.ShouldEqual(0.5);

        private static QuantityReportFactory quantityReportFactory;
        private static QuantityByInterviewersReportInputModel input;
        private static QuantityByResponsibleReportView result;
        private static TestInMemoryWriter<UserDocument> userDocuments;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
