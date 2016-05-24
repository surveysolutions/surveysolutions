﻿using System;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using System.Linq;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    internal class when_interview_completed_event_received_after_restart : InterviewStatusTimeSpanDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewStatusTimeSpansStorage = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses =
                Create.Entity.InterviewStatuses(interviewid: interviewId, statuses:
                    new[]
                    {
                        Create.Entity.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.InterviewerAssigned),
                        Create.Entity.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.FirstAnswerSet),
                        Create.Entity.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.Completed),
                        Create.Entity.InterviewCommentedStatus(interviewId, status: InterviewExportedAction.Restarted)
                    });
            interviewStatusesStorage.Store(interviewStatuses, interviewId.FormatGuid());

            interviewStatusTimeSpansStorage.Store(
                Create.Entity.InterviewStatusTimeSpans(interviewId: interviewId.FormatGuid(),
                    timeSpans:
                        new[]
                        {
                            Create.Entity.TimeSpanBetweenStatuses(interviewerId: interviewId,
                                endStatus: InterviewExportedAction.Completed)
                        }), interviewId.FormatGuid());

            denormalizer = CreateInterviewStatusTimeSpanDenormalizer(statuses: interviewStatusesStorage, interviewCustomStatusTimestampStorage: interviewStatusTimeSpansStorage);
        };

        Because of = () => denormalizer.Handle(Create.PublishedEvent.InterviewCompleted(interviewId: interviewId));

        It should_contain_only_one_complete_record =
            () =>
                interviewStatusTimeSpansStorage.GetById(interviewId.FormatGuid())
                    .TimeSpansBetweenStatuses.Count(ts=>ts.EndStatus== InterviewExportedAction.Completed).ShouldEqual(1);

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