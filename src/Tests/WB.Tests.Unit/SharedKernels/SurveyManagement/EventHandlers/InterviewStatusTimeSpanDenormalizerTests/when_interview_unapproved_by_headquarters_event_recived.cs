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
    internal class when_interview_unapproved_by_headquarters_event_recived : InterviewStatusTimeSpanDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewStatusTimeSpansStorage = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            
            interviewStatusTimeSpansStorage.Store(
                Create.Entity.InterviewStatusTimeSpans(
                    questionnaireId: questionnaireId,
                    questionnaireVersion: 1,
                    interviewId : interviewId.FormatGuid(),
                    timeSpans: new[]
                    {
                        Create.Entity.TimeSpanBetweenStatuses(interviewerId: userId,
                            timestamp: DateTime.Now.AddHours(1),
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35))
                    })
        , interviewId.FormatGuid());
            
            denormalizer = CreateInterviewStatusTimeSpanDenormalizer(interviewCustomStatusTimestampStorage: interviewStatusTimeSpansStorage);
        };

        Because of = () => denormalizer.Handle(Create.PublishedEvent.UnapprovedByHeadquarters(interviewId: interviewId));

        It should_remove_ApprovedByHeadquarter_as_end_status =
            () =>
                interviewStatusTimeSpansStorage.GetById(interviewId.FormatGuid())
                    .TimeSpansBetweenStatuses.Count(x => x.EndStatus == InterviewExportedAction.ApprovedByHeadquarter)
                    .ShouldEqual(0);

        

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static TestInMemoryWriter<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static Guid userId = Guid.Parse("31111111111111111111111111111111");
        
    }
}