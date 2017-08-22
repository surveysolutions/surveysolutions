using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    internal class when_interview_approved_by_hq_event_recived : InterviewStatusTimeSpanDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewStatusTimeSpansStorage = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses =
                Create.Entity.InterviewStatuses(interviewid: interviewId, statuses:
                    new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId)
                    });
            interviewStatusesStorage.Store(interviewStatuses, interviewId.FormatGuid());
            denormalizer = CreateInterviewStatusTimeSpanDenormalizer(statuses: interviewStatusesStorage, interviewCustomStatusTimestampStorage: interviewStatusTimeSpansStorage);
        };

        Because of = () => denormalizer.Handle(Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId));

        It should_record_ApprovedByHeadquarter_as_end_status =
            () =>
                interviewStatusTimeSpansStorage.GetById(interviewId.FormatGuid())
                    .TimeSpansBetweenStatuses.First()
                    .EndStatus.ShouldEqual(InterviewExportedAction.ApprovedByHeadquarter);

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