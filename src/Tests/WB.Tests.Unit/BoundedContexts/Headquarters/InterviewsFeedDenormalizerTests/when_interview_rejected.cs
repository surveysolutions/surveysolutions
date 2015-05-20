using System;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Tests.Unit.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewsFeedDenormalizerTests
{
    [Subject(typeof (InterviewsFeedDenormalizer))]
    public class when_interview_rejected
    {
        Establish context = () =>
        {
            interviewRejectedEvent = new InterviewRejectedByHQ(Guid.NewGuid(), "comment").ToPublishedEvent(Guid.NewGuid());

            writer = Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>();
            var interviews = Substitute.For<IReadSideKeyValueStorage<InterviewData>>();
            supervisorId = Guid.NewGuid();
            interviews.GetById(interviewRejectedEvent.EventSourceId.FormatGuid())
                .Returns(new InterviewData{SupervisorId = supervisorId });


            denormalizer = Create.InterviewsFeedDenormalizer(writer, interviews);
        };

        private Because of = () => denormalizer.Handle(interviewRejectedEvent);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<InterviewFeedEntry>(x => x.InterviewId == interviewRejectedEvent.EventSourceId.FormatGuid() &&
                    x.EntryType == EntryType.InterviewRejected &&
                    x.Timestamp == interviewRejectedEvent.EventTimeStamp &&
                    x.SupervisorId == supervisorId.FormatGuid()&&
                    x.EntryId == interviewRejectedEvent.EventIdentifier.FormatGuid()), 
                
                interviewRejectedEvent.EventIdentifier.FormatGuid());
        
        static InterviewsFeedDenormalizer denormalizer;
        static IPublishedEvent<InterviewRejectedByHQ> interviewRejectedEvent;
        static IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private static Guid supervisorId;
    }
}

