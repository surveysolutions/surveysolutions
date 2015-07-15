using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_first_answer_given : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        Establish context = () =>
        {
            interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses = Create.InterviewStatuses(statuses:Create.InterviewCommentedStatus(interviewId,status:InterviewExportedAction.InterviewerAssigned));
            denormalizer = CreateDenormalizer(interviewStatuses: interviewStatusesStorage);
        };

        Because of = () => result = denormalizer.Update(interviewStatuses, Create.TextQuestionAnsweredEvent(interviewId: Guid.NewGuid()));

        It should_record_first_answer_status =
            () => result.InterviewCommentedStatuses.Last().Status.ShouldEqual(InterviewExportedAction.FirstAnswerSet);

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatusesStorage;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewStatuses interviewStatuses;
        private static InterviewStatuses result;
    }
}
