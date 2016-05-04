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
    internal class when_interview_approved_by_hq
    {
        Establish context = () =>
        {
            userId = Guid.NewGuid();
            supervisorId = Guid.NewGuid();
            interviewApprovedByHQ = new InterviewApprovedByHQ(userId,"").ToPublishedEvent(Guid.NewGuid());

            writer = Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>();
            var interviews = Substitute.For<IReadSideRepositoryWriter<InterviewSummary>>();
            interviews.GetById(interviewApprovedByHQ.EventSourceId.FormatGuid())
                .Returns(new InterviewSummary() { TeamLeadId = supervisorId, WasCreatedOnClient = false });

            denormalizer = Create.InterviewsFeedDenormalizer(writer, interviewSummaryRepository: interviews);
        };

        private Because of = () => denormalizer.Handle(interviewApprovedByHQ);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<InterviewFeedEntry>(x =>
                    x.InterviewId == interviewApprovedByHQ.EventSourceId.FormatGuid() &&
                    x.EntryType == EntryType.InterviewUnassigned &&
                    x.Timestamp == interviewApprovedByHQ.EventTimeStamp &&
                    x.SupervisorId == supervisorId.FormatGuid() &&
                    x.UserId == userId.FormatGuid() &&
                    x.EntryId == interviewApprovedByHQ.EventIdentifier.FormatGuid()),

                interviewApprovedByHQ.EventIdentifier.FormatGuid());

        static InterviewsFeedDenormalizer denormalizer;
        static IPublishedEvent<InterviewApprovedByHQ> interviewApprovedByHQ;
        static IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private static Guid supervisorId;
        private static Guid userId;
    }
}
