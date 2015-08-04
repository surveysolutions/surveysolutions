using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    internal class when_interview_completed_event_recived : InterviewStatusTimeSpanDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewStatusTimeSpansStorage=new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses =
                Create.InterviewStatuses(interviewid: interviewId, statuses:
                    new[]
                    {
                        Create.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.InterviewerAssigned),
                        Create.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.FirstAnswerSet)
                    });
            interviewStatusesStorage.Store(interviewStatuses, interviewId.FormatGuid());
            denormalizer = CreateInterviewStatusTimeSpanDenormalizer(statuses: interviewStatusesStorage, interviewCustomStatusTimestampStorage: interviewStatusTimeSpansStorage);
        };

        Because of = () => denormalizer.Handle(Create.InterviewCompletedEvent(interviewId: interviewId));

        It should_record_complete_as_end_status =
            () =>
                interviewStatusTimeSpansStorage.GetById(interviewId.FormatGuid())
                    .TimeSpansBetweenStatuses.First()
                    .EndStatus.ShouldEqual(InterviewExportedAction.Completed);

        It should_record_interviewer_assign_as_begin_status =
           () =>
               interviewStatusTimeSpansStorage.GetById(interviewId.FormatGuid())
                   .TimeSpansBetweenStatuses.First()
                   .BeginStatus.ShouldEqual(InterviewExportedAction.InterviewerAssigned);

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatusesStorage;
        private static TestInMemoryWriter<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewStatuses interviewStatuses;
    }
}
