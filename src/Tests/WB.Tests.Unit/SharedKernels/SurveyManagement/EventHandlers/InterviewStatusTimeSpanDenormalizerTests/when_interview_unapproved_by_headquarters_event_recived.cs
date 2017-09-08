﻿using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    [Subject(typeof(InterviewStatusTimeSpanDenormalizer))]
    internal class when_interview_unapproved_by_headquarters_event_recived
    {
        private Establish context = () =>
        {
            interviewSummary = Create.Entity.InterviewSummary(
                questionnaireId: questionnaireId,
                questionnaireVersion: 1,
                interviewId: interviewId,
                timeSpans: new[]
                {
                    Create.Entity.TimeSpanBetweenStatuses(interviewerId: userId,
                        timestamp: DateTime.Now.AddHours(1),
                        timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35))
                });
        
            
            denormalizer = Create.Service.InterviewStatusTimeSpanDenormalizer();
        };

        Because of = () => denormalizer.Update(interviewSummary, Create.PublishedEvent.UnapprovedByHeadquarters(interviewId: interviewId));

        It should_remove_ApprovedByHeadquarter_as_end_status = () =>
            interviewSummary.TimeSpansBetweenStatuses.Count(x => x.EndStatus == InterviewExportedAction.ApprovedByHeadquarter).ShouldEqual(0);

        

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid userId = Guid.Parse("31111111111111111111111111111111");
        private static InterviewSummary interviewSummary;

    }
}