using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_second_answer_given : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        Establish context = () =>
        {
            interviewStatuses = Create.Entity.InterviewSummary(statuses: new [] { Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId) } );
            denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();
        };

        Because of = () => result = denormalizer.Update(interviewStatuses, Create.PublishedEvent.TextQuestionAnswered(interviewId: Guid.NewGuid()));

        It should_record_first_answer_status =
            () => result.InterviewCommentedStatuses.Last().Status.ShouldEqual(InterviewExportedAction.FirstAnswerSet);

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary interviewStatuses;
        private static InterviewSummary result;
    }
}
