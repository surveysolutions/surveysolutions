using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers
{
    internal class InterviewsFeedDenormalizer : BaseDenormalizer,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewApprovedByHQ>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRejectedByHQ>
    {
        private readonly IReadSideRepositoryWriter<InterviewFeedEntry> writer;
        private readonly IReadSideKeyValueStorage<InterviewData> interviews;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewInterviewSummaryes;

        public InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> writer,
            IReadSideKeyValueStorage<InterviewData> interviews, IReadSideRepositoryWriter<InterviewSummary> interviewInterviewSummaryes)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (interviews == null) throw new ArgumentNullException("interviews");
            this.writer = writer;
            this.interviews = interviews;
            this.interviewInterviewSummaryes = interviewInterviewSummaryes;
        }

        public override object[] Writers
        {
            get { return new object[] { writer}; }
        }

        public override object[] Readers
        {
            get { return new object[] { interviews, interviewInterviewSummaryes }; }
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            InterviewData interviewData = Monads.Maybe(() => this.interviews.GetById(evnt.EventSourceId));
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

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            InterviewData interviewData = this.interviews.GetById(evnt.EventSourceId);

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
                InterviewerId = supervisorId != responsibleId ? responsibleId : null
            }, evnt.EventIdentifier.FormatGuid());
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            string supervisorId = Monads.Maybe(() => this.interviewInterviewSummaryes.GetById(evnt.EventSourceId).TeamLeadId.FormatGuid());

            this.writer.Store(new InterviewFeedEntry
            {
                SupervisorId = supervisorId,
                EntryType = EntryType.InterviewUnassigned,
                Timestamp = evnt.EventTimeStamp,
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid()
            }, evnt.EventIdentifier);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            string supervisorId = Monads.Maybe(() => this.interviewInterviewSummaryes.GetById(evnt.EventSourceId).TeamLeadId.FormatGuid());

            this.writer.Store(new InterviewFeedEntry
            {
                SupervisorId = supervisorId,
                EntryType = EntryType.InterviewDeleted,
                Timestamp = evnt.EventTimeStamp,
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid()
            }, evnt.EventIdentifier);
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            string supervisorId = Monads.Maybe(() => this.interviewInterviewSummaryes.GetById(evnt.EventSourceId).TeamLeadId.FormatGuid());

            this.writer.Store(new InterviewFeedEntry
            {
                SupervisorId = supervisorId,
                EntryType = EntryType.InterviewUnassigned,
                Timestamp = evnt.EventTimeStamp,
                InterviewId = evnt.EventSourceId.FormatGuid(),
                EntryId = evnt.EventIdentifier.FormatGuid(),
                UserId = evnt.Payload.UserId.FormatGuid()
            }, evnt.EventIdentifier);
        }
    }
}