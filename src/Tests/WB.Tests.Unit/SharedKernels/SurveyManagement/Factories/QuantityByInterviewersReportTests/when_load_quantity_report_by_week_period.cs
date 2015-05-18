using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityByInterviewersReportTests
{
    internal class when_load_quantity_report_by_week_period : QuantityByInterviewersReportTestContext
    {
        Establish context = () =>
        {
            input = CreateQuantityByInterviewersReportInputModel(supervisorId: supervisorId, period: "w");

            var user = Create.UserDocument(supervisorId: supervisorId);
            userDocuments = new TestInMemoryWriter<UserDocument>();
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
                            timestamp: input.From.Date.AddDays(1)),
                        Create.InterviewCommentedStatus(interviewerId: user.PublicKey,
                            timestamp: input.From.Date.AddDays(-15))
                    }), "2");

            _quantityReportFactory = CreateQuantityByInterviewersReport(userDocuments: userDocuments, interviewStatuses: interviewStatuses);
        };

        Because of = () =>
            result = _quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Count().ShouldEqual(1);

        It should_return_first_row_with_1_interview_at_first_period_and_zero_interviews_at_second = () =>
            result.Items.First().QuantityByPeriod.ShouldEqual(new long[] { 1, 0 });

        It should_return_first_row_with_1_in_Total = () =>
            result.Items.First().Total.ShouldEqual(1);

        It should_return_first_row_with_0_5_in_Average = () =>
           result.Items.First().Average.ShouldEqual(0.5);

        private static QuantityReportFactory _quantityReportFactory;
        private static QuantityByInterviewersReportInputModel input;
        private static QuantityByResponsibleReportView result;
        private static TestInMemoryWriter<UserDocument> userDocuments;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
