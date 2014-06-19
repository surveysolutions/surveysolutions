using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using Raven.Client.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers
{
    internal class InterviewsFeedDenormalizer : BaseDenormalizer,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewRejectedByHQ>
    {
        private readonly IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviews;

        public InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> writer,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviews)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (interviews == null) throw new ArgumentNullException("interviews");
            this.writer = writer;
            this.interviews = interviews;
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            InterviewData interviewData = Monads.Maybe(() => this.interviews.GetById(evnt.EventSourceId).Document);
            if(interviewData!= null && interviewData.CreatedOnClient)
                return;

            writer.Store(new InterviewFeedEntry
            {
                SupervisorId = evnt.Payload.SupervisorId.FormatGuid(),
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryType = EntryType.SupervisorAssigned,
                Timestamp = evnt.EventTimeStamp,
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid()
            }, evnt.EventIdentifier);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            InterviewData interviewData = this.interviews.GetById(evnt.EventSourceId).Document;

            this.writer.Store(new InterviewFeedEntry
            {
                SupervisorId = interviewData.SupervisorId.GetValueOrDefault().FormatGuid(),
                EntryType = EntryType.InterviewUnassigned,
                Timestamp = evnt.EventTimeStamp,
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid()
            }, evnt.EventIdentifier);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            InterviewData interviewData = this.interviews.GetById(evnt.EventSourceId).Document;

            string supervisorId = Monads.Maybe(() => interviewData.SupervisorId.FormatGuid());
            string responsibleId = interviewData.ResponsibleId.FormatGuid();
            this.writer.Store(new InterviewFeedEntry
            {
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryType = EntryType.InterviewRejected,
                SupervisorId = supervisorId,
                Timestamp = evnt.EventTimeStamp,
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid(),
                InterviewerId =  supervisorId != responsibleId ? responsibleId : null
            }, evnt.EventIdentifier.FormatGuid());
        }

        public override Type[] BuildsViews
        {
            get { return new[] { typeof (InterviewFeedEntry) }; }
        }
    }
}