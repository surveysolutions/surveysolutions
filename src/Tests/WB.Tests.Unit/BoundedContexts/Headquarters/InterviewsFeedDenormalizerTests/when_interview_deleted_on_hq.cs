using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Tests.Unit.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewsFeedDenormalizerTests
{
    [Subject(typeof(InterviewsFeedDenormalizer))]
    internal class when_interview_deleted_on_hq
    {
        Establish context = () =>
        {
            userId = Guid.NewGuid();
            supervisorId = Guid.NewGuid();
            interviewDeletedEvent = new InterviewDeleted(userId).ToPublishedEvent(Guid.NewGuid());

            writer = Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>();
            var interviews = Substitute.For<IReadSideRepositoryWriter<InterviewSummary>>();
            interviews.GetById(interviewDeletedEvent.EventSourceId.FormatGuid())
                .Returns(new InterviewSummary() { TeamLeadId = supervisorId, WasCreatedOnClient = false });

            denormalizer = Create.InterviewsFeedDenormalizer(writer, interviewSummaryRepository: interviews);
        };

        private Because of = () => denormalizer.Handle(interviewDeletedEvent);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<InterviewFeedEntry>(x =>
                    x.InterviewId == interviewDeletedEvent.EventSourceId.FormatGuid() &&
                    x.EntryType == EntryType.InterviewUnassigned &&
                    x.Timestamp == interviewDeletedEvent.EventTimeStamp &&
                    x.SupervisorId == supervisorId.FormatGuid() &&
                    x.UserId == userId.FormatGuid() &&
                    x.EntryId == interviewDeletedEvent.EventIdentifier.FormatGuid()),

                interviewDeletedEvent.EventIdentifier.FormatGuid());

        static InterviewsFeedDenormalizer denormalizer;
        static IPublishedEvent<InterviewDeleted> interviewDeletedEvent;
        static IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private static Guid supervisorId;
        private static Guid userId;
    }
}
