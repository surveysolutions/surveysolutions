using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Tests.Interviews.InterviewsFeedDenormalizerTests
{
    [Subject(typeof(InterviewsFeedDenormalizer))]
    internal class when_supervisor_assigned_on_interview_on_hq
    {
        Establish context = () =>
        {
            userId = Guid.NewGuid();
            supervisorId = Guid.NewGuid();
            supervisorAssignedEvent = Create.PublishedEvent(Guid.NewGuid(), new SupervisorAssigned(userId, supervisorId));

            writer = Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>();
            var interviews = Substitute.For<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>();
            interviews.GetById(supervisorAssignedEvent.EventSourceId.FormatGuid())
                .Returns(new ViewWithSequence<InterviewData>(new InterviewData { SupervisorId = supervisorId, CreatedOnClient = false}, 1));

            denormalizer = Create.InterviewsFeedDenormalizer(writer, interviews);
        };

        private Because of = () => denormalizer.Handle(supervisorAssignedEvent);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<InterviewFeedEntry>(x => 
                    x.InterviewId == supervisorAssignedEvent.EventSourceId.FormatGuid() &&
                    x.EntryType == EntryType.SupervisorAssigned &&
                    x.Timestamp == supervisorAssignedEvent.EventTimeStamp &&
                    x.SupervisorId == supervisorId.FormatGuid() &&
                    x.UserId == userId.FormatGuid() &&
                    x.EntryId == supervisorAssignedEvent.EventIdentifier.FormatGuid()),

                supervisorAssignedEvent.EventIdentifier.FormatGuid());

        static InterviewsFeedDenormalizer denormalizer;
        static IPublishedEvent<SupervisorAssigned> supervisorAssignedEvent;
        static IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private static Guid supervisorId;
        private static Guid userId;
    }
}
